using Mmo.Server.Connections;
using Mmo.Server.PlayerService;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Network.Interfaces;

public interface IBroadcastService
{

    // Zone Broadcasts
    void BroadcastToZone(ushort zoneId, INetworkMessage message);
    void BroadcastToZoneExcept(ushort zoneId, Guid excludedClientId, INetworkMessage message);

    // Proximity Broadcasts
    void BroadcastInRange(ushort zoneId, Position center, float radius, INetworkMessage message);

    void BroadcastInRangeExcept(ushort zoneId, Position center, float radius, Guid excludedClientId,
        INetworkMessage message);

    // Targeted
    void SendToPlayer(ClientConnection client, INetworkMessage message);
    void SendToPlayers(IEnumerable<ClientConnection> clients, INetworkMessage message);
    void SendError(ClientConnection client, string code, string message, string? details, string? field);

    // Global
    void BroadcastGlobal(INetworkMessage message);
    void BroadcastGlobalExcept(Guid excludedClientId, INetworkMessage message);

    //Party
    void BroadcastToParty(ServerPlayerCharacter characterInParty,INetworkMessage message);
    void BroadcastToPartyExcept(ServerPlayerCharacter characterInPartyAndToExcluded, INetworkMessage message);

    //Guild
    void BroadcastToGuild(ServerPlayerCharacter characterInGuild,INetworkMessage message);
    void BroadcastToGuildExcept(ServerPlayerCharacter characterInGuildAndToExcluded, INetworkMessage message);

}
