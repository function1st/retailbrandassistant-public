
# Retail Brand Assistant

This project is a configurable Retail Brand Assistant built using **Microsoft Semantic Kernel** for AI Orchestration, Open AI gpt-4o Large Language Model, and **Microsoft Bing CustomSearch** for brand-specific search results. Using Retail Brand Assistant allows brands to rapidly test and deploy a fully functioning AI Assistant for Customers that uses real-time page information to answer questions with very high accuracy and consistency.

| When retail brand experiences fail like this... | Retail Brand Assistant succeeds... |
|:----------------------------------------------------------------------------:|:---------------------------------:|
| <img src="https://github.com/function1st/retailbrandassistant-public/assets/129132283/43d368a7-f62c-4e0d-9bd8-76f1e2fe00a1" alt="Screenshot" width="450"> | <img src="https://github.com/function1st/retailbrandassistant-public/assets/129132283/ec8f3794-be92-43b3-9c0b-7ee35f23fc4e" alt="Screenshot" width="250"> |

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)
- OpenAI API key
- [Bing CustomSearch instance and key](https://www.customsearch.ai/)

## Bing CustomSearch Configuration - Skip if already configured
#### Bing CustomSearch Prerequisites
1. **Microsoft Azure Account**: You need an Azure account to use Bing Custom Search. Sign up at [Azure](https://azure.microsoft.com/).

#### Step 1: Create a Bing Custom Search Resource on Azure

1. Go to the [Azure portal](https://portal.azure.com/).
2. Click on **Create a resource**.
3. Search for **Bing Custom Search** and select it.
4. Click **Create**.
5. Fill in the required details:
   - Subscription: Your Azure subscription.
   - Resource group: Create a new one or use an existing one.
   - Name: A unique name for your resource.
   - Pricing tier: Choose a pricing tier that suits your needs.
   - Resource region: Select the region.
6. Click **Review + create** and then **Create**.

#### Step 2: Access the Bing Custom Search UI

1. After the resource is created, go to the resource page.
2. Under **Resource Management**, click on **Custom Search Portal**. This will take you to the Bing Custom Search UI.

#### Step 3: Add a Domain to Your Custom Search

1. In the Custom Search UI, click on **Create new custom search instance**.
2. Provide a name and description for your custom search instance.
3. Click **Create**.
4. Click on **Add a site to your search instance**.
5. Enter the domain or specific URLs you want to include in your custom search.
6. Click **Add**.

#### Step 4: Customize Your Search Instance

1. After adding the domain, you can further customize your search instance by specifying:
   - **Domains and URLs**: Add multiple domains or specific URLs.
   - **Pinned Results**: Pin specific pages to the top of the search results.
   - **Block Certain Sites**: Exclude certain domains or URLs from search results.
  
      <br><img src="https://github.com/function1st/retailbrandassistant-public/assets/129132283/a7a80af8-9dc0-42f6-bbab-59f1b5ddd261" alt="Screenshot" width="450">

#### Step 5: Publish Your Custom Search Instance

1. Once you have added the necessary domains and customized your search instance, click on **Publish**.
2. Confirm the details and click **Publish** again to make your custom search instance live.
   
      <img width="450" alt="bingcs-publishhelp" src="https://github.com/function1st/retailbrandassistant-public/assets/129132283/dfaee6c6-372a-45a3-8c1b-74bd9a79088b">
      <br><img width="450" alt="bingcs-publish" src="https://github.com/function1st/retailbrandassistant-public/assets/129132283/214a2e9b-d0c1-4efd-afb4-03574df9f98b">


#### Step 6: Retrieve API Key and Endpoint

1. Go back to the Azure portal, under your Bing Custom Search resource.
2. Under **Keys and Endpoint**, copy one of the keys and the endpoint URL. You will need these if you decide to access the custom search programmatically.

That's it! You've set up and used Bing Custom Search with the Custom Search UI, added domains, and published your custom search instance.

## Retail Brand Assistant Setup
### Project Structure
```
project_root/
├── reactfrontend/
│   ├── public/
│   ├── scripts/
│   ├── src/
│   │   ├── App.css
│   │   ├── App.js
│   ├── package-lock.json
│   └── package.json
├── skbackend/
│   ├── Program.cs
│   ├── RetailContextPlugin.cs
│   ├── SalesHelpPlugin.cs
│   ├── SalesHelpPrompt.txt
│   └── SystemMessage.txt
├── README.md
├── setup.sh
├── start.sh
├── startbackend.sh
├── startfrontend.sh
└── stop.sh
```

1. Clone the repository:
   ```bash
   git clone https://github.com/function1st/retailbrandassistant-public.git
   cd retailbrandassistant-public
   ```

2. Make the scripts executable and run the setup script:
   ```bash
   chmod +x setup.sh start.sh startbackend.sh startfrontend.sh stop.sh
   ./setup.sh
   ```

3. Follow the prompts to enter your brand name, language, market, and locale.

4. Edit the `.env` file and add your API keys:
   ```bash
   OPENAI_API_KEY=your_openai_api_key_here
   RETAIL_CONTEXT_API_KEY=your_retail_context_api_key_here
   CUSTOM_CONFIG_ID=your_bing_custom_config_id_here
   ```

## Usage

1. To start the application:
   ```bash
   ./start.sh
   ```
   This will automatically update the current date in the system message and start both the backend and frontend.

2. Interact with the Retail Brand Assistant through the browser.

3. To stop the application:
   ```bash
   ./stop.sh
   ```

## Configuration

The setup script will help you configure the basic brand information and site information. If you need to change this information later, you can either run the setup script again or manually edit the `SystemMessage.txt` file.

The current date is automatically updated each time you start the application using the start script.

You can customize the Retail Brand Assistant by modifying the following files:

- `SystemMessage.txt`: Contains the system message that defines the agent's behavior and how it uses real-time context.
- `Program.cs`: Main Semantic Kernel logic and chat handling. In addition, the prompt logic for Plug-in orchrestration as well as Topic classification is found here.
- `RetailContextPlugin.cs`: Plugin for retrieving real-time retail context. Leverages Bing CustomSearch to retrieve top results, uses Open AI to pick the most relevant pages that answer the question, visits those pages to parse imporant information from them, and then provides that context back to Semantic Kernel for leveraging with Open AI.
- `SalesHelpPlugin.cs`: Sample plugin for handling sales-related queries based on the SalesHelpPromt.txt instructions.
- `SalesHelpPrompt.txt`: Contains a sample prompt for handling a hypothetical human transfer scenario for sales-related queries.

**IMPORTANT:** This project is intended for educational purposes only and should not be used for production workloads. The creators and contributors of this project accept no responsibility for its functionality, reliability, or any consequences arising from its use.

# Orchestration and Conversation Topic Classifers
Summary of Program.cs and Detailed Overview of Conversation and Topic Classification Focus

## Table of Contents
1. [Introduction](#introduction)
2. [Semantic Kernel Configuration](#semantic-kernel-configuration)
3. [Classifier for Plugin/Function Selection](#classifier-for-pluginfunction-selection)
4. [Conversation Topic Classifier](#conversation-topic-classifier)
5. [Main Message Handling](#main-message-handling)
6. [Detailed Walkthrough of Conversation Flows](#detailed-walkthrough-of-conversation-flows)
   6.1 [SalesHelp Flow](#saleshelp-flow)
   6.2 [RetailContext Flow](#retailcontext-flow)
   6.3 [General Flow](#general-flow)
7. [Usage of Topic Classification Across Flows](#usage-of-topic-classification-across-flows)
8. [Conclusion](#conclusion)

## 1. Introduction

This document provides a comprehensive walkthrough of the key components in Program.cs, focusing on the Semantic Kernel configuration, the main conversation flows, and the crucial role of Conversation Topic Classification throughout the system.

## 2. Semantic Kernel Configuration

The Semantic Kernel is configured as follows:

```csharp
builder.Services.AddSingleton<Kernel>(sp =>
{
    IKernelBuilder kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: "gpt-4o",
            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")!
        );

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
```

Key points:
- The Kernel uses the "gpt-4o" model for chat completion, which is the latest model from OpenAI as of this project.
- Two plugins are added: RetailContextPlugin and SalesHelpPlugin (on top of a 'General' catch all plugin)
- A separate IChatCompletionService is registered, extracted from the Kernel.

## 3. Classifier for Plugin/Function Selection

The system uses a classifier to determine which plugin or function to use for each user input:

```csharp
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
```

This classifier returns one of three categories: SalesHelp, RetailContext, or General, based on the user's input and recent conversation history.

## 4. Conversation Topic Classifier

The Conversation Topic Classifier is a crucial component that determines the current topic of the conversation:

```csharp
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
```

This classifier:
- Takes into account the current topic, recent conversation history, and the latest user input.
- Determines if the topic has changed or remained the same.
- Returns either the current topic (if unchanged) or a new topic.

The topic classification is used to:
- Maintain context across multiple user inputs
- Determine when to fetch new retail context or reuse existing information
- Provide more relevant and coherent responses

## 5. Main Message Handling

The main message handling is implemented in the `SendMessage` method:

```csharp
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
    catch (Exception ex)
    {
        // Error handling
    }
}
```

This method orchestrates the entire conversation flow, from classifying the user input to handling the query and updating the conversation topic.

## 6. Detailed Walkthrough of Conversation Flows

### 6.1 SalesHelp Flow

```csharp
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
        _currentTopics[connectionId] = "";  // Reset the topic after transfer
    }
    else
    {
        await Clients.Caller.SendAsync("ReceiveMessage", salesInfo);
        chatHistory.AddAssistantMessage(salesInfo);
        _currentTopics[connectionId] = "SalesHelp";  // Maintain SalesHelp topic
    }
}
```

The SalesHelp flow handles queries requiring human assistance. It either provides AI-generated assistance or initiates a transfer to a human agent when necessary. SalesHelp is a simple sample implementation of how a transfer plug-in might work, providing proof of concept for a full implementation.

### 6.2 RetailContext Flow

```csharp
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

    string prompt = $@"Answer the question using Real-time Context. The Real-time Context is a JSON array where each object represents a webpage and contains the following fields:
    - PageTitle: The title of the webpage
    - PageUrl: The URL of the webpage
    - ParsedPageText: The main content of the webpage
    - Hyperlinks: An array of important links on the page, each with a Url and Text field

    Use this structured data to provide a comprehensive and accurate answer. When referencing information, mention the source URL.

    Here is the **Real-time Context:** {retailContextJson}

    **User Question:** {userInput}

    **Response:**";

    await GenerateAndStreamResponse(prompt, connectionId);
}
```

The RetailContext flow handles queries requiring real-time information. It generates a search query, fetches or reuses retail context data, and uses this data to generate a response. Don't let the name "RetailContext" limit the applicability as this will work for any site and brand.

### 6.3 General Flow

```csharp
private async Task HandleGeneralQuery(string userInput, string connectionId)
{
    var currentTopic = _currentTopics[connectionId];
    string prompt = $"The current conversation topic is: {currentTopic}. Please provide a friendly and helpful response to the following user input, keeping in mind your role, guidelines, the conversation history, and the current topic: {userInput}";
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
```

The General flow handles queries that **don't require specific retail context or human assistance.** It generates a response based on the current topic and conversation history, highly leveraging the system prompt and general LLM knowledge.

## 7. Usage of Topic Classification Across Flows

The topic classification plays a crucial role in all conversation flows:

1. **In the main message handling:**
   - After each query is handled, the topic is reclassified:
     ```csharp
     var currentTopic = await ClassifyTopic(message, connectionId);
     _currentTopics[connectionId] = currentTopic;
     ```
   - This ensures the topic is updated based on the latest interaction.

2. **In the SalesHelp flow:**
   - The topic is set to "SalesHelp" if no transfer occurs:
     ```csharp
     _currentTopics[connectionId] = "SalesHelp";
     ```
   - The topic is reset if a transfer occurs:
     ```csharp
     _currentTopics[connectionId] = "";
     ```
   - This allows the system to maintain context within a SalesHelp interaction and reset when appropriate.

3. **In the RetailContext flow:**
   - The current topic is used as a key for caching retail context:
     ```csharp
     if (retailContextCache.TryGetValue(currentTopic, out var cachedContext))
     {
         // Use cached context
     }
     else
     {
         // Fetch new context and cache it
         retailContextCache[currentTopic] = retailContextJson;
     }
     ```
   - This allows for efficient reuse of relevant context when the topic hasn't changed.
   - When the topic changes, new context is fetched, ensuring up-to-date information.

4. **In the General flow:**
   - The current topic is included in the prompt:
     ```csharp
     string prompt = $"The current conversation topic is: {currentTopic}. ...";
     ```
   - This ensures the AI's response is contextually relevant to the ongoing conversation.

5. **Continuous topic tracking:**
   - The topic classification result is stored and used across multiple turns of the conversation:
     ```csharp
     _currentTopics[connectionId] = currentTopic;
     ```
   - This persistent tracking allows the system to maintain context over time, influencing:
     a. Which plugin is selected for handling queries (via the `ClassifyUserInput` method).
     b. How retail context is cached and reused.
     c. The context provided to the AI for generating responses.

6. **Influence on plugin selection:**
   - The current topic influences the `ClassifyUserInput` method:
     ```csharp
     Current mode: {(_currentTopics[connectionId] == "SalesHelp" ? "SalesHelp" : "Not SalesHelp")}
     ```
   - This helps maintain continuity in SalesHelp interactions and prevents unnecessary switches between modes.

7. **Adaptive response generation:**
   - In all flows, the current topic helps tailor the AI's responses to the ongoing conversation context.
   - For example, in the RetailContext flow, the topic influences the search query generation:
     ```csharp
     var searchQueryResult = await RetryOperationAsync(() => 
         _kernel.InvokeAsync(_searchQueryFunction, new KernelArguments 
         { 
             ["history"] = string.Join("\n", chatHistory.Select(m => $"{m.Role}: {m.Content}")),
             ["input"] = userInput 
         }));
     ```
   - The search query function can use the conversation history, which implicitly includes the topic progression, to generate more relevant queries.

8. **Topic change detection:**
   - The `ClassifyTopic` method is designed to detect both subtle and significant topic changes:
     ```csharp
     Based on the current topic, recent conversation history, and the latest user input, determine the most relevant and specific topic for the current state of the conversation. If the topic has changed, provide the new topic. If it hasn't changed significantly, return the current topic.
     ```
   - This allows the system to maintain topic continuity when appropriate, but also adapt when the conversation shifts.

9. **Client-side awareness:**
   - The current topic is sent to the client:
     ```csharp
     await SendSystemMessageOnce(connectionId, "CurrentTopic", $"Current topic: {currentTopic}");
     ```
   - This can be used for UI/UX purposes, such as displaying the current context to the user or adjusting the interface based on the conversation topic.

By leveraging the topic classification throughout the system, the conversation maintains coherence and relevance across multiple interactions, adapting its behavior based on the evolving focus of the conversation. This dynamic topic tracking enables the system to provide more contextually appropriate responses, efficiently manage resources through caching, and create a more natural, context-aware conversation flow.

## 8. Conclusion

The Program.cs file implements a sophisticated conversational AI system that leverages the Semantic Kernel, custom plugins, and intelligent classifiers to provide context-aware, real-time responses to user queries. The system's core strengths include:

1. **Adaptive conversation handling:** The use of plugin/function classification allows the system to route queries to the most appropriate handler (SalesHelp, RetailContext, or General).

2. **Dynamic topic tracking:** The Conversation Topic Classifier maintains context across multiple interactions, enabling more coherent and relevant responses.

3. **Efficient information retrieval:** The RetailContext flow combines real-time information fetching with intelligent caching based on conversation topics.

4. **Seamless human integration:** The SalesHelp flow provides a sample of how to orchestrate AI assistance while also facilitating smooth transfers to human agents when necessary.

5. **Contextual response generation:** All flows leverage the current topic and conversation history to generate more relevant and coherent responses.


 
# Real-time Context Plugin - RetailContextPlugin Code Walkthrough

## Table of Contents
1. [Introduction](#introduction)
2. [Class Overview](#class-overview)
3. [Detailed Component Analysis](#detailed-component-analysis)
3.1 [Class Definition and Dependencies](#class-definition-and-dependencies)
3.2 [Main Function: GetRetailContext](#main-function-getretailcontext)
3.3 [Bing Search Integration](#bing-search-integration)
3.4 [OpenAI Integration](#openai-integration)
3.5 [URL Fetching and Parsing](#url-fetching-and-parsing)
3.6 [Link Extraction](#link-extraction)
4. [Key Features and Optimizations](#key-features-and-optimizations)
5. [Integration with Program.cs](#integration-with-programcs)

## 1. Introduction

The `RetailContextPlugin` is a combination search+fetch+parse function which orchestrates real-timem context enrichment for the user's question.

## 2. Class Overview

The `RetailContextPlugin` class orchestrates a series of operations:

1. Initiating a Bing search based on user queries (from search terms generated by Semantic Kernel)
2. Leveraging OpenAI to refine and select the most relevant search results from the Bing response
3. Efficiently fetching and parsing content from selected URLs
4. Extracting and scoring important information and links from the parsed content
5. Providing the content back as context for answering the user's initial question.

## 3. Detailed Component Analysis

### 3.1 Class Definition and Dependencies

```csharp
public class RetailContextPlugin
{
    private static readonly Random random = new Random();
    private static readonly string[] USER_AGENTS = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15",
        // ... more user agents ...
    };
    private readonly IHttpClientFactory _httpClientFactory;

    public RetailContextPlugin(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
}
```

#### Detailed Explanation:

- `Random random`: This static field is used to randomly select user agents, adding unpredictability to the plugin's web requests. This randomness helps in mimicking human-like behavior and avoiding detection by anti-bot systems.

- `USER_AGENTS`: This array contains a variety of user agent strings representing different browsers and operating systems. User agents are crucial in web scraping as they inform websites about the "browser" making the request. By rotating through different user agents, the plugin can:
  1. Avoid being easily identified as a bot
  2. Mimic requests from various browsers and devices
  3. Potentially bypass simple anti-scraping measures
  **Note:** This method is not advised beyond exploration and educational purposes. Any brand owners that use RetailContextPlugin on their own site should implement approved methods for this activity instead.

- `IHttpClientFactory _httpClientFactory`: This is injected through the constructor and used to create `HttpClient` instances. Using `IHttpClientFactory` offers several benefits:
  1. It manages the lifetime of `HttpClientHandler` instances, improving performance and reliability
  2. It allows for easy configuration of `HttpClient` instances with predefined settings
  3. It facilitates better resource management and DNS refresh compared to creating new `HttpClient` instances for each request

### 3.2 Main Function: GetRetailContext

```csharp
[Microsoft.SemanticKernel.KernelFunction]
public async Task<string> GetRetailContext(string query)
{
    // Get search results from Bing
    Console.WriteLine("Starting Bing search...");
    var bingStopwatch = Stopwatch.StartNew();
    var bingResults = await GetBingSearchResults(query);
    bingStopwatch.Stop();
    Console.WriteLine($"Bing search completed in {bingStopwatch.Elapsed.TotalSeconds:F2} seconds");
    Console.WriteLine($"Number of Bing results: {bingResults.Count}");

    if (bingResults.Count == 0)
    {
        Console.WriteLine("No results from Bing search. Exiting.");
        return "[]";
    }

    // Get URLs from OpenAI based on Bing results
    Console.WriteLine("Starting OpenAI call...");
    var openAiStopwatch = Stopwatch.StartNew();
    var urls = await GetUrlsFromOpenAI(query, bingResults);
    openAiStopwatch.Stop();
    Console.WriteLine($"OpenAI call completed in {openAiStopwatch.Elapsed.TotalSeconds:F2} seconds");
    Console.WriteLine($"Number of URLs returned from OpenAI: {urls.Count}");

    if (urls.Count == 0)
    {
        Console.WriteLine("No URLs were returned from OpenAI. Exiting.");
        return "[]";
    }

    // Parallel DNS resolution and connection establishment
    await Task.WhenAll(urls.Select(PrefetchDnsAndConnect));

    var stopwatch = Stopwatch.StartNew();

    var tasks = urls.Select(url => FetchAndParseUrlAsync(url, CancellationToken.None));
    var results = await Task.WhenAll(tasks);

    stopwatch.Stop();

    var output = new List<PageOutput>();

    for (int i = 0; i < urls.Count; i++)
    {
        var pageOutput = new PageOutput
        {
            PageTitle = results[i].PageTitle,
            PageUrl = results[i].FinalUrl,
            ParsedPageText = results[i].Content,
            Hyperlinks = results[i].Hyperlinks.Select(h => new Hyperlink { Url = h.Url, Text = h.Text }).ToList()
        };

        output.Add(pageOutput);
    }

    var jsonOutput = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine($"Total parallel operation time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
    return jsonOutput;
}
```

#### Detailed Explanation:

This method is the core of the `RetailContextPlugin`. It's decorated with `[Microsoft.SemanticKernel.KernelFunction]`, indicating it can be invoked by the Semantic Kernel, which is an AI orchestration framework.

The function performs several key operations:

1. **Bing Search**:
   - It starts by calling `GetBingSearchResults(query)` to fetch search results from Bing.
   - The time taken for this operation is measured and logged.
   - If no results are found, the function exits early, returning an empty JSON array.

2. **OpenAI URL Selection**:
   - It then calls `GetUrlsFromOpenAI(query, bingResults)` to use OpenAI's capabilities to select the most relevant URLs from the Bing search results.
   - Again, the time taken is measured and logged.
   - If no URLs are selected, the function exits early.

3. **Parallel DNS Resolution and Connection Establishment**:
   - Before fetching the content, it calls `PrefetchDnsAndConnect` for each URL in parallel.
   - This step optimizes the subsequent content fetching by resolving DNS and establishing connections in advance.

4. **Parallel Content Fetching and Parsing**:
   - It creates a task for each URL to fetch and parse its content (`FetchAndParseUrlAsync`).
   - These tasks are executed in parallel using `Task.WhenAll`, which waits for all tasks to complete.

5. **Result Compilation**:
   - The results from each URL are compiled into a list of `PageOutput` objects.
   - Each `PageOutput` contains the page title, URL, parsed text content, and important hyperlinks.

6. **JSON Serialization**:
   - The final output is serialized to JSON format with indentation for readability.

7. **Performance Logging**:
   - The total time taken for the parallel operations is logged.

Some key C# features leveraged:
- Asynchronous programming with `async/await`
- Parallel task execution
- LINQ for data transformation
- JSON serialization
- Extensive use of `Task` and `Task<T>` for managing asynchronous operations

The use of parallel processing for DNS resolution, connection establishment, and content fetching significantly improves the efficiency of the plugin, especially when dealing with multiple URLs.

### 3.3 Bing Search Integration

```csharp
private async Task<List<BingSearchResult>> GetBingSearchResults(string query)
{
    var bingApiKey = Environment.GetEnvironmentVariable("BING_SUBSCRIPTION_KEY");
    var customConfigId = Environment.GetEnvironmentVariable("CUSTOM_CONFIG_ID");
    if (string.IsNullOrEmpty(bingApiKey) || string.IsNullOrEmpty(customConfigId))
    {
        throw new InvalidOperationException("Bing API key or Custom Config ID is not set in the environment variables.");
    }

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", bingApiKey);

    var encodedQuery = Uri.EscapeDataString(query);
    var url = $"https://api.bing.microsoft.com/v7.0/custom/search?q={encodedQuery}&customconfig={customConfigId}&mkt=en-US&count=5";

    try
    {
        Console.WriteLine($"Sending request to Bing API: {url}");
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Bing API Response: {content}");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        var searchResponse = JsonSerializer.Deserialize<BingSearchResponse>(content, options);

        if (searchResponse?.WebPages?.Value == null || searchResponse.WebPages.Value.Count == 0)
        {
            Console.WriteLine("No results returned from Bing. Using fallback URLs.");
            return new List<BingSearchResult>
            {
                new BingSearchResult { Name = "Example Deal Site", Url = "https://www.example.com/deals", Snippet = "Find great deals here" },
                new BingSearchResult { Name = "Sample Discount Store", Url = "https://www.sample.com/discounts", Snippet = "Discounts on various items" }
            };
        }

        return searchResponse.WebPages.Value;
    }
    catch (HttpRequestException e)
    {
        Console.WriteLine($"Error calling Bing API: {e.Message}");
        return new List<BingSearchResult>();
    }
    catch (JsonException e)
    {
        Console.WriteLine($"Error deserializing Bing API response: {e.Message}");
        return new List<BingSearchResult>();
    }
}
```

#### Detailed Explanation:

This method is responsible for fetching search results from the Bing API for Bing CustomSearch. Bing CustomSearch allows for easy UI-driven configuration of website domains to scope to, block, etc. to refine search results for a given brand.
Here's a breakdown of its functionality:

1. **Environment Variable Retrieval**:
   - It retrieves the Bing API key and custom configuration ID (Bing CustomSearch) from environment variables.
   - This approach keeps sensitive information out of the source code, enhancing security.
   - If either is missing, it throws an `InvalidOperationException`, preventing the method from proceeding with invalid credentials.

2. **HTTP Client Setup**:
   - A new `HttpClient` is created for this request.
   - The Bing API key is added to the request headers using the `Ocp-Apim-Subscription-Key` header, which is required for authentication with the Bing API.

3. **Query Encoding and URL Construction**:
   - The query generated by SemanticKernel is URL-encoded to ensure it's properly formatted for the HTTP request.
   - The API URL is constructed with several parameters:
     - `q`: The encoded search query
     - `customconfig`: The custom configuration ID for the Bing Custom Search instance
     - `mkt`: Set to "en-US" for English (United States) results
     - `count`: Limited to 5 results to balance between variety and API usage

4. **API Request and Response Handling**:
   - The request is sent to the Bing API asynchronously.
   - The response status is checked with `EnsureSuccessStatusCode()`, which throws an exception for non-success status codes.
   - The response content is read as a string.

5. **JSON Deserialization**:
   - The response is deserialized into a `BingSearchResponse` object.
   - `PropertyNameCaseInsensitive = true` is set to allow for case-insensitive property matching, increasing the robustness of the deserialization process.

6. **Fallback Mechanism**:
   - If no results are returned from Bing, a fallback list of example results is provided. Optionally, these could be changed to specific known pages on a brand's site to help with this scenario.

7. **Error Handling**:
   - Specific catch blocks are used for `HttpRequestException` (network-related errors) and `JsonException` (deserialization errors).
   - Errors are logged, and an empty list is returned in case of failures, allowing the plugin to gracefully handle errors without crashing.

8. **Logging**:
   - Extensive console logging is implemented throughout the method, providing visibility into the API request, response, and any issues that occur.

The use of Bing's Custom Search API allows for more targeted and relevant results compared to a general web search or even a "site:domain.com" scoped search, which really improves the experience.

### 3.4 OpenAI Integration

```csharp
private async Task<List<string>> GetUrlsFromOpenAI(string query, List<BingSearchResult> bingResults)
{
    var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrEmpty(openAiApiKey))
    {
        throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set.");
    }

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");

    var systemMessage = "Please select up to 3 URLs from the provided list that best answer the query. Provide response in compact JSON format without newlines or extra spaces. Example format: {\"selected_urls\":[{\"url\":\"URL1\"},{\"url\":\"URL2\"}]}";        
    var userMessage = $"Query: {query}\n\nSearch Results:\n" + string.Join("\n", bingResults.Select((r, i) => $"{i + 1}. Title: {r.Name}\n   URL: {r.Url}\n   Snippet: {r.Snippet}"));

    var request = new
    {
        model = "gpt-4o",
        messages = new[]
        {
            new { role = "system", content = systemMessage },
            new { role = "user", content = userMessage }
        },
        max_tokens = 150
    };

    var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"OpenAI API Response: {responseBody}");

    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

    var content = jsonResponse.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    Console.WriteLine($"Extracted content: {content}");

    if (content == null)
    {
        Console.WriteLine("No content extracted from OpenAI response.");
        return new List<string>();
    }

    // Strip JSON code block markers if they exist
    content = content.Trim();
    if (content.StartsWith("```json"))
    {
        content = content.Substring(7);
    }
    if (content.EndsWith("```"))
    {
        content = content.Substring(0, content.Length - 3);
    }
    content = content.Trim();

    Console.WriteLine($"Cleaned content: {content}");

    try
    {
        var urlsJson = JsonSerializer.Deserialize<JsonElement>(content);
        var urls = urlsJson.GetProperty("selected_urls").EnumerateArray()
            .Select(u => u.GetProperty("url").GetString())
            .Where(u => !string.IsNullOrEmpty(u))
            .ToList();

        Console.WriteLine($"Extracted URLs: {string.Join(", ", urls)}");

        return urls!;
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Error parsing JSON from OpenAI response: {ex.Message}");
        return new List<string>();
    }
}
```

#### Detailed Explanation:

This method integrates with OpenAI's API to refine the search results obtained from Bing. Here's a breakdown of its functionality:

1. **API Key Retrieval and Validation**:
   - The OpenAI API key is retrieved from an environment variable, ensuring security.
   - If the key is missing, an `InvalidOperationException` is thrown, preventing unauthorized API calls.

2. **HTTP Client Setup**:
   - A new `HttpClient` is created for the OpenAI API request.
   - The API key is added to the Authorization header using the Bearer token scheme, which is required for OpenAI API authentication.

3. **Request Preparation**:
   - A system message is defined, instructing OpenAI on how to process the input and format the output, and asking for "up to three" URLs, giving some variability in the number that is most suited to the user's question.
   - A user message is constructed, combining the original query and the Bing search results.
   - The request object is created with the following properties:
     - `model`: Set to "gpt-4o", likely a custom or fine-tuned version of GPT-4 for this specific task.
     - `messages`: An array containing the system and user messages.
     - `max_tokens`: Limited to 150 to ensure a concise response.

4. **API Request and Response Handling**:
   - The request is sent to the OpenAI API asynchronously using `PostAsJsonAsync`.
   - The response status is checked with `EnsureSuccessStatusCode()`.
   - The response content is read as a string and logged.

5. **Response Parsing**:
   - The JSON response is deserialized into a `JsonElement` for flexible parsing.
   - The content of the AI's response is extracted from the JSON structure.

6. **Content Cleaning**:
   - The method handles cases where the AI might wrap the JSON in code block markers (```json), removing them if present.
   - This increases the robustness of the parsing process, handling slight variations in AI output format.

7. **URL Extraction**:
   - The cleaned content is deserialized as JSON.
   - URLs are extracted from the "selected_urls" property, filtered to remove any null or empty strings.

8. **Error Handling and Logging**:
   - Extensive logging is implemented throughout the method.
   - A specific catch block for `JsonException` handles any JSON parsing errors, returning an empty list in case of failure.

The use of OpenAI to refine search results adds an intelligent layer to the plugin. It adds about 1,000 - 1,500ms to the overall process, but highly improves the relevance of the selected URLs based on the context of the user's query, and limits the amount of content that needs to be fetched and parsed in the next section.

### 3.5 URL Fetching and Parsing

```csharp
private async Task<(string Content, List<(string Url, string Text)> Hyperlinks, string FinalUrl, string PageTitle)> FetchAndParseUrlAsync(string url, CancellationToken cancellationToken)
{
    try
    {
        using var httpClient = _httpClientFactory.CreateClient("ConfiguredClient");
        httpClient.Timeout = TimeSpan.FromSeconds(10); // Set a timeout
        var userAgent = USER_AGENTS[random.Next(USER_AGENTS.Length)];
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
        var baseUri = new Uri(finalUrl);

        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        // Extract page title
        var pageTitle = doc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim() ?? "No Title";

        // Extract important links
        var hyperlinks = ExtractImportantLinks(doc, baseUri);

        // Remove header, footer, and other non-content elements
        var nodesToRemove = doc.DocumentNode.SelectNodes("//header | //footer | //nav | //aside | //script | //style");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        var mainContent = doc.DocumentNode.SelectSingleNode("//main") ?? doc.DocumentNode.SelectSingleNode("//body");
        var textNodes = mainContent?.SelectNodes(".//text()[not(ancestor::script) and not(ancestor::style)]");

        var textContent = new StringBuilder();
        if (textNodes != null)
        {
            foreach (var node in textNodes)
            {
                var text = HttpUtility.HtmlDecode(node.InnerText).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    textContent.Append(text).Append(" ");
                }
            }
        }

        var parsedContent = textContent.ToString().Trim();

        return (parsedContent, hyperlinks, finalUrl, pageTitle);
    }
    catch (Exception ex)
    {
        return ($"Error fetching or parsing {url}: {ex.Message}", new List<(string, string)>(), url, "Error");
    }
}
```

#### Detailed Explanation:

This method is responsible for fetching and parsing the content of a given URL. It's a crucial part of the plugin, extracting the relevant information from web pages. Here's a detailed breakdown:

1. **HTTP Client Setup**:
   - The method uses a pre-configured `HttpClient` from the `IHttpClientFactory`, which is set up with optimal settings for performance and reliability.
   - A timeout of 10 seconds is set to prevent hanging on slow or unresponsive servers.
   - A random user agent is selected from the predefined list, helping to mimic different browsers and avoid detection as a bot.
   - Additional headers are set to further simulate a real browser request:
     - `Accept-Encoding`: Indicates support for various compression methods.
     - `Accept`: Specifies the preferred content types, prioritizing HTML.
     - `Connection`: Set to "keep-alive" to reuse the TCP connection for multiple requests.

2. **HTTP Request and Response Handling**:
   - The request is sent asynchronously using `GetAsync` with `HttpCompletionOption.ResponseHeadersRead`, which allows reading the headers before downloading the entire content.
   - `EnsureSuccessStatusCode()` is called to throw an exception for non-success status codes.
   - The content is read as a string asynchronously.
   - The final URL is captured to handle any redirects that might have occurred.

3. **HTML Parsing**:
   - The HtmlAgilityPack library is used to parse the HTML content.
   - The page title is extracted from the `<title>` tag.
   - Important links are extracted using the `ExtractImportantLinks` method (explained in the next section).

4. **Content Cleaning**:
   - Non-content elements like headers, footers, navigation, and scripts are removed to focus on the main content.
   - This is done by selecting these elements and removing them from the document.

5. **Text Extraction**:
   - The main content is identified by looking for a `<main>` tag or falling back to the `<body>` if `<main>` is not present.
   - All text nodes within the main content are selected, excluding those within `<script>` and `<style>` tags.
   - The text is decoded to handle HTML entities, trimmed, and concatenated into a single string.

6. **Error Handling**:
   - The entire process is wrapped in a try-catch block.
   - If any error occurs during fetching or parsing, a formatted error message is returned along with empty or default values for other fields.

7. **Return Value**:
   - The method returns a tuple containing:
     - The parsed content
     - A list of important hyperlinks
     - The final URL (which may be different from the initial URL due to redirects)
     - The page title

The combination of these techniques allows the plugin to effectively extract useful content from web pages while handling various edge cases and potential errors.

### 3.6 Link Extraction

```csharp
private static List<(string Url, string Text)> ExtractImportantLinks(HtmlDocument doc, Uri baseUri)
{
    var mainContent = doc.DocumentNode.SelectSingleNode("//main") 
        ?? doc.DocumentNode.SelectSingleNode("//article")
        ?? doc.DocumentNode.SelectSingleNode("//div[@id='content']")
        ?? doc.DocumentNode.SelectSingleNode("//div[@class='content']")
        ?? doc.DocumentNode.SelectSingleNode("//body");

    if (mainContent == null)
    {
        return new List<(string, string)>();
    }

    var links = mainContent.SelectNodes(".//a[@href]")
        ?.Select(a => 
        {
            var url = a.GetAttributeValue("href", "");
            var text = a.InnerText.Trim();
            var title = a.GetAttributeValue("title", "");
            var cssClass = a.GetAttributeValue("class", "");
            var ariaLabel = a.GetAttributeValue("aria-label", "");
            var role = a.GetAttributeValue("role", "");
            var parentNode = a.ParentNode;
            return (Url: url, Text: text, Title: title, CssClass: cssClass, AriaLabel: ariaLabel, Role: role, Node: a, ParentNode: parentNode);
        })
        .Where(link => !string.IsNullOrEmpty(link.Url) 
                    && !link.Url.StartsWith("#") 
                    && !link.Url.StartsWith("javascript:")
                    && link.Text.Length > 3)
        .Select(link => 
        {
            if (Uri.TryCreate(baseUri, link.Url, out var absoluteUri))
            {
                return (absoluteUri.ToString(), link.Text, link.Title, link.CssClass, link.AriaLabel, link.Role, link.Node, link.ParentNode);
            }
            return (link.Url, link.Text, link.Title, link.CssClass, link.AriaLabel, link.Role, link.Node, link.ParentNode);
        })
        .Where(link => Uri.TryCreate(link.Item1, UriKind.Absolute, out var linkUri) 
                    && linkUri.Host == baseUri.Host)
        .Distinct()
        .ToList() ?? new List<(string, string, string, string, string, string, HtmlNode, HtmlNode)>();

    var scoredLinks = links
        .Select(link => 
        {
            int score = 0;
            var combinedText = (link.Item2 + " " + link.Item3 + " " + link.Item5).ToLower(); // Include aria-label in combinedText
            var url = link.Item1.ToLower();
            
            // Scoring logic...

            return (Link: link, Score: score);
        })
        .OrderByDescending(item => item.Score)
        .Take(10)
        .Select(item => (item.Link.Item1, item.Link.Item2)) // Convert back to (Url, Text) tuple
        .ToList();

    return scoredLinks;
}
```

#### Detailed Explanation:

This method is responsible for extracting and scoring important links from the parsed HTML content. It's a crucial component for identifying relevant sub-pages or related content. Here's a detailed breakdown:

1. **Main Content Identification**:
   - The method attempts to find the main content area of the page by looking for common HTML structures:
     - `<main>` tag
     - `<article>` tag
     - `<div>` with id "content"
     - `<div>` with class "content"
     - Falling back to the `<body>` if none of the above are found
   - This approach helps focus on the most relevant part of the page, ignoring navigation menus and footers.

2. **Link Selection and Initial Filtering**:
   - All `<a>` tags with `href` attributes within the main content are selected.
   - Each link is transformed into a tuple containing various attributes (URL, text, title, CSS class, ARIA label, role, and parent node information).
   - Initial filtering is applied to remove:
     - Empty URLs
     - Internal page anchors (starting with #)
     - JavaScript links
     - Links with very short text (3 characters or less)

3. **URL Normalization**:
   - Relative URLs are converted to absolute URLs using the base URI of the page.
   - This ensures all URLs are in a consistent format for further processing.

4. **Same-Domain Filtering**:
   - Links are filtered to only include those from the same domain as the original page.
   - This helps focus on internal links, which are more likely to be relevant to the original content.

5. **Link Scoring**:
   - Each link is assigned a score based on various factors:
     - Presence of retail-related keywords in the link text or URL
     - URL structure (e.g., containing "/product/" or "/item/")
     - Presence of price information
     - HTML structure and attributes (e.g., product-related classes or microdata)
     - Presence of images
     - Accessibility attributes (e.g., role="button" or product-related ARIA labels)

6. **Final Selection**:
   - Links are ordered by their scores in descending order.
   - The top 10 scoring links are selected.
   - The final result is converted back to a simple (URL, Text) tuple for ease of use.

The link scoring system in more detail:

```csharp
int score = 0;
var combinedText = (link.Item2 + " " + link.Item3 + " " + link.Item5).ToLower();
var url = link.Item1.ToLower();

// Retail-related keywords
if (combinedText.Contains("buy") || combinedText.Contains("shop") || combinedText.Contains("product")) score += 3;

// URL structure
if (url.Contains("/product/") || url.Contains("/item/")) score += 3;
if (System.Text.RegularExpressions.Regex.IsMatch(url, @"/[A-Za-z0-9-]+/[A-Za-z0-9-]+$")) score += 2;

// Price information
if (System.Text.RegularExpressions.Regex.IsMatch(combinedText, @"\$\d+(\.\d{2})?|\d+(\.\d{2})?\s*(USD|EUR|GBP)")) score += 3;

// Product-related words
if (combinedText.Contains("details") || combinedText.Contains("specifications") || combinedText.Contains("specs")) score += 2;

// HTML structure
if (link.Item4.Contains("product") || link.Item4.Contains("item")) score += 2;
if (link.ParentNode.Name == "li" && (link.ParentNode.ParentNode.GetAttributeValue("class", "").Contains("product") || link.ParentNode.ParentNode.GetAttributeValue("class", "").Contains("item"))) score += 2;

// Microdata
if (link.Node.GetAttributeValue("itemprop", "") == "url" && link.ParentNode.GetAttributeValue("itemtype", "").Contains("Product")) score += 3;

// Context
if (link.Node.SelectSingleNode("./img") != null || link.ParentNode.SelectSingleNode("./img") != null) score += 2;

// Accessibility attributes
if (link.Item6 == "button") score += 2;
if (link.Item5.Contains("product") || link.Item5.Contains("item")) score += 3;
if (link.Node.GetAttributeValue("aria-describedby", "").Contains("product")) score += 2;
if (link.Node.GetAttributeValue("aria-details", "").Contains("product")) score += 2;

// Check for "Add to Cart" or similar phrases in accessibility attributes
if (combinedText.Contains("add to cart") || combinedText.Contains("buy now")) score += 4;

// Other importance factors
if (link.Item2.All(c => char.IsUpper(c))) score += 2; // All caps text
if (link.Item2.Length > 20) score += 1; // Longer link text
```

This scoring system takes into account various factors:
1. **Retail-specific keywords**: Words like "buy", "shop", or "product" increase the score.
2. **URL structure**: URLs containing "/product/" or "/item/" are favored.
3. **Price information**: The presence of price-like patterns increases the score.
4. **Product-related terminology**: Words like "details", "specifications", or "specs" are considered.
5. **HTML structure**: Links within product-related HTML structures get higher scores.
6. **Microdata**: Links with product-related microdata are prioritized.
7. **Visual context**: Links associated with images are given extra points.
8. **Accessibility attributes**: The method considers ARIA roles and labels, which often provide semantic information about the link's purpose.
9. **Call-to-action phrases**: Phrases like "add to cart" or "buy now" significantly increase the score.
10. **Text formatting**: All-caps text and longer link text slightly increase the score.

This comprehensive scoring system allows the plugin to identify the most relevant product-related links on a page, even when dealing with various website structures and designs.

## 4. Key Features and Optimizations

The RetailContextPlugin incorporates several key features and optimizations to enhance its performance and effectiveness:

1. **Parallel Processing**:
   - The plugin uses `Task.WhenAll` to fetch and parse multiple URLs concurrently, significantly reducing the total processing time.
   - Example: `var results = await Task.WhenAll(tasks);`

2. **Asynchronous Operations**:
   - Extensive use of async/await pattern throughout the code for non-blocking I/O operations.
   - This improves responsiveness and allows efficient use of system resources.

3. **HTTP/2 and Connection Pooling**:
   - The HttpClient is configured to use **HTTP/2** and maintain up to **75 connections** per server.
   - This configuration is set in Program.cs:
     ```csharp
     EnableMultipleHttp2Connections = true,
     MaxConnectionsPerServer = 75,
     ```

4. **User Agent Rotation**:
   - The plugin rotates through different user agents to mimic various browsers and reduce the chance of being blocked.
   - Example: `var userAgent = USER_AGENTS[random.Next(USER_AGENTS.Length)];`

5. **Intelligent Link Extraction**:
   - The `ExtractImportantLinks` method uses a sophisticated scoring system to identify the most relevant links on a page.

6. **Content Cleaning**:
   - Non-essential elements like headers, footers, and scripts are removed to focus on the main content.
   - Example: `var nodesToRemove = doc.DocumentNode.SelectNodes("//header | //footer | //nav | //aside | //script | //style");`

7. **Error Handling and Logging**:
   - Comprehensive try-catch blocks and logging statements throughout the code for better error tracking and debugging.

8. **Fallback Mechanisms**:
   - The plugin includes fallback options, such as providing example URLs when Bing search fails, ensuring robustness.

9. **Environment Variable Usage**:
   - Sensitive information like API keys is stored in environment variables, enhancing security.

10. **Customizable Search Parameters**:
    - The Bing search can be customized using the `CUSTOM_CONFIG_ID`, allowing for tailored search results.

## 5. Integration with Program.cs

The Program.cs file sets up the environment and configuration for the RetailContextPlugin. Here are the key points of integration:

1. **HttpClient Configuration**:
   ```csharp
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
   ```
   This configuration optimizes the HttpClient for performance and reliability:
   - Disables cookies and proxies to reduce overhead
   - Enables automatic redirection and decompression
   - Configures HTTP/2 with multiple connections
   - Sets a high limit for concurrent connections per server
   - Configures a connection lifetime for efficient resource management
   - Specifies modern TLS protocols for security

2. **RetailContextPlugin Registration**:
   ```csharp
   builder.Services.AddSingleton<RetailContextPlugin>();
   ```
   This registers the RetailContextPlugin as a singleton service, ensuring a single instance is used throughout the application lifecycle.

3. **Semantic Kernel Integration**:
   ```csharp
   kernelBuilder.Plugins.AddFromObject(retailContextPlugin, "RetailContext");
   ```
   This adds the RetailContextPlugin to the Semantic Kernel, making it available for use in AI-powered operations.

4. **Environment Variable Loading**:
   ```csharp
   DotNetEnv.Env.Load();
   ```
   This loads environment variables, which are used for storing sensitive information like API keys.

5. **CORS Configuration**:
   ```csharp
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
   ```
   This sets up CORS (Cross-Origin Resource Sharing) policies, allowing the API to be accessed from specified origins.

These integrations in Program.cs ensure that the RetailContextPlugin is properly configured, optimized, and integrated into the larger application ecosystem.

# Conclusion
**Disclaimer**
Users of this project are solely responsible for ensuring their use complies with the terms and conditions of all third-party services utilized, including but not limited to Bing Custom Search, Azure services, and OpenAI. Users must also ensure their use of this project adheres to all applicable local, national, and international laws and regulations.

The creators and contributors of this project are not responsible for any misuse, data breaches, costs incurred, or any other liabilities arising from the use of this project or the third-party services it integrates with. This project is provided "as is" without any warranty, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or the use or other dealings in the software.

## License
This project is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. To view a copy of this license, visit [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/).

This project is licensed for free use for educational and non-commercial purposes only. Commercial use is strictly prohibited. The software is provided "as is", without warranty of any kind, express or implied. In no event shall the authors or copyright holders be liable for any claim, damages, or other liability, whether in an action of contract, tort, or otherwise, arising from, out of, or in connection with the software or the use or other dealings in the software.

## Contributing
Contributions to improve the educational value of this project are welcome. Please follow these steps to contribute:

1. Fork the repository
2. Create a new branch for your feature or bug fix
3. Make your changes and commit them with clear, descriptive messages
4. Push your changes to your fork
5. Submit a pull request with a clear description of your changes
