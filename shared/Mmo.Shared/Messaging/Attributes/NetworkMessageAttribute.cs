using Mmo.Shared.Messaging.Enums;

namespace Mmo.Shared.Messaging.Attributes;

/// <summary>
///     Attribute to mark a class as a network message with its corresponding MessageType.
///     Used for automatic message registration in the MessageSerializer.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NetworkMessageAttribute(MessageType type) : Attribute
{
    /// <summary>
    ///     The MessageType associated with this network message.
    /// </summary>
    public MessageType Type { get; } = type;
}
