using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using DotNetEnv;
using System.Web;
using Microsoft.SemanticKernel;

public class RetailContextPlugin
{
    private static readonly Random random = new Random();
    private static readonly string[] USER_AGENTS = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/91.0.864.59 Safari/537.36",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1"
    };

    private readonly IHttpClientFactory _httpClientFactory;

    public RetailContextPlugin(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

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

    private async Task PrefetchDnsAndConnect(string url)
    {
        try
        {
            var uri = new Uri(url);
            var addresses = await Dns.GetHostAddressesAsync(uri.Host);
            if (addresses.Length > 0)
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(addresses[0], uri.Port);
            }
        }
        catch
        {
            // Ignore any errors during prefetch
        }
    }

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
                
                // Retail-related keywords
                if (combinedText.Contains("buy") || combinedText.Contains("shop") || combinedText.Contains("product")) score += 3;
                
                // URL structure
                if (url.Contains("/product/") || url.Contains("/item/")) score += 3;
                if (System.Text.RegularExpressions.Regex.IsMatch(url, @"/[A-Za-z0-9-]+/[A-Za-z0-9-]+$")) score += 2; // Potential product URL pattern

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
                if (link.Item6 == "button") score += 2; // role="button" often indicates an important action
                if (link.Item5.Contains("product") || link.Item5.Contains("item")) score += 3; // aria-label mentioning product or item
                if (link.Node.GetAttributeValue("aria-describedby", "").Contains("product")) score += 2;
                if (link.Node.GetAttributeValue("aria-details", "").Contains("product")) score += 2;

                // Check for "Add to Cart" or similar phrases in accessibility attributes
                if (combinedText.Contains("add to cart") || combinedText.Contains("buy now")) score += 4;

                // Other importance factors
                if (link.Item2.All(c => char.IsUpper(c))) score += 2; // All caps text
                if (link.Item2.Length > 20) score += 1; // Longer link text
                
                return (Link: link, Score: score);
            })
            .OrderByDescending(item => item.Score)
            .Take(10)
            .Select(item => (item.Link.Item1, item.Link.Item2)) // Convert back to (Url, Text) tuple
            .ToList();

        return scoredLinks;
    }
}

public class BingSearchResponse
{
    [JsonPropertyName("webPages")]
    public WebPages? WebPages { get; set; }
}

public class WebPages
{
    [JsonPropertyName("value")]
    public List<BingSearchResult>? Value { get; set; }
}

public class BingSearchResult
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    [JsonPropertyName("dateLastCrawled")]
    public DateTime DateLastCrawled { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("isNavigational")]
    public bool IsNavigational { get; set; }
}

public class PageOutput
{
    public string? PageTitle { get; set; }
    public string? PageUrl { get; set; }
    public string? ParsedPageText { get; set; }
    public List<Hyperlink>? Hyperlinks { get; set; }
}

public class Hyperlink
{
    public string? Url { get; set; }
    public string? Text { get; set; }
}