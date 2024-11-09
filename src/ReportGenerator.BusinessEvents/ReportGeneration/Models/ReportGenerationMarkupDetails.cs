namespace ReportGenerator.BusinessEvents.ReportGeneration.Models;

public class ReportGenerationMarkupDetails
{
    public string? ImageUri { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public string Comments { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
}
