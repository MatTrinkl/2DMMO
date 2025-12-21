using Microsoft.Extensions.DependencyInjection;
using Mmo.Server.AsyncTask.Interface;
using Mmo.Server.Connections.Records;
using Mmo.Server.MessageRouting.Handler;
using Mmo.Server.Messages;
using Mmo.Server.Network.Interfaces;
using Mmo.Server.Player.Interfaces;
using Mmo.Server.PlayerService;
using Mmo.Server.Zones;
using Mmo.Shared.Authentification.Interfaces;
using Mmo.Shared.Authentification.Records;
using Mmo.Shared.Connection.Messages;
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
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly IAuthenticationService _authService = services.GetRequiredService<IAuthenticationService>();
    private readonly ILog _log = log;
    private readonly IPlayerService _playerService = services.GetRequiredService<IPlayerService>();
    private readonly IAsyncTaskService _asyncTask = services.GetRequiredService<IAsyncTaskService>();
    private readonly IBroadcastService _broadcast = services.GetRequiredService<IBroadcastService>();

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
        // Speichere was wir brauchen (Connection kann sich nicht ändern)
        Guid connectionId = ctx.Connection.Id;
        ClientConnection connection = ctx.Connection;

        // ════════════════════════════════════════════════════════════
        // ASYNC TASK mit Result
        // ════════════════════════════════════════════════════════════
        _asyncTask.Run(
            connectionId,

            // 1. ASYNC TEIL (läuft auf ThreadPool)
            async () =>
            {
                AuthResult authResult = await _authService.AuthenticateAsync(
                    request.Username,
                    request.Password
                );

                if (!authResult.Success)
                {
                    _log.Error(authResult.Error!, "Auth service error for {ConnectionId}", ctx.ConnectionId);
                    _broadcast.SendError(ctx.Connection, "AUTH_SERVICE_ERROR",
                        "Authentication service unavailable.  Please try again later.", null, null);
                    return new LoginTaskResult(false, Error: authResult.Error);
                }
                // Authentication successful - Update connection state
                Guid sessionToken = IdRegistry.Instance.GeneratePersistentId();
                ctx.Connection.SetAuthenticated(
                    authResult.AccountId!.Value,
                    authResult.Username??request.Username,
                    authResult.Flags,
                    sessionToken
                );
                _log.Info("Login successful for {ConnectionId}: {Username} (AccountId: {AccountId})",
                    ctx.ConnectionId, authResult.Username!, authResult.AccountId);

                // Player spawnen (auch async)
                ServerPlayerCharacter player = await _playerService.SpawnPlayerAsync(
                    authResult.AccountId!.Value,
                    request.Username,
                    connection
                );

                return new LoginTaskResult(true, Player: player);
            },

            // 2. COMPLETION CALLBACK (läuft im Game Loop mit frischem ctx)
            (outCtx, result) =>
            {
                // ctx ist FRISCH - hat jetzt auch ctx.Player falls gespawnt!
                if (result.Success)
                {
                    // Send response
                    // TODO: Add session token, character list, and server info to response
                    _broadcast.SendToPlayer(outCtx.Connection,
                        new LoginResponse(true, outCtx.ConnectionId, outCtx.ServerPlayer!.RuntimeId.ZoneId, null));

                    _log.Info("Player {Name} logged in", outCtx.ServerPlayer!.Name);
                }
                else
                {
                    _log.Warn("Login failed for {ConnectionId}: {Error}", ctx.ConnectionId, result.Error!);
                    _broadcast.SendToPlayer(outCtx.Connection,
                        new LoginResponse(false, outCtx.ConnectionId, 0, result.Error));
                }
            }
        );
    }

    /// <summary>
    ///     Verarbeitet einen Logout-Request.
    /// </summary>
    /* private void HandleLogoutRequest(MessageContext ctx, LogoutRequest request)
     {
         if (!RequireAuthenticated(ctx)) return;

         _log.Info("Logout request from {ConnectionId}:  {Username}", ctx.ConnectionId, ctx.Connection.Username);

         // ─── Spieler aus Welt entfernen (falls InGame) ───
         if (ctx.HasCharacter && ctx.PlayerInfo != null)
         {
             // Broadcast an Zone
             ctx.BroadcastToZoneExceptSelf(new PlayerLeftZone(ctx.PlayerId!.Value));

             // Spieler-Daten speichern (async, Fire-and-Forget)
             var saveTask = _playerService.SaveAndRemovePlayerAsync(ctx.ConnectionId);
             ctx.RunAsync(saveTask,
                 onCompleted: _ => _log.Debug("Player data saved for {ConnectionId}", ctx.ConnectionId),
                 onError: (_, ex) => _log.Error(ex, "Failed to save player data for {ConnectionId}", ctx.ConnectionId)
             );
         }

         // ─── Session invalidieren (async, Fire-and-Forget) ───
         if (ctx.Connection.SessionToken.HasValue)
         {
             var invalidateTask = _authService.InvalidateSessionAsync(ctx.Connection.SessionToken.Value);
             ctx.RunAsync(invalidateTask,
                 onCompleted: _ => { },
                 onError: (_, ex) => _log.Error(ex, "Failed to invalidate session for {ConnectionId}", ctx.ConnectionId)
             );
         }

         // ─── Connection-State zurücksetzen ───
         ctx.Connection.ResetAuth();

         // ─── Response senden ───
         ctx.Send(new LogoutResponse(true));
     }

     /// <summary>
     ///     Verarbeitet einen Reconnect-Request.
     /// </summary>
     private void HandleReconnectRequest(MessageContext ctx, ReconnectRequest request)
     {
         if (ctx.IsAuthenticated)
         {
             ctx.SendError("ALREADY_AUTHENTICATED", "You are already logged in");
             return;
         }

         _log.Debug("Reconnect request from {ConnectionId} with token {Token}", ctx.ConnectionId, request.SessionToken);

         // ─── Session validieren (async) ───
         var validateTask = _authService.ValidateSessionAsync(request.SessionToken);

         ctx.RunAsync(validateTask,
             onCompleted: (ctx, authResult) => OnReconnectValidated(ctx, authResult, request.SessionToken),
             onError: (ctx, ex) =>
             {
                 _log.Error(ex, "Session validation error for {ConnectionId}", ctx.ConnectionId);
                 ctx.Send(new ReconnectResponse(false, "Session validation failed"));
             }
         );
     }

     private void OnReconnectValidated(MessageContext ctx, AuthResult authResult, Guid sessionToken)
     {
         if (!authResult.Success)
         {
             _log.Warn("Reconnect failed for {ConnectionId}: {Error}", ctx.ConnectionId, authResult.Error);
             ctx.Send(new ReconnectResponse(false, authResult.Error ?? "Invalid or expired session"));
             return;
         }

         // Session gültig - State updaten
         ctx.Connection.SetAuthenticated(
             authResult.AccountId!.Value,
             authResult.Username!,
             authResult.Flags,
             sessionToken
         );

         _log.Info("Reconnect successful for {ConnectionId}: {Username}", ctx.ConnectionId, authResult.Username);

         ctx.Send(new ReconnectResponse(true, null)
         {
             Username = authResult.Username,
             AccountFlags = authResult.Flags
         });
     }

     // ═══════════════════════════════════════════════════════════════
     // HEARTBEAT
     // ═══════════════════════════════════════════════════════════════

     /// <summary>
     ///     Server sendet Heartbeat an Client (für Keep-Alive).
     ///     Wird vom HeartbeatService aufgerufen, nicht vom Client.
     /// </summary>
     private void HandleHeartbeat(MessageContext ctx, Heartbeat request)
     {
         // Client sollte eigentlich keinen Heartbeat senden, aber wir antworten trotzdem
         ctx.Send(new HeartbeatResponse
         {
             ServerTimestamp = request.ServerTimestamp,
             ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
         });
     }

     /// <summary>
     ///     Client antwortet auf Heartbeat (für Latenz-Messung).
     /// </summary>
     private void HandleHeartbeatResponse(MessageContext ctx, HeartbeatResponse response)
     {
         var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
         var latencyMs = (int)(now - response.ServerTimestamp);

         ctx.Connection.UpdateLatency(latencyMs);
     }

     // ═══════════════════════════════════════════════════════════════
     // CHARACTER LIST
     // ═══════════════════════════════════════════════════════════════

     /// <summary>
     ///     Lädt die Charakter-Liste für den Account.
     /// </summary>
     private void HandleCharacterListRequest(MessageContext ctx, CharacterListRequest request)
     {
         if (!RequireAuthenticated(ctx)) return;

         _log.Debug("Character list request from {ConnectionId}", ctx.ConnectionId);

         var loadTask = _playerService.GetCharacterListAsync(ctx.Connection.AccountId!.Value);

         ctx.RunAsync(loadTask,
             onCompleted: (ctx, characters) =>
             {
                 _log.Debug("Loaded {Count} characters for {ConnectionId}", characters.Count, ctx.ConnectionId);
                 ctx.Send(new CharacterListResponse(characters));
             },
             onError: (ctx, ex) =>
             {
                 _log.Error(ex, "Error loading character list for {ConnectionId}", ctx.ConnectionId);
                 ctx.SendError("DB_ERROR", "Failed to load character list");
             }
         );
     }

     // ═══════════════════════════════════════════════════════════════
     // CHARACTER SELECT
     // ═══════════════════════════════════════════════════════════════

     /// <summary>
     ///     Wählt einen Charakter aus und betritt das Spiel.
     /// </summary>
     private void HandleCharacterSelect(MessageContext ctx, CharacterSelect request)
     {
         if (!RequireAuthenticated(ctx)) return;

         if (ctx.HasCharacter)
         {
             ctx.SendError("ALREADY_IN_GAME", "You already have a character selected");
             return;
         }

         _log.Debug("Character select from {ConnectionId}:  {CharacterId}", ctx.ConnectionId, request.CharacterId);

         // ─── Charakter laden (async) ───
         var loadTask = _playerService.LoadCharacterAsync(ctx.Connection.AccountId!.Value, request.CharacterId);

         ctx.RunAsync(loadTask,
             onCompleted: (ctx, characterData) => OnCharacterLoaded(ctx, characterData),
             onError: (ctx, ex) =>
             {
                 _log.Error(ex, "Error loading character for {ConnectionId}", ctx.ConnectionId);
                 ctx.Send(new CharacterSelectResponse(false, "Failed to load character"));
             }
         );
     }

     private void OnCharacterLoaded(MessageContext ctx, CharacterData? characterData)
     {
         if (characterData == null)
         {
             ctx.Send(new CharacterSelectResponse(false, "Character not found or does not belong to your account"));
             return;
         }

         // ─── Spieler spawnen (synchron im Game-Loop) ───
         var playerEntity = new PlayerEntity(
             characterData.CharacterId,
             ctx.Connection.AccountId!.Value,
             characterData.Name,
             characterData.Position
         )
         {
             Level = characterData.Level,
             MaxHealth = characterData.MaxHealth,
             CurrentHealth = characterData.CurrentHealth,
             MaxResource = characterData.MaxResource,
             CurrentResource = characterData.CurrentResource,
             Race = characterData.Race,
             Class = characterData.Class,
             Faction = characterData.Faction
         };

         var serverPlayer = new ServerPlayer(playerEntity, ctx.Connection)
         {
             AccountId = ctx.Connection.AccountId,
             AccountFlags = ctx.Connection.AccountFlags
         };

         // Zum ZoneManager hinzufügen
         _zoneManager.AddPlayer(serverPlayer);

         // Connection-State auf InGame setzen
         ctx.Connection.SetInGame();

         _log.Info("Character selected for {ConnectionId}: {Name} (Level {Level}) in Zone {ZoneId}",
             ctx.ConnectionId, characterData.Name, characterData.Level, serverPlayer.RuntimeId.ZoneId);

         // ─── Response senden ───
         ctx.Send(new CharacterSelectResponse(true, null)
         {
             CharacterId = characterData.CharacterId,
             Name = characterData.Name,
             Level = characterData.Level,
             ZoneId = serverPlayer.RuntimeId.ZoneId,
             Position = playerEntity.Position
         });

         // ─── Broadcast an Zone ───
         ctx.BroadcastToZoneExceptSelf(new PlayerJoinedZone(playerEntity));

         // ─── Spieler über andere Spieler in Zone informieren ───
         SendExistingPlayersToNewPlayer(ctx, serverPlayer);
     }

     private void SendExistingPlayersToNewPlayer(MessageContext ctx, ServerPlayer newPlayer)
     {
         var playersInZone = _zoneManager.GetServerPlayersInZone(newPlayer.RuntimeId.ZoneId);

         foreach (var existingPlayer in playersInZone)
         {
             // Nicht sich selbst senden
             if (existingPlayer.Connection.Id == ctx.ConnectionId)
                 continue;

             ctx.Send(new PlayerJoinedZone(existingPlayer.Entity));
         }
     }

     // ═══════════════════════════════════════════════════════════════
     // CHARACTER CREATE
     // ═══════════════════════════════════════════════════════════════

     /// <summary>
     ///     Erstellt einen neuen Charakter.
     /// </summary>
     private void HandleCharacterCreate(MessageContext ctx, CharacterCreate request)
     {
         if (!RequireAuthenticated(ctx)) return;

         _log.Debug("Character create from {ConnectionId}: {Name}", ctx.ConnectionId, request.Name);

         // ─── Input-Validierung ───
         var validationError = ValidateCharacterName(request.Name);
         if (validationError != null)
         {
             ctx.Send(new CharacterCreateResponse(false, null, validationError));
             return;
         }

         // ─── Charakter erstellen (async) ───
         var createTask = _playerService.CreateCharacterAsync(
             ctx.Connection.AccountId!.Value,
             request.Name,
             request.Race,
             request.Class,
             request.Gender
         );

         ctx.RunAsync(createTask,
             onCompleted: (ctx, result) =>
             {
                 if (!result.Success)
                 {
                     ctx.Send(new CharacterCreateResponse(false, null, result.Error ?? "Failed to create character"));
                     return;
                 }

                 _log.Info("Character created for {ConnectionId}: {Name} ({CharacterId})",
                     ctx.ConnectionId, request.Name, result.CharacterId);

                 ctx.Send(new CharacterCreateResponse(true, result.CharacterId, null));
             },
             onError: (ctx, ex) =>
             {
                 _log.Error(ex, "Error creating character for {ConnectionId}", ctx.ConnectionId);
                 ctx.SendError("DB_ERROR", "Failed to create character");
             }
         );
     }

     // ═══════════════════════════════════════════════════════════════
     // CHARACTER DELETE
     // ═══════════════════════════════════════════════════════════════

     /// <summary>
     ///     Löscht einen Charakter.
     /// </summary>
     private void HandleCharacterDelete(MessageContext ctx, CharacterDelete request)
     {
         if (!RequireAuthenticated(ctx)) return;

         if (ctx.HasCharacter && ctx.PlayerId == request.CharacterId)
         {
             ctx.SendError("CANNOT_DELETE", "You cannot delete your currently active character");
             return;
         }

         _log.Debug("Character delete from {ConnectionId}: {CharacterId}", ctx.ConnectionId, request.CharacterId);

         // ─── Charakter löschen (async) ───
         var deleteTask = _playerService.DeleteCharacterAsync(
             ctx.Connection.AccountId!.Value,
             request.CharacterId
         );

         ctx.RunAsync(deleteTask,
             onCompleted: (ctx, success) =>
             {
                 if (!success)
                 {
                     ctx.Send(new CharacterDeleteResponse(false,
                         "Character not found or does not belong to your account"));
                     return;
                 }

                 _log.Info("Character deleted for {ConnectionId}: {CharacterId}",
                     ctx.ConnectionId, request.CharacterId);

                 ctx.Send(new CharacterDeleteResponse(true, null));
             },
             onError: (ctx, ex) =>
             {
                 _log.Error(ex, "Error deleting character for {ConnectionId}", ctx.ConnectionId);
                 ctx.SendError("DB_ERROR", "Failed to delete character");
             }
         );
     }

     // ═══════════════════════════════════════════════════════════════
     // DISCONNECT
     // ═══════════════════════════════════════════════════════════════

     /// <summary>
     ///     Client sendet Disconnect-Nachricht (graceful disconnect).
     /// </summary>
     private void HandleDisconnect(MessageContext ctx, Disconnect request)
     {
         _log.Info("Disconnect from {ConnectionId}: {Reason}", ctx.ConnectionId, request.Reason);

         // Cleanup wie bei Logout
         if (ctx.HasCharacter && ctx.PlayerInfo != null)
         {
             ctx.BroadcastToZoneExceptSelf(new PlayerLeftZone(ctx.PlayerId!.Value));

             var saveTask = _playerService.SaveAndRemovePlayerAsync(ctx.ConnectionId);
             ctx.RunAsync(saveTask,
                 onCompleted: _ => { },
                 onError: (_, ex) => _log.Error(ex, "Failed to save player data for {ConnectionId}", ctx.ConnectionId)
             );
         }

         // Connection trennen
         ctx.Disconnect(request.Message);
     }*/

    private static string? ValidateCharacterName(string? name)
    {
        //Todo: to CharacterCreationService
        if (string.IsNullOrWhiteSpace(name))
            return "Character name cannot be empty";

        if (name.Length < 2)
            return "Character name must be at least 2 characters";

        if (name.Length > 16)
            return "Character name must be at most 16 characters";

        // TODO:  Regex für erlaubte Zeichen
        // if (!Regex. IsMatch(name, "^[a-zA-Z]+$"))
        //     return "Character name can only contain letters";

        // TODO: Banned names check

        return null;
    }
}
