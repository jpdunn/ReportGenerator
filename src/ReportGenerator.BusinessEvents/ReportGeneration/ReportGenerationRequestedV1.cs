using System.Text.Json.Serialization;
using ReportGenerator.BusinessEvents.ReportGeneration.Models;
using ReportGenerator.Common.Messaging;

namespace ReportGenerator.BusinessEvents.ReportGeneration;

public class ReportGenerationRequestedV1 : CloudEvent<ReportGenerationRequestedModel>
{
    [JsonConstructor]
    public ReportGenerationRequestedV1(Uri source, string id, DateTimeOffset time, ReportGenerationRequestedModel data)
        : base(
            ReportGenerationEventTypes.ReportGenerationRequestedV1,
            source,
            id,
            "1.0",
            null,
            "Report Generation Requested V1",
            (DateTimeOffset?)time,
            "application/json",
            data
        ) { }
}
