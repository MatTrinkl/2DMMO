using MessagePack;
using Mmo.Shared.Enums;

namespace Mmo.Shared.Helper;

/// <summary>
/// This class helps to decode the MessageType while Deserializing an incoming message.
/// </summary>
[MessagePackObject]
public class MessageHeader
{
    /// <summary>
    /// Type of the incoming message.
    /// </summary>
    [Key(0)] public MessageType Type { get; set; }
}
