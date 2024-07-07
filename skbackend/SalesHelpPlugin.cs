using System.ComponentModel;
using Microsoft.SemanticKernel;

public class SalesHelpPlugin
{
    [KernelFunction, Description("Get the appropriate human assistance for sales topics for a customer. Only use for obtaining human sales assistance and nothing else.")]
    public string GetSalesHelpQueue(
        [Description("The type of product (Surface, Xbox, Office, Windows, or Other)")] string productType,
        [Description("The type of customer (Personal or Business)")] string customerType,
        [Description("Whether it's a new or existing order")] string orderType)
    {
        var queue = DetermineQueue(productType, customerType);
        return $"I'm connecting you to our {queue} for {orderType} orders. A sales representative will be with you shortly to assist you with your {productType} {orderType.ToLower()} order.";
    }

    private string DetermineQueue(string productType, string customerType)
    {
        if (productType == "Surface" || productType == "Xbox" || productType == "Office" || productType == "Windows")
        {
            return $"{productType} {customerType} Sales Queue";
        }
        return $"General {customerType} Sales Queue";
    }

    [KernelFunction, Description("Determine if a user explicitly requests human assistance or sales order processing")]
    public bool RequiresHumanAssistance(
        [Description("The customer's query or request")] string query)
    {
        string lowercaseQuery = query.ToLower();
        return lowercaseQuery.Contains("speak to a human") ||
               lowercaseQuery.Contains("talk to a person") ||
               lowercaseQuery.Contains("human agent") ||
               lowercaseQuery.Contains("real person") ||
               lowercaseQuery.Contains("process my order") ||
               lowercaseQuery.Contains("human help with my order") ||
               lowercaseQuery.Contains("human help to place an order") ||
               (lowercaseQuery.Contains("human") && lowercaseQuery.Contains("purchase"));
    }
}