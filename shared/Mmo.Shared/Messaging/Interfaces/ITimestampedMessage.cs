using Mmo.Shared.Networking;

namespace Mmo.Shared.Messaging.Interfaces;

/// <summary>
///     This interface is an extension for the <see cref="INetworkMessage" /> when there needs to be a timestamp in the
///     message.
/// </summary>
public interface ITimestampedMessage : INetworkMessage
{
    /// <summary>
    ///     The timestamp of the message. Use <see cref="NetworkTime.Now"/> when creating the Message.
    /// </summary>
    long Timestamp { get; init; }
}
