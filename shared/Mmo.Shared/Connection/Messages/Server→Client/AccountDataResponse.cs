using MessagePack;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Connection.Messages.Server_Client;

/// <summary>
///     This class sends a response to a <see cref="RealmListRequest" /> to the client.
///     Server -> Client
///     Todo: We dont have different server currently but later at sometime.
///     Todo: DTO
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.AccountDataResponse)]
public class AccountDataResponse : IServerMessage
{
    /// <summary>
    ///     The ID of the account which uses this connection.
    /// </summary>
    [Key(1)]
    public long AccountId { get; set; }

    /// <summary>
    ///     The Email of the account which uses this connection.
    /// </summary>
    [Key(2)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Has this account Premium.
    /// </summary>
    [Key(3)]
    public bool IsPremium { get; set; }

    /// <summary>
    ///     When is the premium status gone.
    /// </summary>
    [Key(4)]
    public long? PremiumUntil { get; set; }

    /// <summary>
    ///     When was the account created.
    /// </summary>
    [Key(5)]
    public long CreatedAt { get; set; }

    /// <summary>
    ///     What is the total playtime of the account over all characters in seconds.
    /// </summary>
    [Key(6)]
    public long TotalPlaytime { get; set; }

    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.AccountDataResponse;
}
