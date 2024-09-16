# Retail Brand Assistant

This project is a configurable Retail Brand Assistant built using **Microsoft Semantic Kernel** for AI Orchestration, OpenAI or Azure OpenAI GPT models, and **Microsoft Bing Custom Search** for brand-specific search results. Using the Retail Brand Assistant allows brands to rapidly test and deploy a fully functioning AI Assistant for customers that uses real-time page information to answer questions with high accuracy and consistency.

| When retail brand experiences fail like this... | Retail Brand Assistant succeeds... |
|:-----------------------------------------------:|:----------------------------------:|
| ![Screenshot](https://github.com/function1st/retailbrandassistant-public/assets/129132283/43d368a7-f62c-4e0d-9bd8-76f1e2fe00a1) | ![Screenshot](https://github.com/function1st/retailbrandassistant-public/assets/129132283/ec8f3794-be92-43b3-9c0b-7ee35f23fc4e) |

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)
- OpenAI API key **or** Azure OpenAI API key and deployment
- [Bing Custom Search instance and key](https://www.customsearch.ai/)
- Node.js and npm (for the frontend)

## Bing Custom Search Configuration (Skip if already configured)

<details>
  <summary>Click to expand Bing Custom Search setup instructions</summary>

### Bing Custom Search Prerequisites

1. **Microsoft Azure Account**: You need an Azure account to use Bing Custom Search. Sign up at [Azure](https://azure.microsoft.com/).

### Step 1: Create a Bing Custom Search Resource on Azure

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

### Step 2: Access the Bing Custom Search UI

1. After the resource is created, go to the resource page.
2. Under **Resource Management**, click on **Custom Search Portal**. This will take you to the Bing Custom Search UI.

### Step 3: Add a Domain to Your Custom Search

1. In the Custom Search UI, click on **Create new custom search instance**.
2. Provide a name and description for your custom search instance.
3. Click **Create**.
4. Click on **Add a site to your search instance**.
5. Enter the domain or specific URLs you want to include in your custom search.
6. Click **Add**.

### Step 4: Customize Your Search Instance

1. After adding the domain, you can further customize your search instance by specifying:
   - **Domains and URLs**: Add multiple domains or specific URLs.
   - **Pinned Results**: Pin specific pages to the top of the search results.
   - **Block Certain Sites**: Exclude certain domains or URLs from search results.

### Step 5: Publish Your Custom Search Instance

1. Once you have added the necessary domains and customized your search instance, click on **Publish**.
2. Confirm the details and click **Publish** again to make your custom search instance live.

### Step 6: Retrieve API Key and Endpoint

1. Go back to the Azure portal, under your Bing Custom Search resource.
2. Under **Keys and Endpoint**, copy one of the keys and the endpoint URL. You will need these if you decide to access the custom search programmatically.

</details>

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

### Initial Setup

1. **Clone the repository:**

   ```bash
   git clone https://github.com/function1st/retailbrandassistant-public.git
   cd retailbrandassistant-public
   ```

2. **Make the scripts executable and run the setup script:**

   ```bash
   chmod +x setup.sh start.sh startbackend.sh startfrontend.sh stop.sh
   ./setup.sh
   ```

3. **Follow the prompts to enter your brand name, language, market, and locale.**

4. **Edit the `.env` file and add your API keys:**

   Depending on whether you are using OpenAI or Azure OpenAI, you will need to provide different environment variables.

   **For OpenAI:**

   ```ini
   # .env file in skbackend directory

   AZURE_OPEN_AI=False  # or omit this line; default is False

   OPENAI_API_KEY=your_openai_api_key_here
   OPENAI_MODEL_NAME=gpt-4  # Optional, defaults to 'gpt-4o-mini' if not set
   BING_SUBSCRIPTION_KEY=your_bing_subscription_key_here
   CUSTOM_CONFIG_ID=your_bing_custom_config_id_here
   ```

   **For Azure OpenAI:**

   ```ini
   # .env file in skbackend directory

   AZURE_OPEN_AI=True

   AZURE_OPENAI_ENDPOINT=https://your-azure-endpoint.openai.azure.com/
   AZURE_OPENAI_KEY=your_azure_openai_api_key_here
   AZURE_OPENAI_DEPLOYMENT_NAME=your_deployment_name_here
   BING_SUBSCRIPTION_KEY=your_bing_subscription_key_here
   CUSTOM_CONFIG_ID=your_bing_custom_config_id_here
   ```

   **Note:** The `AZURE_OPEN_AI` environment variable determines whether the application uses Azure OpenAI or OpenAI. If `AZURE_OPEN_AI` is not set or set to `False`, the application defaults to OpenAI.

### Usage

1. **To start the application:**

   ```bash
   ./start.sh
   ```

   This will automatically update the current date in the system message and start both the backend and frontend.

2. **Interact with the Retail Brand Assistant through your web browser at `http://localhost:3000`.**

3. **To stop the application:**

   ```bash
   ./stop.sh
   ```

## Configuration

The setup script helps you configure the basic brand information and site information. If you need to change this information later, you can either run the setup script again or manually edit the `SystemMessage.txt` file located in the `skbackend` directory.

The current date is automatically updated each time you start the application using the start script.

You can customize the Retail Brand Assistant by modifying the following files:

- `SystemMessage.txt`: Contains the system message that defines the agent's behavior and how it uses real-time context.
- `Program.cs`: Main Semantic Kernel logic and chat handling. In addition, the prompt logic for plugin orchestration as well as topic classification is found here.
- `RetailContextPlugin.cs`: Plugin for retrieving real-time retail context. Leverages Bing Custom Search to retrieve top results, uses OpenAI to pick the most relevant pages that answer the question, visits those pages to parse important information from them, and then provides that context back to Semantic Kernel for leveraging with OpenAI.
- `SalesHelpPlugin.cs`: Sample plugin for handling sales-related queries based on the `SalesHelpPrompt.txt` instructions.
- `SalesHelpPrompt.txt`: Contains a sample prompt for handling a hypothetical human transfer scenario for sales-related queries.

**IMPORTANT:** This project is intended for educational purposes only and should not be used for production workloads. The creators and contributors of this project accept no responsibility for its functionality, reliability, or any consequences arising from its use.

## Orchestration and Conversation Topic Classifiers

For a detailed overview of the conversation flows and how the conversation topic classifiers work, please refer to the [Orchestration and Conversation Topic Classifiers](#orchestration-and-conversation-topic-classifiers) section.

## License

This project is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License. To view a copy of this license, visit [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/).

This project is licensed for free use for educational and non-commercial purposes only. Commercial use is strictly prohibited. The software is provided "as is," without warranty of any kind, express or implied.

## Contributing

Contributions to improve the educational value of this project are welcome. Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them with clear, descriptive messages.
4. Push your changes to your fork.
5. Submit a pull request with a clear description of your changes.

**Disclaimer**

Users of this project are solely responsible for ensuring their use complies with the terms and conditions of all third-party services utilized, including but not limited to Bing Custom Search, Azure services, and OpenAI. Users must also ensure their use of this project adheres to all applicable local, national, and international laws and regulations.

The creators and contributors of this project are not responsible for any misuse, data breaches, costs incurred, or any other liabilities arising from the use of this project or the third-party services it integrates with.