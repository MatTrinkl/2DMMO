using Mmo.Server.Entities;
using Mmo.Server.GameLoop;
using Mmo.Server.Messages;
using Mmo.Server.Networking;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;
using Mmo.Shared.Messages.ZoneEvents;

namespace Mmo.Server.MessageRouting.MessageHandler;

public class LoginHandler(GameServer gameServer, ILog log)
{
    private readonly GameServer _gameServer = gameServer;
    private readonly ILog _log = log;

    internal void Handle(ClientConnection connection, LoginRequest request)
    {
        // TODO: Replace with IAuthenticationService later
        // - Async validation BEFORE the Game Loop (in ClientConnection/AuthState)
        // - Load PlayerData from DB (IPlayerDataService.LoadPlayerDataAsync)
        // - Only then send LoginSuccessEvent to GameServer
        //
        // The Game Loop must NEVER make synchronous DB calls!
        // DB access must always be async in the Auth phase.
        
        // ═══════════════════════════════════════════════════
        // INPUT VALIDATION
        // ═══════════════════════════════════════════════════
        
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            _log.Warn("Login failed for {ConnectionId}: Username is empty", connection.Id);
            QueueLoginResponse(connection, false, null, "Username cannot be empty");
            return;
        }
        
        if (request.Username.Length < 3)
        {
            _log.Warn("Login failed for {ConnectionId}: Username too short (min 3 characters)", connection.Id);
            QueueLoginResponse(connection, false, null, "Username must be at least 3 characters");
            return;
        }
        
        if (request.Username.Length > 20)
        {
            _log.Warn("Login failed for {ConnectionId}: Username too long (max 20 characters)", connection.Id);
            QueueLoginResponse(connection, false, null, "Username must be at most 20 characters");
            return;
        }
        
        // TODO: Validate username characters (alphanumeric + allowed special chars)
        // TODO: Validate password (currently not checked - security issue!)
        // TODO: Check for duplicate username/session
        
        // ═══════════════════════════════════════════════════
        // SPAWN PLAYER
        // ═══════════════════════════════════════════════════
        
        // Spawns player in their last position or in the start zone
        PlayerEntity playerEntity = _gameServer.ZoneManager.SpawnPlayer(connection.Id, request.Username);
        var player = new ServerPlayer(playerEntity, connection);
        
        // Connect Player Entity with connection
        _gameServer.ZoneManager.AddPlayer(player); // TODO: Add ZoneID parameter later
        
        // Connect the connectionId to the persistentID of the Player entity
        _gameServer.NetworkServer.AssociatePlayer(connection, player.Entity.PersistentId);
        
        _log.Info("Player {Username} logged in successfully (ConnectionId: {ConnectionId}, PersistentId: {PersistentId})",
            request.Username, connection.Id, player.Entity.PersistentId);
        
        // Queue Response
        QueueLoginResponse(connection, true, player, null);
    }

    private void QueueLoginResponse(ClientConnection connection, bool success, ServerPlayer? player, string? error)
    {
        // Queue Login Response in the GameServer which will be processed in the Output phase
        ushort zoneId = player?.Entity.ZoneId ?? 0;
        var clientResponse = OutgoingMessage.ToClient(
            connection,
            new LoginResponse(success, connection.Id, zoneId, error)
        );
        _gameServer.QueueOutgoingMessage(clientResponse);
        
        if (!success)
        {
            return;
        }
        
        // Broadcast player joined to zone
        var broadcastPlayerConnectionToZone = OutgoingMessage.BroadcastToZone(
            new PlayerJoinedZone(player!.Entity),
            player.Entity.ZoneId
        );
        _gameServer.QueueOutgoingMessage(broadcastPlayerConnectionToZone);
    }

}
