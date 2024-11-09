using System.ComponentModel.DataAnnotations;

namespace ReportGenerator.Common.Messaging;

public class CloudEvent<TData> : ICloudEvent<TData>
    where TData : class
{
    /// <summary>
    /// The type of message sent.
    /// </summary>
    [MinLength(1)]
    public string Type { get; set; }

    /// <summary>
    /// The source of the message.
    /// </summary>
    public Uri Source { get; set; }

    /// <summary>
    /// The Id as a GUID.
    /// </summary>
    [MinLength(1)]
    public string Id { get; set; }

    /// <summary>
    /// The version specification.
    /// </summary>
    [MinLength(1)]
    public string SpecVersion { get; set; }

    /// <summary>
    /// The data schema.
    /// </summary>
    public Uri? DataSchema { get; set; }

    /// <summary>
    /// The subject of the message.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// The time the record was created/modified.
    /// </summary>
    public DateTimeOffset? Time { get; set; }

    /// <summary>
    /// The data content type we are expecting.
    /// </summary>
    public string? DataContentType { get; set; }

    /// <summary>
    /// The data model payload.
    /// </summary>
    public TData Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudEvent{TData}"/> class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="source">The source.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="specVersion">The spec version.</param>
    /// <param name="dataSchema">The data schema.</param>
    /// <param name="subject">The subject.</param>
    /// <param name="time">The time.</param>
    /// <param name="dataContentType">Type of the data content.</param>
    /// <param name="data">The data.</param>
    public CloudEvent(
        string? type,
        Uri? source,
        string? id,
        string? specVersion,
        Uri? dataSchema,
        string? subject,
        DateTimeOffset? time,
        string? dataContentType,
        TData? data
    )
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        SpecVersion = specVersion ?? throw new ArgumentNullException(nameof(specVersion));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        DataSchema = dataSchema;
        Subject = subject;
        Time = time;
        DataContentType = dataContentType;
    }
}
