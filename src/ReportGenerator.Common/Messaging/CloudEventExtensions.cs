using ReportGenerator.Common.Exceptions;
using System.Text.Json;
using Serilog;

namespace ReportGenerator.Common.Messaging;

public static class CloudEventExtensions
{
    /// <summary>
    /// Parses the cloud event using a defined type.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <param name="cloudEvent">The cloud event message.</param>
    public static CloudEvent<TData> Cast<TData>(this CloudEvent<object> cloudEvent)
        where TData : class
    {
        Log.Information("Parsing the Cloud Event. Id: {CloudEvent_Id}", cloudEvent.Id);

        try
        {
            return new CloudEvent<TData>(
                cloudEvent.Type,
                cloudEvent.Source,
                cloudEvent.Id,
                cloudEvent.SpecVersion,
                cloudEvent.DataSchema,
                cloudEvent.Subject,
                cloudEvent.Time,
                cloudEvent.DataContentType,
                Convert<TData>(cloudEvent.Data)
            );
        }
        catch (Exception ex)
        {
            string message =
                $"An error occurred while attempting to parse the Cloud Event message into the appropriate message type model. Message:\n{cloudEvent}";

            Log.Error(ex, message, cloudEvent);
            throw new UnrecoverableException(message, ex);
        }
    }

    /// <summary>
    /// TODO: Respect the data content type and the original issues identified around
    /// json deserialization.
    ///
    /// Cloud events support multiple data content types including string and number
    /// literals.
    ///      • If both the requested type and the value are of type string return the value
    ///      as a string.
    ///      • Otherwise we have an object or a string that can be converted to the requested type.
    /// </summary>
    /// <typeparam name="TData">The type to convert the value to.</typeparam>
    /// <param name="value">The object to be converted.</param>
    /// <returns>The original value converted to the requested type.</returns>
    /// <exception cref="InvalidCastException">If the json deserialization returns null.</exception>
    private static TData Convert<TData>(object value)
    {
        if (typeof(TData) == typeof(string) && value is string)
        {
            return (TData)value;
        }

#pragma warning disable CS8604 // Possible null reference argument.
        TData? val = JsonSerializer.Deserialize<TData>(value.ToString());
#pragma warning restore CS8604 // Possible null reference argument.

        if (val == null)
        {
            throw new InvalidCastException("Value could not be deserialized to the requested type.");
        }

        return val;
    }
}
