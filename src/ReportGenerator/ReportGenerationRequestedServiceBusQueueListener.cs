using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ReportGenerator.BusinessEvents.ReportGeneration;
using ReportGenerator.BusinessEvents.ReportGeneration.Models;
using ReportGenerator.Common.Messaging;
using ReportGenerator.ReportGeneration;
using Serilog;

namespace ReportGenerator;

public class ReportGenerationRequestedServiceBusQueueListener : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusClient _client;
    private readonly ReportGenerationService _reportGenerationService;

    public ReportGenerationRequestedServiceBusQueueListener(
        IOptions<ServiceBusOptions> options,
        ReportGenerationService reportGenerationService
    )
    {
        _reportGenerationService = reportGenerationService;

        _client = new ServiceBusClient(options.Value.ServiceBusConnectionString);
        _processor = _client.CreateProcessor(
            options.Value.ReportGenerationRequestsQueueName,
            new ServiceBusProcessorOptions() { Identifier = "Atom-ReportGen-ServiceBus-Listener" }
        );

        _processor.ProcessMessageAsync += ProcessMessageHandlerAsync;
        _processor.ProcessErrorAsync += ProcessErrorHandlerAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Subscribing to Service Bus queue.");

        await _processor.StartProcessingAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information(
            "Stopping background task {TaskName}.",
            nameof(ReportGenerationRequestedServiceBusQueueListener)
        );

        await _processor.StopProcessingAsync(cancellationToken);

        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        await _processor.DisposeAsync();
        await _client.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessMessageHandlerAsync(ProcessMessageEventArgs args)
    {
        if (args.CancellationToken.IsCancellationRequested)
        {
            Log.Information("Cancellation during processing requested, stopping processing.");
            return;
        }

        try
        {
            string message = args.Message.Body.ToString();
            Log.Information("Received: {Message} from queue.", message);

            var cloudEventMessage = JsonSerializer.Deserialize<CloudEvent<object>>(message);

            if (cloudEventMessage is null)
            {
                Log.Error("Deserializing message into Cloud Event resulted in null.");

                throw new NullReferenceException("Cloud event cannot be null.");
            }

            if (cloudEventMessage.Type == ReportGenerationEventTypes.ReportGenerationRequestedV1)
            {
                var eventModel = cloudEventMessage.Cast<ReportGenerationRequestedModel>();

                await _reportGenerationService.HandleReportGenerationRequestAsync(eventModel.Data);

                // Complete the message so that it is removed from the queue.
                await args.CompleteMessageAsync(args.Message);
            }
        }
        catch (Exception e)
        {
            Log.Error("Error handling message. '{ErrorMessage}'", e.Message);

            // We can't process the message so dead-letter it otherwise
            // we'll continually try to handle it and keep failing.
            await args.DeadLetterMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task ProcessErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        if (args.CancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        Log.Error(
            "The Service Bus subscription received an error processing a message, errorSource: {ErrorSource}, entityPath: {EntityPath}, fullyQualifiedNamespace: {FullyQualifiedNamespace}",
            args.ErrorSource.ToString(),
            args.EntityPath,
            args.FullyQualifiedNamespace
        );

        return Task.CompletedTask;
    }
}
