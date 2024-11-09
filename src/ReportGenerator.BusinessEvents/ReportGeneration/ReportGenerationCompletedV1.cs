using ReportGenerator.BusinessEvents.ReportGeneration.Models;
using ReportGenerator.Common.Messaging;
using System.Text.Json.Serialization;

namespace ReportGenerator.BusinessEvents.ReportGeneration;

public class ReportGenerationCompletedV1 : CloudEvent<ReportGenerationCompletedModel>
{
    [JsonConstructor]
    public ReportGenerationCompletedV1(Uri source, string id, DateTimeOffset time, ReportGenerationCompletedModel data)
        : base(
            ReportGenerationEventTypes.ReportGenerationCompletedV1,
            source,
            id,
            "1.0",
            null,
            "Report Generation Completed V1",
            (DateTimeOffset?)time,
            "application/json",
            data
        ) { }
}
