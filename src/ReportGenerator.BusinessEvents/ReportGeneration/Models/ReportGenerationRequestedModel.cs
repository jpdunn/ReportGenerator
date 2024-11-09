namespace ReportGenerator.BusinessEvents.ReportGeneration.Models;

public class ReportGenerationRequestedModel
{
    public int ProjectID { get; set; }

    public string EventType { get; } = ReportGenerationEventTypes.ReportGenerationRequestedV1;

    public List<ReportGenerationMarkupDetails> MarkupDetails { get; set; } = [];

    public string Author { get; set; } = string.Empty;
}
