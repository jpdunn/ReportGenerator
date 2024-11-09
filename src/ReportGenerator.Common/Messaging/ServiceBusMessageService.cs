using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Serilog;

namespace ReportGenerator.Common.Messaging;

public class ServiceBusMessageService : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusMessageService(string connectionString, string queueName)
    {
        // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443.
        _client = new(
            connectionString,
            new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets }
        );
        _sender = _client.CreateSender(queueName);
    }

    public async Task SendMessageAsync(object payload)
    {
        string payloadString = JsonSerializer.Serialize(payload);

        var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(payloadString));

        Log.Information("Sending message to service bus: {Message}", payloadString);

        await _sender.SendMessageAsync(message);
    }

    public async ValueTask DisposeAsync()
    {
        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
