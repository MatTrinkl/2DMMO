using Mmo.Server.GameLoop;
using Mmo.Server.MessageRouting.MessageHandler;
using Mmo.Server.Networking;
using Mmo.Shared.Enums;
using Mmo.Shared.Interfaces;
using Mmo.Shared.Messages.Connection;

namespace Mmo.Server.MessageRouting;

/// <summary>
/// Routes incoming network messages to their appropriate handlers based on message type.
/// </summary>
/// <remarks>
/// The MessageRouter categorizes messages into three groups:
/// <list type="bullet">
/// <item><description>Client → Server: Messages that should be processed by the server (LoginRequest, PositionUpdate, etc.)</description></item>
/// <item><description>Server → Client: Messages that should never be received from clients (LoginResponse, broadcasts, etc.)</description></item>
/// <item><description>Bidirectional: Messages that can flow in both directions (JoinZone, LeaveZone)</description></item>
/// </list>
/// <para>
/// Unimplemented handlers log at Debug level to indicate missing functionality without cluttering production logs.
/// Invalid messages (e.g., server-to-client messages received from clients) log at Warn level.
/// </para>
/// </remarks>
public class MessageRouter(GameServer gameServer, ILog log)
{
    private readonly GameServer _gameServer = gameServer;
    private readonly ILog _log = log;
    private readonly LoginHandler _loginHandler = new(gameServer, log);

    /// <summary>
    /// Routes an incoming network message to its appropriate handler.
    /// </summary>
    /// <param name="connection">The client connection that sent the message.</param>
    /// <param name="message">The network message to route.</param>
    /// <remarks>
    /// Messages are routed based on their <see cref="MessageType"/>. Unimplemented handlers
    /// log at Debug level with a TODO comment. Invalid messages (server-to-client messages
    /// received from clients) log at Warn level.
    /// </remarks>
    public void Route(ClientConnection connection, INetworkMessage message)
    {
        switch (message.Type)
        {
            // ═══════════════════════════════════════════════════
            // CLIENT → SERVER Messages (These are handled here)
            // ═══════════════════════════════════════════════════
            
            case MessageType.LoginRequest:
                _loginHandler.Handle(connection, (LoginRequest)message);
                break;
            
            // TODO: Implement handlers for these client-to-server messages:
            case MessageType.LogoutRequest:
                _log.Debug("LogoutRequest from {ConnectionId} - Handler not yet implemented", connection.Id);
                // TODO: Create LogoutHandler
                break;
                
            case MessageType.Heartbeat:
                // Heartbeat is handled by ClientConnection timeout logic
                // No logging needed - this happens every few seconds per client
                break;
                
            case MessageType.PositionUpdate:
                _log.Debug("PositionUpdate from {ConnectionId} - Handler not yet implemented", connection.Id);
                // TODO: Create PositionUpdateHandler
                break;
                
            case MessageType.ActionRequest:
                _log.Debug("ActionRequest from {ConnectionId} - Handler not yet implemented", connection.Id);
                // TODO: Create ActionRequestHandler
                break;
                
            case MessageType.ChatMessage:
                _log.Debug("ChatMessage from {ConnectionId} - Handler not yet implemented", connection.Id);
                // TODO: Create ChatMessageHandler
                break;
                
            case MessageType.Ping:
                _log.Debug("Ping from {ConnectionId} - Handler not yet implemented", connection.Id);
                // TODO: Create PingHandler
                break;
            
            // ═══════════════════════════════════════════════════
            // SERVER → CLIENT Messages (Should NEVER be received)
            // ═══════════════════════════════════════════════════
            
            case MessageType.LoginResponse:
            case MessageType.Disconnect:
            case MessageType.ZoneState:
            case MessageType.PlayerJoinedZone:
            case MessageType.PlayerLeftZone:
            case MessageType.PositionBroadcast:
            case MessageType.ActionResult:
            case MessageType.DamageEvent:
            case MessageType.DeathEvent:
            case MessageType.ChatBroadcast:
            case MessageType.ChatWhisper:
            case MessageType.Pong:
                _log.Warn("Received server-to-client message type {Type} from {ConnectionId} - This should not happen",
                    message.Type, connection.Id);
                break;
            
            // ═══════════════════════════════════════════════════
            // BIDIRECTIONAL or ZONE Messages (Future implementation)
            // ═══════════════════════════════════════════════════
            
            case MessageType.JoinZone:
            case MessageType.LeaveZone:
                _log.Debug("{Type} from {ConnectionId} - Handler not yet implemented", 
                    message.Type, connection.Id);
                // TODO: Implement when zone transfer system is ready
                break;
            
            // ═══════════════════════════════════════════════════
            // UNKNOWN Messages
            // ═══════════════════════════════════════════════════
            
            default:
                _log.Warn("Unknown message type: {Type} from {ConnectionId}",
                    message.Type, connection.Id);
                break;
        }
    }
}
