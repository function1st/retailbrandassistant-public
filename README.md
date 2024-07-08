
# Retail Brand Agent

This project is a configurable Retail Brand Agent built using Microsoft Semantic Kernel, Open AI, and Microsoft Bing CustomSearch. It allows brands to rapidly test and deploy a fully functioning AI Assistant for Customers that uses real-time page information to answer questions with very high accuracy and consistency.

| When retail brand experiences fail like this... | Retail Brand Assistant succeeds... |
|:----------------------------------------------------------------------------:|:---------------------------------:|
| <img src="readme-png/wholesale-fail.png" alt="Screenshot" width="450"> | <img src="readme-png/rba-wholesale.png" alt="Screenshot" width="250"> |

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)
- OpenAI API key
- Bing CustomSearch instance and key

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
  
      <br><img src="readme-png/bingcs-urls.png" alt="Screenshot" width="450">

#### Step 5: Publish Your Custom Search Instance

1. Once you have added the necessary domains and customized your search instance, click on **Publish**.
2. Confirm the details and click **Publish** again to make your custom search instance live.
   
      <img src="readme-png/bingcs-publishhelp.png" alt="Screenshot" width="450">
      <br><img src="readme-png/bingcs-publish.png" alt="Screenshot" width="450">


#### Step 6: Retrieve API Key and Endpoint

1. Go back to the Azure portal, under your Bing Custom Search resource.
2. Under **Keys and Endpoint**, copy one of the keys and the endpoint URL. You will need these if you decide to access the custom search programmatically.

That's it! You've set up and used Bing Custom Search with the Custom Search UI, added domains, and published your custom search instance.

## Retail Brand Assistant Setup

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

2. Interact with the Retail Brand Agent through the browser.

3. To stop the application:
   ```bash
   ./stop.sh
   ```

## Configuration

The setup script will help you configure the basic brand information and site information. If you need to change this information later, you can either run the setup script again or manually edit the `SystemMessage.txt` file.

The current date is automatically updated each time you start the application using the start script.

You can customize the Retail Brand Agent by modifying the following files:

- `SystemMessage.txt`: Contains the system message that defines the agent's behavior and how it uses real-time context.
- `Program.cs`: Main Semantic Kernel logic and chat handling. In addition, the prompt logic for Plug-in orchrestration as well as Topic classification is found here.
- `RetailContextPlugin.cs`: Plugin for retrieving real-time retail context. Leverages Bing CustomSearch to retrieve top results, uses Open AI to pick the most relevant pages that answer the question, visits those pages to parse imporant information from them, and then provides that context back to Semantic Kernel for leveraging with Open AI.
- `SalesHelpPlugin.cs`: Sample plugin for handling sales-related queries based on the SalesHelpPromt.txt instructions.
- `SalesHelpPrompt.txt`: Contains a sample prompt for handling a hypothetical human transfer scenario for sales-related queries.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
