using Mmo.Server.GameLoop;
using Mmo.Server.MessageRouting.MessageHandler;
using Mmo.Server.Networking;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;

namespace Mmo.Server.MessageRouting;

public class MessageRouter(GameServer gameServer, ILog log)
{
    private readonly GameServer _gameServer = gameServer;
    private readonly ILog _log = log;
    private readonly LoginHandler _loginHandler = new(gameServer, log);


    public void Route(ClientConnection connection, INetworkMessage message)
    {
        switch (message.Type)
        {
            case MessageType.LoginRequest:
                _loginHandler.Handle(connection, (LoginRequest)message);
                break;
            case MessageType.LoginResponse:
            case MessageType.LogoutRequest:
            case MessageType.Heartbeat:
            case MessageType.Disconnect:
            case MessageType.JoinZone:
            case MessageType.LeaveZone:
            case MessageType.ZoneState:
            case MessageType.PlayerJoinedZone:
            case MessageType.PlayerLeftZone:
            case MessageType.PositionUpdate:
            case MessageType.PositionBroadcast:
            case MessageType.ActionRequest:
            case MessageType.ActionResult:
            case MessageType.DamageEvent:
            case MessageType.DeathEvent:
            case MessageType.ChatMessage:
            case MessageType.ChatBroadcast:
            case MessageType.ChatWhisper:
            case MessageType.Ping:
            case MessageType.Pong:
            default:
                _log.Warn("Unknown message type: {Type} from {ConnectionId}",
                    message.Type, connection.Id);
                break;
        }
    }
}
