using MessagePack;
using Mmo.Shared.Character.Messages.Server_Client;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Zones.Enums;

namespace Mmo.Shared.Zones.Messages.Client_Server;

/// <summary>
///     This is sent after the <see cref="CharacterSelectResponse"/> for loading the Spawn Zone.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.GetZoneRequest)]
public record GetZoneRequest : IClientMessage
{
    /// <inheritdoc/>
    [Key(0)]
    public MessageType Type => MessageType.GetZoneRequest;

    /// <summary>
    /// ID of the Zone which the character is spawning in.
    /// </summary>
    [Key(1)]
    public ushort ZoneId { get; set; }
}
