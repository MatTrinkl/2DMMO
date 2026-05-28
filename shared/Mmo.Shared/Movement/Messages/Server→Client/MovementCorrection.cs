using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Movement.Enums;
using Mmo.Shared.Movement.Records;

namespace Mmo.Shared.Movement.Messages.Server_Client;

/// <summary>
///     This class is sent back to the clients correcting the calculated position.
/// </summary>
[MessagePackObject]
[NetworkMessage(MessageType.MovementCorrection)]
public class MovementCorrection : IServerMessage, ITimestampedMessage
{
    /// <summary>
    ///     The Message Type of this Message.
    /// </summary>
    [Key(0)]
    public MessageType Type => MessageType.MovementCorrection;


    /// <summary>
    ///     The timestamp of the message.
    /// </summary>
    [Key(1)]
    public long Timestamp { get; init; }

    /// <summary>
    ///     The corrected position of the entity.
    /// </summary>
    [Key(3)]
    public required Position CorrectedPosition { get; set; }

    /// <summary>
    ///     The corrected velocity of the entity.
    /// </summary>
    [Key(4)]
    public required Velocity CorrectedVelocity { get; set; }

    /// <summary>
    /// The reason for the corrections.
    /// </summary>
    [Key(5)]
    public MovementCorrectionReason? Reason { get; init; }

    /// <summary>
    /// The number which was corrected.
    /// </summary>
    [Key(6)]
    public uint SequenceNumber { get; init; }
}
