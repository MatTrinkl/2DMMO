namespace Mmo.Shared.Messages.Interfaces;

/// <summary>
///     This interface is an extension for the <see cref="INetworkMessage" /> when there needs to be a timestamp in the
///     message.
/// </summary>
public interface ITimestampedMessage : INetworkMessage
{
    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    long Timestamp { get; set; }
}
