using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.AsyncTask.Interface;
using Mmo.Server.Connections.Records;
using Mmo.Server.MessageRouting.Handler;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Player;
using Mmo.Server.Player.Interfaces;
using Mmo.Shared.Authentification.Interfaces;
using Mmo.Shared.Authentification.Records;
using Mmo.Shared.Connection.Messages.Client_Server;
using Mmo.Shared.Connection.Messages.Server_Client;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.Connections.MessageHandler;

/// <summary>
///     Handler for Connection category (0000-0099).
///     Processes:
///     - Login / Logout / Reconnect
///     - Character Selection (List, Select, Create, Delete)
///     - Heartbeat
///     IMPORTANT:
///     - Authentication and DB access run async via ctx.RunAsync()
///     - Handler methods themselves are synchronous (void)
///     - Responses are queued and sent in Output phase
/// </summary>
public class ConnectionHandler(
    IServiceProvider services,
    ILog log)
    : BaseCategoryHandler(log)
{
    private readonly IAsyncTaskService _asyncTask = services.GetRequiredService<IAsyncTaskService>();
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly IAuthenticationService _authService = services.GetRequiredService<IAuthenticationService>();
    private readonly IBroadcastService _broadcast = services.GetRequiredService<IBroadcastService>();
    private readonly ILog _log = log;
    private readonly IPlayerService _playerService = services.GetRequiredService<IPlayerService>();

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    public override MessageCategory Category => MessageCategory.Connection;

    // ═══════════════════════════════════════════════════════════════
    // PROTECTED METHODS
    // ═══════════════════════════════════════════════════════════════

    protected override void RegisterHandlers()
    {
        // Login / Logout
        Register<LoginRequest>(MessageType.LoginRequest, HandleLoginRequest);
        //Register<LogoutRequest>(MessageType.LogoutRequest, HandleLogoutRequest);
        //Register<ReconnectRequest>(MessageType.ReconnectRequest, HandleReconnectRequest);

        // Heartbeat
        //Register<Heartbeat>(MessageType.Heartbeat, HandleHeartbeat);
        //Register<HeartbeatResponse>(MessageType.HeartbeatResponse, HandleHeartbeatResponse);

        // Character Selection
        //Register<CharacterListRequest>(MessageType.CharacterListRequest, HandleCharacterListRequest);
        //Register<CharacterSelect>(MessageType.CharacterSelect, HandleCharacterSelect);
        //Register<CharacterCreate>(MessageType.CharacterCreate, HandleCharacterCreate);
        //Register<CharacterDelete>(MessageType.CharacterDelete, HandleCharacterDelete);

        // Disconnect
        // Register<Disconnect>(MessageType.Disconnect, HandleDisconnect);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - Login / Logout / Reconnect
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    ///     Processes a login request.
    ///     Authentication runs asynchronously in the background.
    /// </summary>
    private void HandleLoginRequest(MessageContext ctx, LoginRequest request)
    {
        // Store what we need (Connection cannot change)
        Guid connectionId = ctx.Connection.Id;
        ClientConnection connection = ctx.Connection;

        // ════════════════════════════════════════════════════════════
        // ASYNC TASK with Result
        // ════════════════════════════════════════════════════════════
        _asyncTask.Run(
            connectionId,

            // 1. ASYNC PART (runs on ThreadPool)
            async () =>
            {
                AuthResult authResult = await _authService.AuthenticateAsync(
                    request.Username,
                    request.Password
                );

                if (!authResult.Success)
                {
                    _log.Error(authResult.ErrorCode!.ToString(), "Auth service error for {ConnectionId}",
                        ctx.ConnectionId);
                    _broadcast.SendError(ctx.Connection, "AUTH_SERVICE_ERROR",
                        "Authentication service unavailable.  Please try again later.", null, null);
                    return new LoginTaskResult(false, ErrorCode: authResult.ErrorCode,
                        ErrorMessage: authResult.ErrorMessage);
                }

                // Authentication successful - Update connection state
                Guid sessionToken = IdRegistry.Instance.GeneratePersistentId();
                ctx.Connection.SetAuthenticated(
                    authResult.AccountId!.Value,
                    authResult.Username ?? request.Username,
                    authResult.Flags,
                    sessionToken
                );
                _log.Info("Login successful for {ConnectionId}: {Username} (AccountId: {AccountId})",
                    ctx.ConnectionId, authResult.Username!, authResult.AccountId);

                // Spawn player (also async)
                ServerPlayerCharacter player = await _playerService.SpawnPlayerAsync(
                    authResult.AccountId!.Value,
                    request.Username,
                    connection
                );

                return new LoginTaskResult(true, player);
            },

            // 2. COMPLETION CALLBACK (runs in Game Loop with fresh ctx)
            (outCtx, result) =>
            {
                // ctx is FRESH - now also has ctx.Player if spawned!
                if (result.Success)
                {
                    // Send response
                    // TODO: Add session token, character list, and server info to response
                    _broadcast.SendToPlayer(outCtx.Connection,
                        new LoginResponse
                        {
                            Success = true, AccountId = outCtx.PlayerInfo!.AccountId,
                            AccountName = outCtx.PlayerInfo.Name, IsPremium = false, SessionToken = "TempToken"
                        });

                    _log.Info("Player {Name} logged in", outCtx.ServerPlayer!.Name);
                }
                else
                {
                    _log.Warn("Login failed for {ConnectionId}: {Error}", ctx.ConnectionId, result.ErrorCode!);
                    _broadcast.SendToPlayer(outCtx.Connection,
                        new LoginResponse
                        {
                            Success = false, ErrorCode = result.ErrorCode, ErrorMessage = result.ErrorMessage
                        });
                }
            }
        );
    }


    private static string? ValidateCharacterName(string? name)
    {
        //Todo: to CharacterCreationService
        if (string.IsNullOrWhiteSpace(name))
            return "Character name cannot be empty";

        if (name.Length < 2)
            return "Character name must be at least 2 characters";

        if (name.Length > 16)
            return "Character name must be at most 16 characters";

        // TODO: Regex for allowed characters
        // if (!Regex. IsMatch(name, "^[a-zA-Z]+$"))
        //     return "Character name can only contain letters";

        // TODO: Banned names check

        return null;
    }
}
