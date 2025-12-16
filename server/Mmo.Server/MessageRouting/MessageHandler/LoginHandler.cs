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
        // TODO: Später durch IAuthenticationService ersetzen
        // - Async Validierung VOR dem Game Loop (in ClientConnection/AuthState)
        // - PlayerData aus DB laden (IPlayerDataService. LoadPlayerDataAsync)
        // - Erst dann LoginSuccessEvent an GameServer senden
        //
        // Der Game Loop darf KEINE synchronen DB-Calls machen!
        // DB-Zugriff muss immer async in der Auth-Phase passieren.
        if (string.IsNullOrWhiteSpace(request.Username) ||
            request.Username.Length < 3 ||
            request.Username.Length > 20)
        {
            QueueLoginResponse(connection, false, null, "Invalid username");
            return;
        }

        //Spawns player in his last position or in the start region.
        PlayerEntity playerEntity = _gameServer.ZoneManager.SpawnPlayer(connection.Id, request.Username);
        var player = new ServerPlayer(playerEntity, connection);
        //Connect Player Entity with connection
        _gameServer.ZoneManager.AddPlayer(player /*later with ZoneID*/);
        //Connect the connectionId to the persistentID of the Player entity
        _gameServer.NetworkServer.AssociatePlayer(connection, player.Entity.PersistentId);

        // 4. Queue Response
        QueueLoginResponse(connection, true, player, null);
    }

    private void QueueLoginResponse(ClientConnection connection, bool success, ServerPlayer? player, string? error)
    {
        //Queue Login Response in the GameServer which will be worked of in the Outputphase
        var clientResponse =
            OutgoingMessage.ToClient(connection, new LoginResponse(success, connection.Id, (player==null?(ushort)0:player.Entity.ZoneId), error));
        _gameServer.QueueOutgoingMessage(clientResponse);
        if (!success) return;
        var broadcastPlayerConnectionToZone = OutgoingMessage.BroadcastToZone(new PlayerJoinedZone(player!.Entity), player!.Entity.ZoneId);
        _gameServer.QueueOutgoingMessage(broadcastPlayerConnectionToZone);
    }

}
