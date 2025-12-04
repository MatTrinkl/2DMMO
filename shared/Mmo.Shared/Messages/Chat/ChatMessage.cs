using MessagePack;
using Mmo.Shared.Enums;
using Mmo.Shared.Messages.Interfaces;

namespace Mmo.Shared.Messages.Chat;

/// <summary>
///     This class is used when a client sends a Chat Message.
/// </summary>
[MessagePackObject]
public class ChatMessage : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ChatMessage()
    {
    }

    /// <summary>
    ///     Creates a new Chat Message.
    /// </summary>
    /// <param name="entityId">Entity which sends the Message.</param>
    /// <param name="message">The message broadcasted to all clients.</param>
    public ChatMessage(Guid entityId, string message)
    {
        EntityId = entityId;
        Message = message;
    }

    /// <summary>
    ///     The Entity who is sending the message to the server with the context in <see cref="Message" />.
    /// </summary>
    [Key(1)]
    public Guid EntityId { get; set; }

    /// <summary>
    ///     The context of this message.
    /// </summary>
    [Key(2)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.ChatMessage;
}
