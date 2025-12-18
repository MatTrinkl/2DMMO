using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Chat;

/// <summary>
///     This Message is send to all clients after the server got a chat message.
/// </summary>
[MessagePackObject]
public class ChatBroadcast : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ChatBroadcast()
    {
    }

    /// <summary>
    ///     Creates a new ChatBroadcastMessage.
    /// </summary>
    /// <param name="entityId">Entity which send the Message.</param>
    /// <param name="message">The message broadcasted to all clients.</param>
    public ChatBroadcast(Guid entityId, string message)
    {
        EntityId = entityId;
        Message = message;
    }

    /// <summary>
    ///     The Entity who is sending the original message which gets broadcasted with the context in <see cref="Message" />.
    /// </summary>
    [Key(1)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The context of this broadcast.
    /// </summary>
    [Key(2)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ChatBroadcast;
}
