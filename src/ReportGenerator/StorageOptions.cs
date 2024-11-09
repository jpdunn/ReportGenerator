namespace ReportGenerator;

public class StorageOptions
{
    /// <summary>
    /// Section Name in appsettings.json.
    /// </summary>
    public static string Section => "StorageOptions";

    public string StorageAccountConnectionString { get; set; } = string.Empty;

    public string StorageAccountKey { get; set; } = string.Empty;

    public string StorageAccountName { get; set; } = string.Empty;

    public string GeneratedReportsContainerName { get; set; } = string.Empty;
}
