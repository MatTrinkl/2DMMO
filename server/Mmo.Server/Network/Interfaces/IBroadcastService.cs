using Mmo.Server.Connections;
using Mmo.Server.PlayerService;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Server.Network.Interfaces;

/// <summary>
///     Service interface for broadcasting messages to players and zones.
///     Provides methods for targeted, zone-based, proximity, and global messaging.
/// </summary>
public interface IBroadcastService
{
    // Zone Broadcasts
    /// <summary>Broadcasts a message to all players in a zone.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="zoneId">The zone ID.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastToZone<T>(ushort zoneId, T message) where T : INetworkMessage;

    /// <summary>Broadcasts a message to all players in a zone except one.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="zoneId">The zone ID.</param>
    /// <param name="excludedClientId">The client ID to exclude.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastToZoneExcept<T>(ushort zoneId, Guid excludedClientId, T message) where T : INetworkMessage;

    // Proximity Broadcasts
    /// <summary>Broadcasts a message to all players within a radius of a position.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="zoneId">The zone ID.</param>
    /// <param name="center">The center position.</param>
    /// <param name="radius">The broadcast radius.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastInRange<T>(ushort zoneId, Position center, float radius, T message) where T : INetworkMessage;

    /// <summary>Broadcasts a message to all players within a radius except one.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="zoneId">The zone ID.</param>
    /// <param name="center">The center position.</param>
    /// <param name="radius">The broadcast radius.</param>
    /// <param name="excludedClientId">The client ID to exclude.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastInRangeExcept<T>(ushort zoneId, Position center, float radius, Guid excludedClientId,
        T message) where T : INetworkMessage;

    // Targeted
    /// <summary>Sends a message to a specific player.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="client">The client connection.</param>
    /// <param name="message">The message to send.</param>
    void SendToPlayer<T>(ClientConnection client, T message) where T : INetworkMessage;

    /// <summary>Sends a message to multiple specific players.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="clients">The client connections.</param>
    /// <param name="message">The message to send.</param>
    void SendToPlayers<T>(IEnumerable<ClientConnection> clients, T message) where T : INetworkMessage;

    /// <summary>Sends an error message to a player.</summary>
    /// <param name="client">The client connection.</param>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="details">Optional error details.</param>
    /// <param name="field">Optional field reference.</param>
    void SendError(ClientConnection client, string code, string message, string? details, string? field);

    // Global
    /// <summary>Broadcasts a message to all connected players.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastGlobal<T>(T message) where T : INetworkMessage;

    /// <summary>Broadcasts a message to all connected players except one.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="excludedClientId">The client ID to exclude.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastGlobalExcept<T>(Guid excludedClientId, T message) where T : INetworkMessage;

    // Party
    /// <summary>Broadcasts a message to all members of a player's party.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="characterInParty">A character in the party.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastToParty<T>(ServerPlayerCharacter characterInParty, T message) where T : INetworkMessage;

    /// <summary>Broadcasts a message to all party members except one.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="characterInPartyAndToExcluded">The character in party to exclude.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastToPartyExcept<T>(ServerPlayerCharacter characterInPartyAndToExcluded, T message) where T : INetworkMessage;

    // Guild
    /// <summary>Broadcasts a message to all members of a player's guild.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="characterInGuild">A character in the guild.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastToGuild<T>(ServerPlayerCharacter characterInGuild, T message) where T : INetworkMessage;

    /// <summary>Broadcasts a message to all guild members except one.</summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="characterInGuildAndToExcluded">The character in guild to exclude.</param>
    /// <param name="message">The message to broadcast.</param>
    void BroadcastToGuildExcept<T>(ServerPlayerCharacter characterInGuildAndToExcluded, T message) where T : INetworkMessage;
}
