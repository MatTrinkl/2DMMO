using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.System.Messages;

/// <summary>
///     General error message from server to client.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.ErrorMessage)]
public class ErrorMessage : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ErrorMessage()
    {
    }

    /// <summary>
    ///     Creates a new Ping Message. Client->Server
    /// </summary>
    /// <param name="code">Error code (machine-readable). E.g. "AUTH_FAILED", "INVALID_INPUT", "NOT_FOUND", "RATE_LIMITED".</param>
    /// <param name="message">User-friendly error message (localizable).</param>
    /// <param name="details">Optional additional details (for debugging).</param>
    /// <param name="field">Optional reference to the erroneous field (for forms).</param>
    public ErrorMessage(string code, string message, string? details, string? field)
    {
        Code = code;
        Message = message;
        Details = details;
        Field = field;
    }

    /// <summary>
    ///     Error code (machine-readable).
    ///     E.g. "AUTH_FAILED", "INVALID_INPUT", "NOT_FOUND", "RATE_LIMITED".
    /// </summary>
    [Key(1)]
    public string Code { get; set; } = "";

    /// <summary>
    ///     User-friendly error message (localizable).
    /// </summary>
    [Key(2)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     Optional additional details (for debugging).
    /// </summary>
    [Key(3)]
    public string? Details { get; set; }

    /// <summary>
    ///     Optional reference to the erroneous field (for forms).
    /// </summary>
    [Key(4)]
    public string? Field { get; set; }

    [Key(0)] public MessageType Type => MessageType.ErrorMessage;
}
