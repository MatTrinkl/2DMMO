using Mmo.Server.Entities;
using Mmo.Server.GameLoop;
using Mmo.Server.Networking;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;

namespace Mmo.Server.MessageRouting.MessageHandler;

public class LoginHandler(GameServer gameServer, ILog log)
{
    private readonly ILog _log = log;
    private readonly GameServer _gameServer = gameServer;

    internal void Handle(Guid connectionId, LoginRequest request)
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
            QueueLoginResponse(connectionId, false, 0, "Invalid username");
            return;
        }

        //Spawns player in his last position or in the start region.
        var playerEntity = _gameServer.ZoneManager.SpawnPlayer(connectionId, request.Username);
        var player = new ServerPlayer(playerEntity, connectionId);
        //Connect Player Entity with connection
        _gameServer.ZoneManager.AddPlayer(player /*later with ZoneID*/);
        //Connect the connectionId to the persistentID of the Player entity
        gameServer.NetworkServer.AssociatePlayer(connectionId, player.Entity.PersistentId);

        // 4. Queue Response
        QueueLoginResponse(connectionId, true, player.Id, null);
    }

    private void QueueLoginResponse(Guid connectionId, bool success, int playerId, string? error)
    {
        //Queue Login Response in the GameServer which will be worked of in the Outputphase
    }
}
