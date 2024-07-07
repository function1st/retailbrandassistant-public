
# Retail Brand Agent

This project is a configurable Retail Brand Agent built using Microsoft Semantic Kernel, Open AI, and Microsoft Bing CustomSearch. It allows brands to rapidly test and deploy a fully functioning AI Assistant for Customers that uses real-time page information to answer questions with very high accuracy and consistency.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)
- OpenAI API key
- Bing CustomSearch instance and key

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/retail-brand-agent.git
   cd retail-brand-agent
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
