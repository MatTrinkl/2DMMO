using Mmo.Server.Connections;
using Mmo.Server.PlayerService;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Network.Interfaces;

public interface IBroadcastService
{
    // Zone Broadcasts
    void BroadcastToZone<T>(ushort zoneId, T message) where T : INetworkMessage;
    void BroadcastToZoneExcept<T>(ushort zoneId, Guid excludedClientId, T message) where T : INetworkMessage;

    // Proximity Broadcasts
    void BroadcastInRange<T>(ushort zoneId, Position center, float radius, T message) where T : INetworkMessage;

    void BroadcastInRangeExcept<T>(ushort zoneId, Position center, float radius, Guid excludedClientId,
        T message) where T : INetworkMessage;

    // Targeted
    void SendToPlayer<T>(ClientConnection client, T message) where T : INetworkMessage;
    void SendToPlayers<T>(IEnumerable<ClientConnection> clients, T message) where T : INetworkMessage;
    void SendError(ClientConnection client, string code, string message, string? details, string? field);

    // Global
    void BroadcastGlobal<T>(T message) where T : INetworkMessage;
    void BroadcastGlobalExcept<T>(Guid excludedClientId, T message) where T : INetworkMessage;

    //Party
    void BroadcastToParty<T>(ServerPlayerCharacter characterInParty, T message) where T : INetworkMessage;
    void BroadcastToPartyExcept<T>(ServerPlayerCharacter characterInPartyAndToExcluded, T message) where T : INetworkMessage;

    //Guild
    void BroadcastToGuild<T>(ServerPlayerCharacter characterInGuild, T message) where T : INetworkMessage;
    void BroadcastToGuildExcept<T>(ServerPlayerCharacter characterInGuildAndToExcluded, T message) where T : INetworkMessage;
}
