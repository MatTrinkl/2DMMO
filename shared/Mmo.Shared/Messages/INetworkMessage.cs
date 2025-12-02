using Mmo.Shared.Enums;

namespace Mmo.Shared.Messages;

public interface INetworkMessage
{
    MessageType Type { get; }
}
