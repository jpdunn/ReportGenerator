namespace ReportGenerator.Common.Messaging;

public interface ICloudEvent<out TData>
{
    /// <summary>
    /// The type of message sent.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// The source of the message.
    /// </summary>
    Uri Source { get; }

    /// <summary>
    /// The Id as a GUID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The version specification.
    /// </summary>
    string SpecVersion { get; }

    /// <summary>
    /// The data schema.
    /// </summary>
    Uri? DataSchema { get; }

    /// <summary>
    /// The subject of the message.
    /// </summary>
    string? Subject { get; }

    /// <summary>
    /// The time the record was created/modified.
    /// </summary>
    DateTimeOffset? Time { get; }

    /// <summary>
    /// The data content type we are expecting.
    /// </summary>
    string? DataContentType { get; }

    /// <summary>
    /// The data model payload.
    /// </summary>
    TData Data { get; }
}
