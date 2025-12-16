using Mmo.Server.Entities;
using Mmo.Server.GameLoop;
using Mmo.Server.Networking;
using Mmo.Shared.Entities;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;

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
            QueueLoginResponse(connection, false, 0, "Invalid username");
            return;
        }

        //Spawns player in his last position or in the start region.
        PlayerEntity playerEntity = _gameServer.ZoneManager.SpawnPlayer(connection.Id, request.Username);
        var player = new ServerPlayer(playerEntity, connection);
        //Connect Player Entity with connection
        _gameServer.ZoneManager.AddPlayer(player /*later with ZoneID*/);
        //Connect the connectionId to the persistentID of the Player entity
        gameServer.NetworkServer.AssociatePlayer(connection, player.Entity.PersistentId);

        // 4. Queue Response
        QueueLoginResponse(connection, true, player.RuntimeId.LocalId, null);
    }

    private void QueueLoginResponse(ClientConnection connectionId, bool success, int playerId, string? error)
    {
        //Queue Login Response in the GameServer which will be worked of in the Outputphase
    }
}
