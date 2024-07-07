
# Retail Brand Agent

This project is a configurable Retail Brand Agent built using Semantic Kernel. It allows users to interact with an AI assistant that can provide information about a specific retail brand, handle sales inquiries, and more.

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

The setup script will help you configure the basic brand information. If you need to change this information later, you can either run the setup script again or manually edit the `SystemMessage.txt` file.

The current date is automatically updated each time you start the application.

You can customize the Retail Brand Agent by modifying the following files:

- `SystemMessage.txt`: Contains the system message that defines the agent's behavior and knowledge.
- `SalesHelpPrompt.txt`: Contains the prompt for handling sales-related queries.
- `Program.cs`: Main program logic and chat loop.
- `RetailContextPlugin.cs`: Plugin for retrieving real-time retail context.
- `SalesHelpPlugin.cs`: Plugin for handling sales-related queries.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
