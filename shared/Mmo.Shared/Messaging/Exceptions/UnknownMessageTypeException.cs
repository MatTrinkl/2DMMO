using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Messaging.Exceptions;

/// <summary>
///     This Exception needs to be thrown when there is an unknown <see cref="MessageType" /> while
///     DeSerialize a <see cref="INetworkMessage" />.
/// </summary>
public class UnknownMessageTypeException(MessageType type) : Exception($"Unknown message type: {type} ({(byte)type})")
{
    /// <summary>
    ///     The unknown MessageType.
    /// </summary>
    public MessageType Type { get; } = type;
}
