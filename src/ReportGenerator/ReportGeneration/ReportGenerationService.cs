using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using ReportGenerator.BusinessEvents.ReportGeneration;
using ReportGenerator.BusinessEvents.ReportGeneration.Models;
using ReportGenerator.Common.Messaging;
using ReportGenerator.Core;
using Serilog;
using Stimulsoft.Report;
using Stimulsoft.Report.Export;

namespace ReportGenerator.ReportGeneration;

/// <summary>
/// Defines the handler for a report generation requested Service Bus message.
/// </summary>
public class ReportGenerationService(
    IOptions<ServiceBusOptions> serviceBusOptions,
    IOptions<StorageOptions> storageOptions,
    AzureStorageService storageService
)
{
    private readonly string _serviceBusConnectionString = serviceBusOptions.Value.ServiceBusConnectionString;
    private readonly string _topicName = serviceBusOptions.Value.ReportGenerationCompletedTopicName;
    private readonly string _containerName = storageOptions.Value.GeneratedReportsContainerName;
    private readonly AzureStorageService _storageService = storageService;

    public async Task HandleReportGenerationRequestAsync(ReportGenerationRequestedModel model)
    {
        var fileStream = GenerateReportDocument(model.MarkupDetails);

        // Reset the position of the stream as we're done writing to it, all we need to do now is upload it.
        fileStream.Position = 0;

        BlobClient? blob;
        string fileName;

        try
        {
            Log.Information("Uploading generated document to blob storage...");

            fileName = $"report-{model.ProjectID}-{Guid.NewGuid()}.docx";

            blob = await _storageService.UploadBlobAsync(_containerName, fileStream, fileName);

            Log.Information("Upload successful.");
        }
        catch (Exception e)
        {
            Log.Error("Upload failed. Error: '{ErrorMessage}'", e.Message);

            throw;
        }
        finally
        {
            // Close the stream so that the resources it uses are released.
            fileStream.Close();
        }

        var messageService = new ServiceBusMessageService(_serviceBusConnectionString, _topicName);

        try
        {
            var currentUtcDateTime = DateTimeOffset.UtcNow;

            var eventModel = new ReportGenerationCompletedV1(
                source: new Uri("urn:jpdunn:report-generator"),
                id: Guid.NewGuid().ToString(),
                time: currentUtcDateTime,
                data: new ReportGenerationCompletedModel
                {
                    ProjectID = model.ProjectID,
                    GeneratedReportUri = blob.Uri.ToString(),
                    FileName = fileName,
                    GenerationDateTimeUTC = currentUtcDateTime,
                    Author = model.Author
                }
            );

            Log.Information("Posting message to service bus. {Message}", eventModel.ToString());

            // Post a message to the topic once the report has been generated.
            await messageService.SendMessageAsync(eventModel);
        }
        catch (Exception e)
        {
            Log.Error("An unknown error occurred attempting to send message to service bus topic. {Error}", e.Message);

            throw;
        }
        finally
        {
            await messageService.DisposeAsync();
        }
    }

    private MemoryStream GenerateReportDocument(List<ReportGenerationMarkupDetails> markupDetails)
    {
        Log.Information("Generating requested document...");

        List<ReportBusinessObject> reportDetails = markupDetails
            .Select(x => new ReportBusinessObject
            {
                Comments = x.Comments,
                ImageUri = string.IsNullOrEmpty(x.ImageUri)
                    ? x.ImageUri
                    : _storageService.GetSasTokenForBlobUri(new Uri(x.ImageUri)),
                Location = x.LocationName,
                Tags = string.Join(", ", x.Tags)
            })
            .ToList();

        var report = new StiReport();

        Log.Information("Loading template.");

        // Load the template from disk.
        report.Load("ReportTemplates/<<INSERT_TEMPLATE_HERE>>.mrt");

        Log.Information("Registering data.");

        // Register the data as the business object is only a virtual object and doesn't have data behind it.
        report.RegData("ReportDetails", "ReportDetails", reportDetails);

        // Register the business object as the virtual mapping.
        report.RegBusinessObject("Data", "ReportBusinessObject", reportDetails);

        // Synchronize the dictionary for both the data and business objects so
        // that they're up to date with the data we just registered.
        report.Dictionary.SynchronizeBusinessObjects();
        report.Dictionary.Synchronize();

        Log.Information("Compiling...");

        // Compile the report so that the variables can be set.
        report.Compile();

        Log.Information("Rendering...");

        // Set the variables that are defined in the template.
        report.Render(false);

        Log.Information("Document generated, exporting...");

        var settings = new StiWordExportSettings();

        MemoryStream stream = new();
        report.ExportDocument(StiExportFormat.Word, stream, settings);

        Log.Information("Document export successful.");

        return stream;
    }
}