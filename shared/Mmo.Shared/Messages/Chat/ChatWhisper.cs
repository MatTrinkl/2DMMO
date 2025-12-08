using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.Chat;

[MessagePackObject]
public class ChatWhisper : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ChatWhisper()
    {
    }

    /// <summary>
    ///     Creates a new Chat Whisper Message.
    /// </summary>
    /// <param name="entityId">Entity which send the Message.</param>
    /// <param name="message">The message broadcasted to all clients.</param>
    /// <param name="recipientId">The Recipient of the chat message.</param>
    public ChatWhisper(Guid entityId, string message, Guid recipientId)
    {
        EntityId = entityId;
        Message = message;
        RecipientId = recipientId;
    }

    /// <summary>
    ///     The Entity who is sending the whisper message.
    /// </summary>
    [Key(1)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The context of this message.
    /// </summary>
    [Key(2)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     Recipient of the message.
    /// </summary>
    [Key(3)]
    public Guid RecipientId { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ChatWhisper;
}
