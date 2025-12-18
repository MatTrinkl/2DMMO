using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Connection;

/// <summary>
///     Response to a reconnect request.
/// </summary>
[MessagePackObject]
public class ReconnectResponse : INetworkMessage
{
    /// <summary>
    ///     The constructor used by <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ReconnectResponse()
    {
    }

    /// <summary>
    ///     Creates a new ReconnectResponse message.
    /// </summary>
    /// <param name="success">Whether the reconnect was successful.</param>
    /// <param name="error">Optional error message if failed.</param>
    public ReconnectResponse(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }

    /// <summary>
    ///     Whether the reconnect was successful.
    /// </summary>
    [Key(1)]
    public bool Success { get; set; }

    /// <summary>
    ///     Optional error message if reconnect failed.
    /// </summary>
    [Key(2)]
    public string? Error { get; set; }

    /// <summary>
    ///     Username of the reconnected account.
    /// </summary>
    [Key(3)]
    public string? Username { get; set; }

    /// <summary>
    ///     Account flags of the reconnected account.
    /// </summary>
    [Key(4)]
    public AccountFlags AccountFlags { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ReconnectResponse;
}
