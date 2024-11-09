namespace ReportGenerator.BusinessEvents.ReportGeneration.Models;

public class ReportGenerationCompletedModel
{
    public int ProjectID { get; set; }

    public string EventType { get; } = ReportGenerationEventTypes.ReportGenerationCompletedV1;

    public string GeneratedReportUri { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public DateTimeOffset GenerationDateTimeUTC { get; set; }

    public string Author { get; set; } = string.Empty;
}
