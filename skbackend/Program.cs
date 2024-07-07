using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed((host) => true);
    });
});

// Load environment variables
DotNetEnv.Env.Load();

// Configure HttpClient for RetailContextPlugin
builder.Services.AddHttpClient("ConfiguredClient").ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    UseCookies = false,
    UseProxy = false,
    AllowAutoRedirect = true,
    AutomaticDecompression = DecompressionMethods.All,
    EnableMultipleHttp2Connections = true,
    MaxConnectionsPerServer = 75,
    PooledConnectionLifetime = TimeSpan.FromMinutes(3),
    SslOptions = new SslClientAuthenticationOptions
    {
        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
    }
})
.ConfigureHttpClient(client =>
{
    client.DefaultRequestVersion = HttpVersion.Version20;
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
});

// Add RetailContextPlugin
builder.Services.AddSingleton<RetailContextPlugin>();

// Add Semantic Kernel services
builder.Services.AddSingleton<Kernel>(sp =>
{
    IKernelBuilder kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: "gpt-4o",
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
        );

    var httpClient = new HttpClient();
    var retailContextPlugin = sp.GetRequiredService<RetailContextPlugin>();
    kernelBuilder.Plugins.AddFromObject(retailContextPlugin, "RetailContext");
    kernelBuilder.Plugins.AddFromObject(new SalesHelpPlugin(), "SalesHelp");

    return kernelBuilder.Build();
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    return kernel.GetRequiredService<IChatCompletionService>();
});

builder.Services.AddSingleton<KernelFunction>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    return kernel.CreateFunctionFromPrompt(@"
        Based on the conversation history and the user's latest message, generate a concise search query to find relevant information from approved real-time sources. Only generate the search term. Do not append 'site:domain.com' to the query. 
        As an alternative search query term, specific URL maybe be sent as the search query. Do this only in cases where a specific Page URL is being discussed and real-time information from it can be beneficial.
        The query should be specific and relevant to the user's question. If you can't generate a meaningful query, return the user's input unchanged.
        You may consider appending the Locale or Market in the query to further target results.
        Do not include date information in the query unless explicitly needed to answer the question.
        History: {{$history}}
        Latest message: {{$input}}
        Generated search query:");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub");
    endpoints.MapControllers();
});

app.Run();

[EnableCors]
public class ChatHub : Hub
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly KernelFunction _searchQueryFunction;
    private static readonly ConcurrentDictionary<string, ChatHistory> _chatHistories = new();
    private static readonly ConcurrentDictionary<string, string> _currentTopics = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _retailContextCaches = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _sentSystemMessages = new();

    public ChatHub(Kernel kernel, IChatCompletionService chatCompletionService, KernelFunction searchQueryFunction)
    {
        _kernel = kernel;
        _chatCompletionService = chatCompletionService;
        _searchQueryFunction = searchQueryFunction;
    }

    public async Task InitializeChat()
    {
        string connectionId = Context.ConnectionId;
        if (_chatHistories.TryGetValue(connectionId, out var existingHistory))
        {
            // Chat already initialized for this connection
            return;
        }

        var chatHistory = new ChatHistory();
        string systemMessage = await File.ReadAllTextAsync("SystemMessage.txt");
        chatHistory.AddSystemMessage(systemMessage);
        
        if (_chatHistories.TryAdd(connectionId, chatHistory))
        {
            _currentTopics[connectionId] = "";
            _retailContextCaches[connectionId] = new Dictionary<string, string>();

            // Send the welcome message
            string brand = GetBrandFromSystemMessage(systemMessage);
            string welcomeMessage = $"Hello, welcome to **{brand}**. I can help with product questions, support, learning, and anything else related to {brand}. How can I help you?";
            await Clients.Caller.SendAsync("ReceiveInitialMessage", welcomeMessage);

            await Clients.Caller.SendAsync("ChatInitialized");
        }
    }

    private string GetBrandFromSystemMessage(string systemMessage)
    {
        var brandMatch = System.Text.RegularExpressions.Regex.Match(systemMessage, @"\[Your Brand\]=(\w+)");
        return brandMatch.Success ? brandMatch.Groups[1].Value : "Unknown";
    }

    public async Task SendMessage(string message)
    {
        string connectionId = Context.ConnectionId;
        var chatHistory = _chatHistories[connectionId];
        chatHistory.AddUserMessage(message);

        try
        {
            var pluginToUse = await ClassifyUserInput(message, connectionId);
            await SendSystemMessageOnce(connectionId, "PluginSelection", $"Selected plugin: {pluginToUse}");

            switch (pluginToUse)
            {
                case "SalesHelp":
                    await HandleSalesHelpQuery(message, connectionId);
                    break;
                case "RetailContext":
                    await HandleRetailContextQuery(message, connectionId);
                    break;
                default:
                    await HandleGeneralQuery(message, connectionId);
                    break;
            }

            // Classify the current topic and send it to the client
            var currentTopic = await ClassifyTopic(message, connectionId);
            _currentTopics[connectionId] = currentTopic;
            await SendSystemMessageOnce(connectionId, "CurrentTopic", $"Current topic: {currentTopic}");
        }
        catch (HttpOperationException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await Clients.Caller.SendAsync("ErrorMessage", "OpenAI quota exceeded. Please try again later.");
            Console.Error.WriteLine($"OpenAI quota exceeded: {ex}");
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", "An error occurred while processing your message.");
            Console.Error.WriteLine($"Error in SendMessage: {ex}");
        }
    }

    private async Task<string> ClassifyUserInput(string userInput, string connectionId)
    {
        var chatHistory = _chatHistories[connectionId];
        var recentHistory = string.Join("\n", chatHistory.TakeLast(5).Select(m => $"{m.Role}: {m.Content}"));

        var promptToClassify = @$"Classify the following user input into one of these categories:
        1. 'SalesHelp' if the user is explicitly asking for 'human' assistance, needs 'human' help with orders, returns, or account-specific issues. This should only be used for transferring to the right human sales help. Do not use this for general help if a human isn't specifically mentioned. If a transfer to human sales help has just occurred only use SalesHelp if a human is specifically being asked for in the latest user message.
        2. 'RetailContext' if the query will benefit from real-time information from approved and official sources. This may be for specific product information, feature details, pricing, policy details, support topics, deal information, troubleshooting articles, facts about the company or directors, investor relations, etc. Nearly any topic can benefit from real-time content so this should be used frequently. If 'RetailContext' has already been leveraged for the same specific topic and a follow up question is being asked, consider using 'General' for any follow up questions about the same topic unless a new query is needed.
        3. 'General' for any other type of query, general chitchat, or follow up questions on an existing topic where 'RetailContext' already retrieved real-time information that can be reused. 

        If a transfer to human sales help has just occurred, classify as 'General' or 'RetailContext' unless the user explicitly asks for human assistance again in their most recent message.

        Current mode: {(_currentTopics[connectionId] == "SalesHelp" ? "SalesHelp" : "Not SalesHelp")}

        Recent conversation history:
        {recentHistory}

        Respond with only the category name.

        User Input: {userInput}

        Category:";

        var classificationResult = await _chatCompletionService.GetChatMessageContentAsync(promptToClassify);
        return classificationResult.Content?.Trim() ?? "General";
    }

    private async Task<string> ClassifyTopic(string userInput, string connectionId)
    {
        var chatHistory = _chatHistories[connectionId];
        var recentHistory = string.Join("\n", chatHistory.TakeLast(5).Select(m => $"{m.Role}: {m.Content}"));

        var currentTopic = _currentTopics[connectionId];

        var promptToClassify = @$"
    Current topic: {currentTopic}

    Recent conversation history:
    {recentHistory}

    User Input: {userInput}

    Based on the current topic, recent conversation history, and the latest user input, determine the most relevant and specific topic for the current state of the conversation. If the topic has changed, provide the new topic. If it hasn't changed significantly, return the current topic. Only return the topic and without any commentary.

    Topic:";

        var classificationResult = await _chatCompletionService.GetChatMessageContentAsync(promptToClassify);
        return classificationResult.Content?.Trim() ?? currentTopic;
    }

    private async Task HandleSalesHelpQuery(string userInput, string connectionId)
    {
        var chatHistory = _chatHistories[connectionId];
        var salesHelpPrompt = await File.ReadAllTextAsync("SalesHelpPrompt.txt");
        var salesHelpHistory = new ChatHistory(salesHelpPrompt);

        foreach (var message in chatHistory.TakeLast(5))
        {
            salesHelpHistory.AddMessage(message.Role, message.Content);
        }

        salesHelpHistory.AddUserMessage(userInput);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var response = await _chatCompletionService.GetChatMessageContentAsync(salesHelpHistory, executionSettings);
        var salesInfo = response.Content ?? string.Empty;

        var (shouldTransfer, transferInfo) = ShouldTransfer(salesInfo);
        if (shouldTransfer)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "Transferring you now...");
            await SendSystemMessageOnce(connectionId, "Transfer", $"Transfer initiated to {transferInfo.location}");
            chatHistory.AddAssistantMessage("Transferring you now...");
            _currentTopics[connectionId] = "";
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", salesInfo);
            chatHistory.AddAssistantMessage(salesInfo);
            _currentTopics[connectionId] = "SalesHelp";
        }
    }

    private async Task HandleRetailContextQuery(string userInput, string connectionId)
    {
        var chatHistory = _chatHistories[connectionId];
        var currentTopic = _currentTopics[connectionId];
        var retailContextCache = _retailContextCaches[connectionId];

        await SendSystemMessageOnce(connectionId, "SearchQuery", "Generating search query...");
        var searchQueryResult = await RetryOperationAsync(() => 
            _kernel.InvokeAsync(_searchQueryFunction, new KernelArguments 
            { 
                ["history"] = string.Join("\n", chatHistory.Select(m => $"{m.Role}: {m.Content}")),
                ["input"] = userInput 
            }));
        var searchQuery = searchQueryResult.GetValue<string>();

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            await SendSystemMessageOnce(connectionId, "SearchQueryWarning", "Warning: Generated search query is empty. Using user input as fallback.");
            searchQuery = userInput;
        }

        await SendSystemMessageOnce(connectionId, "SearchQueryResult", $"Generated search query: {searchQuery}");

        string retailContextJson;
        if (retailContextCache.TryGetValue(currentTopic, out var cachedContext))
        {
            await SendSystemMessageOnce(connectionId, "CacheUse", "Using cached RetailContext data");
            retailContextJson = cachedContext;
        }
        else
        {
            await SendSystemMessageOnce(connectionId, "RetailContext", "Calling RetailContextPlugin...");
            var retailContextFunction = _kernel.Plugins["RetailContext"]["GetRetailContext"];
            var retailContextResult = await RetryOperationAsync(() => _kernel.InvokeAsync(retailContextFunction, new KernelArguments { ["query"] = searchQuery }));
            retailContextJson = retailContextResult.GetValue<string>();

            retailContextCache[currentTopic] = retailContextJson;
        }

        await SendSystemMessageOnce(connectionId, "RetailContextResponse", $"RetailContextPlugin response received. Length: {retailContextJson?.Length ?? 0}");

        if (string.IsNullOrWhiteSpace(retailContextJson))
        {
            await SendSystemMessageOnce(connectionId, "RetailContextWarning", "Warning: RetailContext is empty. There might be an issue with the API call.");
            await HandleGeneralQuery(userInput, connectionId);
            return;
        }

        try
        {
            var retailContextData = JsonSerializer.Deserialize<List<PageOutput>>(retailContextJson);

            if (retailContextData != null && retailContextData.Any())
            {
                await SendSystemMessageOnce(connectionId, "RetailContextSources", "RetailContext sources:");
                foreach (var content in retailContextData)
                {
                    if (!string.IsNullOrWhiteSpace(content.PageTitle) && !string.IsNullOrWhiteSpace(content.PageUrl))
                    {
                        await SendSystemMessageOnce(connectionId, "RetailContextSource", $"- {content.PageTitle}: {content.PageUrl}");
                    }
                }

                // Use the entire JSON structure as the combinedContext
                var combinedContext = retailContextJson;

                string prompt = $@"Answer the question using Real-time Context. The Real-time Context is a JSON array where each object represents a webpage and contains the following fields:
                - PageTitle: The title of the webpage
                - PageUrl: The URL of the webpage
                - ParsedPageText: The main content of the webpage
                - Hyperlinks: An array of important links on the page, each with a Url and Text field

                Use this structured data to provide a comprehensive and accurate answer. When referencing information, mention the source URL.

                Here is the **Real-time Context:** {combinedContext}

                **User Question:** {userInput}

                **Response:**";

                await GenerateAndStreamResponse(prompt, connectionId);
            }
            else
            {
                await SendSystemMessageOnce(connectionId, "NoRetailContextData", "Warning: No retail context data found. Falling back to general query handling.");
                await HandleGeneralQuery(userInput, connectionId);
            }
        }
        catch (JsonException ex)
        {
            await SendSystemMessageOnce(connectionId, "JsonParseError", $"Error parsing JSON response: {ex.Message}");
            await SendSystemMessageOnce(connectionId, "RawJson", $"Raw JSON: {retailContextJson}");
            await HandleGeneralQuery(userInput, connectionId);
        }
    }

    private async Task HandleGeneralQuery(string userInput, string connectionId)
    {
        string prompt = $"Please provide a friendly and helpful response to the following user input, keeping in mind your role and guidelines and the conversation history: {userInput}";
        await GenerateAndStreamResponse(prompt, connectionId);
    }

    private async Task GenerateAndStreamResponse(string prompt, string connectionId)
    {
        var chatHistory = _chatHistories[connectionId];
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var fullHistory = new ChatHistory(chatHistory);
        fullHistory.AddUserMessage(prompt);

        var response = new StringBuilder();
        var isFirstChunk = true;

        await foreach (var content in _chatCompletionService.GetStreamingChatMessageContentsAsync(
            fullHistory,
            executionSettings: executionSettings,
            kernel: _kernel))
        {
            if (content.Content is not null)
            {
                if (isFirstChunk)
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", content.Content);
                    isFirstChunk = false;
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveMessageStream", content.Content);
                }
                response.Append(content.Content);
            }
        }

        string responseString = response.ToString();
        if (!string.IsNullOrEmpty(responseString))
        {
            chatHistory.AddAssistantMessage(responseString);
        }
    }

    private async Task<T> RetryOperationAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int delayMilliseconds = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (HttpOperationException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (i == maxRetries - 1) throw;
                await Clients.Caller.SendAsync("ReceiveMessage", "I'm processing a lot of requests right now. Let me take a moment to catch up...");
                await Task.Delay(delayMilliseconds * (i + 1));
            }
            catch (JsonException ex)
            {
                await SendSystemMessageOnce(Context.ConnectionId, "JsonError", $"Error parsing JSON response: {ex.Message}");
                if (i == maxRetries - 1) throw;
                await Task.Delay(delayMilliseconds * (i + 1));
            }
        }

        throw new Exception("Operation failed after maximum retries");
    }

    private (bool shouldTransfer, TransferInfo transferInfo) ShouldTransfer(string salesInfo)
    {
        if (salesInfo.Contains("Transferring you now..."))
        {
            try
            {
                int jsonStart = salesInfo.IndexOf('{');
                if (jsonStart != -1)
                {
                    string jsonPart = salesInfo.Substring(jsonStart);
                    var transferInfo = JsonSerializer.Deserialize<TransferInfo>(jsonPart);
                    return (true, transferInfo ?? new TransferInfo { action = "transfer", location = "unknown" });
                }
            }
            catch (JsonException)
            {
                // JSON parsing failed, return default values
            }
        }
        return (false, new TransferInfo());
    }

    private async Task SendSystemMessageOnce(string connectionId, string messageType, string message)
    {
        string key = $"{messageType}:{message}";
        if (_sentSystemMessages.TryGetValue(connectionId, out var sentMessages))
        {
            if (sentMessages.Add(key))
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", message);
            }
        }
        else
        {
            var newSet = new HashSet<string> { key };
            if (_sentSystemMessages.TryAdd(connectionId, newSet))
            {
                await Clients.Caller.SendAsync("ReceiveSystemMessage", message);
            }
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        _sentSystemMessages.TryRemove(connectionId, out _);
        _chatHistories.TryRemove(connectionId, out _);
        _currentTopics.TryRemove(connectionId, out _);
        _retailContextCaches.TryRemove(connectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}

public class TransferInfo
{
    public string action { get; set; } = "transfer";
    public string location { get; set; } = "unknown";
}

[ApiController]
[Route("api/[controller]")]
public class BrandController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        string systemMessage = System.IO.File.ReadAllText("SystemMessage.txt");
        var brandMatch = System.Text.RegularExpressions.Regex.Match(systemMessage, @"\[Your Brand\]=(\w+)");
        var brand = brandMatch.Success ? brandMatch.Groups[1].Value : "Unknown";

        return Ok(new { brand });
    }
}