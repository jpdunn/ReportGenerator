namespace ReportGenerator;

public class ServiceBusOptions
{
    /// <summary>
    /// Section Name in appsettings.json.
    /// </summary>
    public static string Section => "ServiceBusOptions";

    public string ServiceBusConnectionString { get; set; } = string.Empty;

    public string ReportGenerationRequestsQueueName { get; set; } = string.Empty;

    public string ReportGenerationCompletedTopicName { get; set; } = string.Empty;
}
