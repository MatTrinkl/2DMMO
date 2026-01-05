namespace Mmo.Shared.Messaging.Enums;

/// <summary>
/// Network message types for the 2DMMO project, organized in 100-blocks for O(1) routing.
/// </summary>
/// <remarks>
/// <para><b>Message Frame Format:</b></para>
/// <code>
/// ┌──────────────┬──────────────┬─────────────────────────────┐
/// │   2 Bytes    │   4 Bytes    │         N Bytes             │
/// │    Type      │   Length     │         Payload             │
/// │  (ushort)    │  (uint32)    │    (MessagePack Data)       │
/// └──────────────┴──────────────┴─────────────────────────────┘
/// </code>
/// <para><b>O(1) Routing:</b></para>
/// <para>Messages are organized into 100-block categories for efficient routing:</para>
/// <list type="bullet">
/// <item><description>Category = MessageType / 100</description></item>
/// <item><description>Each category contains up to 100 message types</description></item>
/// <item><description>Enables O(1) message dispatching without hash lookups</description></item>
/// </list>
/// <para><b>Message Categories:</b></para>
/// <list type="table">
/// <listheader>
/// <term>Range</term>
/// <description>Category</description>
/// </listheader>
/// <item><term>0000-0099</term><description>Connection / Authentication</description></item>
/// <item><term>0100-0199</term><description>Zone Events</description></item>
/// <item><term>0200-0299</term><description>Movement / Position</description></item>
/// <item><term>0300-0399</term><description>Combat</description></item>
/// <item><term>0400-0499</term><description>Chat</description></item>
/// <item><term>0500-0599</term><description>Inventory / Items</description></item>
/// <item><term>0600-0699</term><description>Character / Stats / Progression</description></item>
/// <item><term>0700-0799</term><description>Group / Party</description></item>
/// <item><term>0800-0899</term><description>Guild</description></item>
/// <item><term>0900-0999</term><description>Ping / Latency / System</description></item>
/// <item><term>1000-1099</term><description>Quest</description></item>
/// <item><term>1100-1199</term><description>Trading</description></item>
/// <item><term>1200-1299</term><description>Targeting</description></item>
/// <item><term>1300-1399</term><description>NPC / Dialog / Vendor</description></item>
/// <item><term>1400-1499</term><description>Entity Spawning / Sync</description></item>
/// <item><term>1500-1599</term><description>Buffs / Debuffs / Auras</description></item>
/// <item><term>1600-1699</term><description>Crafting / Professions</description></item>
/// <item><term>1700-1799</term><description>Auction House / Market</description></item>
/// <item><term>1800-1899</term><description>Mail System</description></item>
/// <item><term>1900-1999</term><description>Achievements / Titles</description></item>
/// <item><term>2000-2099</term><description>Mounts / Pets / Companions</description></item>
/// <item><term>2100-2199</term><description>Social (Friends, Block)</description></item>
/// <item><term>2200-2299</term><description>Emotes / Animations / Cosmetics</description></item>
/// <item><term>2300-2399</term><description>Admin / GM Tools</description></item>
/// <item><term>2400-2499</term><description>Instancing / Dungeons / Raids</description></item>
/// <item><term>2500-2599</term><description>PvP / Arena / Battleground</description></item>
/// <item><term>2600-2699</term><description>World State (Weather, Time, Events)</description></item>
/// <item><term>2700-2799</term><description>Matchmaking / Queue</description></item>
/// <item><term>2800-2899</term><description>Leaderboard / Rankings</description></item>
/// <item><term>2900-2999</term><description>Tutorial / Guide System</description></item>
/// <item><term>3000-3099</term><description>Settings / Preferences Sync</description></item>
/// <item><term>3100-3199</term><description>Loot / Rewards</description></item>
/// <item><term>3200-3299</term><description>Cooldowns / Timers</description></item>
/// <item><term>3300-3399</term><description>Inspection / Character Info</description></item>
/// <item><term>3400-3499</term><description>Map / Minimap / Waypoints</description></item>
/// <item><term>3500-3599</term><description>Voice Chat / Audio</description></item>
/// <item><term>3600-3699</term><description>Reporting / Moderation</description></item>
/// <item><term>3700-3799</term><description>Economy / Currency</description></item>
/// <item><term>3800-3899</term><description>Skills / Talents / Abilities</description></item>
/// <item><term>3900-3999</term><description>Equipment / Gear</description></item>
/// <item><term>4000-4099</term><description>Bank / Storage</description></item>
/// <item><term>4100-4199</term><description>Death / Respawn / Ghost</description></item>
/// <item><term>4200-4299</term><description>Transportation</description></item>
/// <item><term>4300-4399</term><description>Notifications / Alerts</description></item>
/// <item><term>4400-4499</term><description>Cutscenes / Cinematics</description></item>
/// <item><term>4500-4599</term><description>Housing / Player Buildings</description></item>
/// <item><term>4600-4699</term><description>Events / Seasonal Content</description></item>
/// <item><term>4900-4999</term><description>Debug / Development</description></item>
/// <item><term>5000-5099</term><description>Server-to-Server (Internal)</description></item>
/// </list>
/// <para><b>Message Direction:</b></para>
/// <para>All messages follow strict directional separation for security:</para>
/// <list type="bullet">
/// <item><description>Client→Server: Requests, input, commands (IClientMessage)</description></item>
/// <item><description>Server→Client: Responses, state updates, broadcasts (IServerMessage)</description></item>
/// <item><description>NO bidirectional messages - each has exactly one direction</description></item>
/// </list>
/// </remarks>
public enum MessageType : ushort
{
    // ═══════════════════════════════════════════════════════════════
    // CONNECTION / AUTHENTICATION (0000-0099)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Initiates the login process with username/password or session token.
    /// Direction: Client→Server. First message after TCP connection establishment.
    /// </summary>
    /// <remarks>
    /// Payload includes: Username, Password (hashed), ClientVersion, HardwareId.
    /// Response: LoginResponse (2) with success status or error code.
    /// Connection timeout: 10 seconds. Rate limited after 3 failed attempts (60s cooldown).
    /// </remarks>
    LoginRequest = 1,
    
    /// <summary>
    /// Response to LoginRequest containing success/failure status and session token.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Includes SessionToken, AccountId, and basic account info.
    /// On failure: Includes ErrorCode (INVALID_CREDENTIALS, ACCOUNT_BANNED, VERSION_MISMATCH, etc.).
    /// </remarks>
    LoginResponse = 2,
    
    /// <summary>
    /// Client requests graceful logout and session termination.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Server will save character state, broadcast PlayerLeftZone, and close connection.
    /// No response message - connection closes after processing.
    /// </remarks>
    LogoutRequest = 3,
    
    /// <summary>
    /// Keepalive message sent periodically to maintain connection.
    /// Direction: Client→Server. Interval: Every 5 seconds.
    /// </summary>
    /// <remarks>
    /// Server expects heartbeat within 15 seconds, otherwise connection times out.
    /// Server may respond with Pong (901) for latency measurement.
    /// </remarks>
    Heartbeat = 4,
    
    /// <summary>
    /// Server forcibly disconnects the client with a reason.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Reasons include: Kicked by admin, banned, duplicate login, anti-cheat violation.
    /// Connection is terminated immediately after sending this message.
    /// CanReconnect flag indicates if reconnection is allowed.
    /// </remarks>
    ForceDisconnect = 5,
    
    /// <summary>
    /// Client attempts to reconnect using a previously issued session token.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Used after unexpected disconnect to restore session state.
    /// Reconnect window: 30 seconds. Max 10 retry attempts with exponential backoff.
    /// Response: ReconnectResponse (7) with success status.
    /// </remarks>
    ReconnectRequest = 6,
    
    /// <summary>
    /// Server response to reconnect attempt.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Restores session, resumes at last known position.
    /// On failure: ErrorCode indicates reason (TOKEN_EXPIRED, SESSION_NOT_FOUND, etc.).
    /// </remarks>
    ReconnectResponse = 7,

    /// <summary>
    /// DEPRECATED: Session validation is now done internally.
    /// </summary>
    /// <remarks>
    /// This message type should no longer be used. Session validation happens
    /// automatically during connection establishment and is not exposed as a separate message.
    /// </remarks>
    [Obsolete("Deprecated: Session validation is now done internally. Remove usage of this message type.")]
    SessionValidate = 8,

    /// <summary>
    /// Client requests to select a character from the character list.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: CharacterId to select.
    /// Response: CharacterSelectResponse (21) with success/failure and spawn data.
    /// Server loads character data and prepares zone entry.
    /// </remarks>
    CharacterSelectRequest = 9,
    
    /// <summary>
    /// Client requests to create a new character.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: CharacterName, Race, Class, Gender, Appearance data.
    /// Response: CharacterCreateResponse (22) with new CharacterId or error.
    /// Subject to name validation, profanity filter, and character slot limits.
    /// </remarks>
    CharacterCreateRequest = 10,
    
    /// <summary>
    /// Client requests to permanently delete a character.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: CharacterId to delete.
    /// Response: CharacterDeleteResponse (23) with confirmation or error.
    /// May require additional confirmation or delay for recovery period.
    /// </remarks>
    CharacterDeleteRequest = 11,
    
    /// <summary>
    /// Client requests the list of characters on this account.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Sent after successful login to display character selection screen.
    /// Response: CharacterListResponse (13) with array of character summaries.
    /// </remarks>
    CharacterListRequest = 12,
    
    /// <summary>
    /// Server sends the list of characters for this account.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: CharacterId, Name, Level, Class, Race, LastPlayed for each character.
    /// Client displays this data in character selection UI.
    /// </remarks>
    CharacterListResponse = 13,
    
    /// <summary>
    /// Client requests to select a specific game server/realm.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: ServerId/RealmId to connect to.
    /// Response: ServerSelectResponse (24) with connection info or queue position.
    /// </remarks>
    ServerSelectRequest = 14,
    
    /// <summary>
    /// Client requests the list of available realms/servers.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Sent after login to display server selection screen.
    /// Response: RealmListResponse (16) with server status, population, and type.
    /// </remarks>
    RealmListRequest = 15,
    
    /// <summary>
    /// Server sends the list of available realms/servers.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: RealmId, Name, Type (PvE/PvP), Status (Online/Offline), Population level.
    /// Updated periodically to reflect current server status.
    /// </remarks>
    RealmListResponse = 16,
    
    /// <summary>
    /// Client requests detailed account data and settings.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Response: AccountDataResponse (18) with account-wide settings and metadata.
    /// Includes: Account tier, subscription status, unlocks, etc.
    /// </remarks>
    AccountDataRequest = 17,
    
    /// <summary>
    /// Server sends detailed account data and settings.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains account-wide information: Premium status, creation date, play time, unlocks.
    /// Used to configure client features and UI based on account entitlements.
    /// </remarks>
    AccountDataResponse = 18,
    
    /// <summary>
    /// Initiates encrypted connection handshake between client and server.
    /// Direction: Bidirectional (Handshake protocol).
    /// </summary>
    /// <remarks>
    /// Establishes encryption keys for secure communication.
    /// Used during initial connection setup before authentication.
    /// Implementation uses standard TLS/SSL protocols.
    /// </remarks>
    EncryptionHandshake = 19,
    
    /// <summary>
    /// Client or server requests to enable/disable message compression.
    /// Direction: Bidirectional.
    /// </summary>
    /// <remarks>
    /// Toggles compression for network traffic to reduce bandwidth.
    /// Typically enabled after login for improved performance.
    /// Uses standard compression algorithms (e.g., gzip, lz4).
    /// </remarks>
    CompressionToggle = 20,
    
    /// <summary>
    /// Server response to CharacterSelectRequest.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Includes character data and initial spawn position/zone.
    /// On failure: ErrorCode (CHARACTER_NOT_FOUND, CHARACTER_IN_USE, etc.).
    /// Triggers zone loading and character spawn on client side.
    /// </remarks>
    CharacterSelectResponse = 21,
    
    /// <summary>
    /// Server response to CharacterCreateRequest.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Includes newly created CharacterId.
    /// On failure: ErrorCode (NAME_TAKEN, INVALID_NAME, SLOT_LIMIT_REACHED, etc.).
    /// </remarks>
    CharacterCreateResponse = 22,
    
    /// <summary>
    /// Server response to CharacterDeleteRequest.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Confirmation that character was deleted.
    /// On failure: ErrorCode (CHARACTER_NOT_FOUND, DELETE_NOT_ALLOWED, etc.).
    /// Character may enter grace period before permanent deletion.
    /// </remarks>
    CharacterDeleteResponse = 23,
    
    /// <summary>
    /// Server response to ServerSelectRequest.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Connection information for selected server.
    /// On queue: Position in queue and estimated wait time.
    /// On failure: ErrorCode (SERVER_FULL, SERVER_OFFLINE, etc.).
    /// </remarks>
    ServerSelectResponse = 24,

    // ═══════════════════════════════════════════════════════════════
    // ZONE EVENTS (0100-0199)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// DEPRECATED: Character spawn data is now included in ZoneState (102).
    /// </summary>
    /// <remarks>
    /// This message type should no longer be used. Character spawn and zone entry
    /// functionality has been consolidated into ZoneState (102) for better atomicity.
    /// </remarks>
    [Obsolete("Deprecated: Character spawn data is now included in ZoneState (102). Remove usage and use ZoneState instead.")]
    JoinZone = 100,

    /// <summary>
    /// Client notifies server of intent to leave current zone.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Server will save zone state, unsubscribe from zone updates, and confirm exit.
    /// Broadcast: CharacterLeftZone (105) sent to other players in the zone.
    /// </remarks>
    LeaveZone = 101,
    
    /// <summary>
    /// Server sends complete zone state to client upon zone entry.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: All entities, terrain data, active events, weather, time of day.
    /// This is the authoritative initial state. Delta updates follow via ZoneDelta (103).
    /// Triggers client zone loading and entity spawning.
    /// </remarks>
    ZoneState = 102,
    
    /// <summary>
    /// Server sends incremental zone state changes.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Delta updates for: Entity spawns/despawns, position changes, state changes.
    /// Sent at regular intervals (per tick) to keep clients synchronized.
    /// Uses dirty tracking to minimize bandwidth.
    /// </remarks>
    ZoneDelta = 103,
    
    /// <summary>
    /// Broadcast notification that a character joined the current zone.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Sent to all players in the zone when a new player enters.
    /// Contains: CharacterId, Name, Position, Appearance data.
    /// Clients spawn the new player entity in their game world.
    /// </remarks>
    CharacterJoinedZone = 104,
    
    /// <summary>
    /// Broadcast notification that a character left the current zone.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Sent to all players in the zone when a player exits.
    /// Contains: CharacterId of departing player.
    /// Clients despawn the player entity from their game world.
    /// </remarks>
    CharacterLeftZone = 105,
    
    /// <summary>
    /// Client requests to transfer to a different zone.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: TargetZoneId, optional EntryPoint/Portal.
    /// Response: ZoneTransferResponse (107) with approval or denial.
    /// Server validates requirements (level, access, etc.) before approval.
    /// </remarks>
    ZoneTransferRequest = 106,
    
    /// <summary>
    /// Server response to zone transfer request.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Includes new ZoneId and spawn coordinates.
    /// On failure: ErrorCode (ZONE_LOCKED, LEVEL_REQUIREMENT, etc.).
    /// Successful transfer triggers LeaveZone + ZoneState sequence.
    /// </remarks>
    ZoneTransferResponse = 107,

    /// <summary>
    /// DEPRECATED: Clients now load assets locally and send ZoneLoadedAck (118) when ready.
    /// </summary>
    /// <remarks>
    /// Previously used for client to report zone loading progress percentage.
    /// No longer needed as asset loading is client-side responsibility.
    /// </remarks>
    [Obsolete("Deprecated: Remove usage - clients load assets locally and send ZoneLoadedAck (118) when ready.")]
    ZoneLoadingProgress = 108,

    /// <summary>
    /// Server notifies client that a new zone has been discovered/unlocked.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Sent when player enters a zone for the first time.
    /// Contains: ZoneId, Name, Description, rewards for discovery.
    /// May trigger achievement or exploration XP gain.
    /// </remarks>
    ZoneDiscovered = 109,
    
    /// <summary>
    /// Client requests list of accessible zones.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Used for map UI and fast travel systems.
    /// Response: ZoneListResponse (111) with available zones and requirements.
    /// </remarks>
    ZoneListRequest = 110,
    
    /// <summary>
    /// Server sends list of zones accessible to the character.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: ZoneId, Name, Level range, Access status (Locked/Unlocked).
    /// Used to populate map and travel UI.
    /// </remarks>
    ZoneListResponse = 111,
    
    /// <summary>
    /// Server initiates transfer to a different shard/instance of current zone.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Used for load balancing or cross-server party/raid grouping.
    /// Transparent to player - maintains position and state.
    /// Connection may briefly reconnect to different zone server.
    /// </remarks>
    ShardTransfer = 112,
    
    /// <summary>
    /// Client requests list of available shards for current zone.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Used when player wants to change shard manually (e.g., to join friends).
    /// Response: ShardListResponse (113) with shard population and IDs.
    /// </remarks>
    ShardListRequest = 113,
    
    /// <summary>
    /// Server sends list of available shards/instances for current zone.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: ShardId, Population, Friends in shard, Recommended status.
    /// Allows player to choose less crowded shard or join friends.
    /// </remarks>
    ShardListResponse = 114,
    
    /// <summary>
    /// Client enters a sub-zone/area within the current zone.
    /// Direction: Client→Server or Server→Client (Notification).
    /// </summary>
    /// <remarks>
    /// Sub-zones are areas within a zone with different properties (music, lighting, etc.).
    /// May trigger area discovery, events, or UI updates.
    /// </remarks>
    SubZoneEnter = 115,
    
    /// <summary>
    /// Client leaves a sub-zone/area within the current zone.
    /// Direction: Client→Server or Server→Client (Notification).
    /// </summary>
    /// <remarks>
    /// Exits sub-zone, reverting to parent zone properties.
    /// May end area-specific effects or events.
    /// </remarks>
    SubZoneLeave = 116,
    
    /// <summary>
    /// Client requests detailed information about a specific zone.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: ZoneId to query.
    /// Returns: Zone metadata (description, level range, type, recommended group size).
    /// Used for UI tooltips and zone information panels.
    /// </remarks>
    GetZoneRequest = 117,
    
    /// <summary>
    /// Client acknowledges that zone assets are loaded and ready.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Sent after client finishes loading zone assets (models, textures, etc.).
    /// Server waits for this before sending ZoneState (102) or allowing gameplay.
    /// Prevents rendering issues and ensures smooth zone transition.
    /// </remarks>
    ZoneLoadedAck = 118,

    // ═══════════════════════════════════════════════════════════════
    // MOVEMENT / POSITION (0200-0299)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Client sends position and movement input to server.
    /// Direction: Client→Server. Frequency: High (every tick with input changes).
    /// </summary>
    /// <remarks>
    /// Contains: Position (X,Y), Velocity, Input state, Sequence number, Timestamp.
    /// Server validates and authorizes movement, broadcasts to nearby players.
    /// Used for client-side prediction with server reconciliation.
    /// </remarks>
    PositionUpdate = 200,
    
    /// <summary>
    /// Server broadcasts authoritative position of an entity to nearby clients.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Contains: EntityId, Position (X,Y), Velocity, Timestamp.
    /// Sent for other players and NPCs. Clients interpolate between updates.
    /// Broadcast range limited to Area of Interest (AOI).
    /// </remarks>
    PositionBroadcast = 201,
    
    /// <summary>
    /// Server corrects client's predicted position (client-side prediction mismatch).
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: Authoritative position, Sequence number of corrected input.
    /// Client snaps or smoothly corrects to server position.
    /// Triggers re-simulation of pending inputs after correction.
    /// Critical for anti-cheat - DO NOT bundle in MessageBundle (950).
    /// </remarks>
    MovementCorrection = 202,
    
    /// <summary>
    /// Client requests to teleport to a specific location.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: Target position or waypoint/portal ID.
    /// Response: TeleportResponse (220) with approval or denial.
    /// Server validates: Distance, cooldown, zone boundaries, access rights.
    /// </remarks>
    TeleportRequest = 203,
    
    /// <summary>
    /// Server executes teleport, moving entity to new position.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: New position, optional zone/shard change.
    /// Client immediately updates position, triggers teleport visual effect.
    /// Broadcast to nearby players as entity despawn/respawn.
    /// </remarks>
    TeleportExecute = 204,
    
    /// <summary>
    /// Server notifies client of movement speed change.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Speed changes from: Buffs/debuffs, items, mounts, terrain effects.
    /// Contains: New speed multiplier, Duration (if temporary), Reason/Source.
    /// Client applies to movement calculations immediately.
    /// </remarks>
    MovementSpeedUpdate = 205,
    
    /// <summary>
    /// Client requests to perform a jump action.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: Current position, jump direction/force.
    /// Response: JumpResponse (221) with approval or denial.
    /// Server validates: Not already jumping, not rooted, stamina/resource cost.
    /// </remarks>
    JumpRequest = 206,
    
    /// <summary>
    /// Server broadcasts jump action to nearby clients.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Contains: EntityId, Jump start position, Velocity vector.
    /// Nearby clients play jump animation and apply physics.
    /// </remarks>
    JumpBroadcast = 207,
    
    /// <summary>
    /// Server notifies client of fall damage taken.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Calculated from fall height and player stats.
    /// Contains: Damage amount, Fall height.
    /// May trigger death if damage exceeds current health.
    /// </remarks>
    FallDamage = 208,
    
    /// <summary>
    /// Client reports being stuck and requests unstuck assistance.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Used when player is trapped in terrain/geometry.
    /// Response: StuckResponse (210) with teleport to safe location.
    /// Rate limited to prevent abuse (e.g., 1 per 5 minutes).
    /// </remarks>
    StuckRequest = 209,
    
    /// <summary>
    /// Server response to stuck request, teleporting player to safety.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Teleports to: Last known safe position, zone entry point, or bind point.
    /// Contains: New position, Cooldown until next stuck request allowed.
    /// </remarks>
    StuckResponse = 210,
    
    /// <summary>
    /// Client requests pathfinding route to target destination.
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: Target position or entity.
    /// Response: PathfindingResponse (212) with waypoint path.
    /// Used for auto-pathing features and NPC navigation display.
    /// </remarks>
    PathfindingRequest = 211,
    
    /// <summary>
    /// Server sends calculated pathfinding route.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: Array of waypoint positions forming path to destination.
    /// Accounts for terrain, obstacles, and navigation mesh.
    /// Client renders path and/or auto-moves along it.
    /// </remarks>
    PathfindingResponse = 212,
    
    /// <summary>
    /// Server forces entity to a specific position (admin/scripted).
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Used for: GM commands, cutscenes, scripted events, anti-cheat corrections.
    /// Contains: Target position, Forced (no client-side smoothing).
    /// Overrides client prediction - immediate snap to position.
    /// </remarks>
    ForcePosition = 213,
    
    /// <summary>
    /// Server notifies client of movement mode change.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Movement modes: Walking, Running, Swimming, Flying, Mounted.
    /// Contains: New mode, Speed multipliers for new mode.
    /// Triggers animation and physics changes on client.
    /// </remarks>
    MovementModeChange = 214,
    
    /// <summary>
    /// Server notifies client of collision with terrain/object.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Used for server-authoritative collision feedback.
    /// Contains: Collision point, Normal vector, Object type.
    /// May trigger sound/visual effects on client.
    /// </remarks>
    CollisionEvent = 215,
    
    /// <summary>
    /// Server applies knockback force to entity.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// From: Combat abilities, explosions, physics events.
    /// Contains: Force vector, Duration, Source entity.
    /// Client applies physics simulation, overrides normal movement temporarily.
    /// </remarks>
    KnockbackEvent = 216,
    
    /// <summary>
    /// Server applies pull force, drawing entity toward a point.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// From: Abilities like "Death Grip", vortex effects, hooks.
    /// Contains: Target position, Pull speed/force, Duration.
    /// Client applies physics simulation toward target.
    /// </remarks>
    PullEvent = 217,
    
    /// <summary>
    /// Server roots entity, preventing movement.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// From: Crowd control abilities (roots, snares, traps).
    /// Contains: Duration, Can cast/attack while rooted.
    /// Client prevents movement input, shows rooted visual effect.
    /// </remarks>
    RootEvent = 218,
    
    /// <summary>
    /// Server stuns entity, preventing movement and actions.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// From: Stun abilities, hard crowd control effects.
    /// Contains: Duration.
    /// Client prevents all input, shows stunned visual effect.
    /// More restrictive than RootEvent - no actions allowed.
    /// </remarks>
    StunMovement = 219,
    
    /// <summary>
    /// Server response to TeleportRequest.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Confirms teleport will execute via TeleportExecute (204).
    /// On failure: ErrorCode (COOLDOWN_ACTIVE, INVALID_DESTINATION, etc.).
    /// </remarks>
    TeleportResponse = 220,
    
    /// <summary>
    /// Server response to JumpRequest.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// On success: Jump is authorized, client can play local prediction.
    /// On failure: ErrorCode (ALREADY_JUMPING, ROOTED, INSUFFICIENT_STAMINA).
    /// </remarks>
    JumpResponse = 221,

    // ═══════════════════════════════════════════════════════════════
    // COMBAT (0300-0399)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Client requests to perform a combat action (ability/skill).
    /// Direction: Client→Server.
    /// </summary>
    /// <remarks>
    /// Payload: ActionId, Target entity, Position (for ground-targeted abilities).
    /// Server validates: Range, line of sight, cooldown, resources, target validity.
    /// Response: ActionResult (301) with success/failure and effects.
    /// </remarks>
    ActionRequest = 300,
    
    /// <summary>
    /// Server sends result of combat action execution.
    /// Direction: Server→Client.
    /// </summary>
    /// <remarks>
    /// Contains: Success/failure, Damage/healing amounts, Hit/miss/crit status.
    /// Broadcast to affected players and spectators.
    /// Triggers combat log entries, damage numbers, animations.
    /// </remarks>
    ActionResult = 301,
    
    /// <summary>
    /// Server notifies clients of damage dealt to an entity.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Contains: Source, Target, Damage amount, Damage type, Critical hit.
    /// Broadcast to nearby players for combat feedback.
    /// Triggers damage numbers, hit effects, health bar updates.
    /// </remarks>
    DamageEvent = 302,
    
    /// <summary>
    /// Server notifies clients that an entity has died.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Contains: EntityId, Killer entity, Death reason.
    /// Triggers: Death animation, loot spawn, respawn timer.
    /// Broadcast to entire zone for players, nearby for NPCs.
    /// </remarks>
    DeathEvent = 303,
    
    /// <summary>
    /// Server notifies clients of healing received by an entity.
    /// Direction: Server→Client (Broadcast).
    /// </summary>
    /// <remarks>
    /// Contains: Source, Target, Healing amount, Overheal, Critical heal.
    /// Broadcast to nearby players for healing feedback.
    /// Triggers healing numbers, heal effects, health bar updates.
    /// </remarks>
    HealEvent = 304,
    
    /// <summary>Attack missed the target. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Attack failed to connect. Triggers "Miss" combat text. No damage dealt.</remarks>
    MissEvent = 305,
    
    /// <summary>Target dodged/evaded the attack. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Target's dodge stat/ability caused attack to miss. Triggers "Dodge" combat text.</remarks>
    DodgeEvent = 306,
    
    /// <summary>Target parried the attack. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Melee attack deflected by target. May create counterattack opportunity.</remarks>
    ParryEvent = 307,
    
    /// <summary>Target blocked the attack with shield. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Damage reduced or negated by block. Contains: Original damage, Blocked amount.</remarks>
    BlockEvent = 308,
    
    /// <summary>Attack scored a critical hit. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Increased damage multiplier applied. Triggers special visual/sound effects.</remarks>
    CriticalHitEvent = 309,
    
    /// <summary>Entity enters combat state. Direction: Server→Client.</summary>
    /// <remarks>Prevents logout, mount, eating. Activates combat UI. Cleared after no combat for duration.</remarks>
    CombatStart = 310,
    
    /// <summary>Entity exits combat state. Direction: Server→Client.</summary>
    /// <remarks>Combat ended - no enemies engaged. Allows out-of-combat actions and regeneration.</remarks>
    CombatEnd = 311,
    
    /// <summary>Server updates threat/aggro level for entity. Direction: Server→Client.</summary>
    /// <remarks>For tank classes and threat management. Contains: Target entity, Threat amount, Percentage of total.</remarks>
    ThreatUpdate = 312,
    
    /// <summary>Client requests threat table for current target. Direction: Client→Server.</summary>
    /// <remarks>Shows who has aggro and threat levels. Response: ThreatListResponse (333).</remarks>
    ThreatListRequest = 313,
    
    /// <summary>Action was interrupted. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Casting/channeling stopped by stun, silence, or interrupt ability. Triggers lockout.</remarks>
    InterruptEvent = 314,
    
    /// <summary>Damage/effect was reflected back to caster. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>From reflect shield/ability. Contains: Original damage, Reflected amount.</remarks>
    ReflectEvent = 315,
    
    /// <summary>Damage was absorbed by shield/barrier. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Shield absorbed all or part of damage. Contains: Original damage, Absorbed amount, Remaining shield.</remarks>
    AbsorbEvent = 316,
    
    /// <summary>Lifesteal effect restored health from damage dealt. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Attacker healed for percentage of damage. Contains: Damage dealt, Health restored.</remarks>
    LifestealEvent = 317,
    
    /// <summary>Boss/NPC enters execute phase (low health). Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Special execute abilities now available. Triggers tactical changes and UI warnings.</remarks>
    ExecutePhase = 318,
    
    /// <summary>Entity enters enraged state. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Increased damage/speed, may trigger special mechanics. From: Low health, time limit, or ability.</remarks>
    EnrageEvent = 319,
    
    /// <summary>Combat log entry for detailed combat tracking. Direction: Server→Client.</summary>
    /// <remarks>Detailed event for combat log UI: All damage, healing, buffs, abilities. Optional - for UI only.</remarks>
    CombatLogEntry = 320,
    
    /// <summary>Threat transferred to another entity. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>From: Taunt, threat drop, feign death. Contains: Old aggro target, New aggro target.</remarks>
    AggroTransfer = 321,
    
    /// <summary>Taunt ability used, forcing target to attack taunter. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Threat manipulation - sets threat to highest + 1. Duration-based forced targeting.</remarks>
    TauntEvent = 322,
    
    /// <summary>Feint/threat reduction ability used. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Reduces user's threat. Used by DPS to avoid pulling aggro.</remarks>
    FeintEvent = 323,
    
    /// <summary>Counterattack triggered after block/parry. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Automatic attack in response to enemy action. Free damage outside normal rotation.</remarks>
    CounterAttack = 324,
    
    /// <summary>Combo chain completed with finisher. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Powerful ability after combo point buildup. Consumes combo resources for enhanced effect.</remarks>
    ComboFinisher = 325,
    
    /// <summary>Area of Effect damage event. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Multiple targets damaged in radius. Contains: Center position, Radius, Damage per target.</remarks>
    AreaDamage = 326,
    
    /// <summary>Damage over time effect tick. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Periodic damage from DoT debuff (bleed, poison, burn). Contains: Tick damage, Remaining duration.</remarks>
    DamageOverTime = 327,
    
    /// <summary>Heal over time effect tick. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Periodic healing from HoT buff (regeneration, lifebloom). Contains: Tick heal, Remaining duration.</remarks>
    HealOverTime = 328,
    
    /// <summary>Damage absorption shield applied to entity. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Creates shield absorbing damage before health. Contains: Shield amount, Duration.</remarks>
    ShieldApplied = 329,
    
    /// <summary>Damage absorption shield depleted/broken. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Shield fully absorbed or expired. Triggers visual effect removal.</remarks>
    ShieldBroken = 330,
    
    /// <summary>Resurrection offer received. Direction: Server→Client.</summary>
    /// <remarks>Another player offers to resurrect. Player must accept/decline. Contains: Resurrecter name, Type (battle rez vs normal).</remarks>
    Resurrection = 331,
    
    /// <summary>Full combat state synchronization. Direction: Server→Client.</summary>
    /// <remarks>Complete combat state update: All buffs, debuffs, resources, cooldowns. Sent on zone in or reconnect.</remarks>
    CombatStateSync = 332,
    
    /// <summary>Server sends threat table data. Direction: Server→Client.</summary>
    /// <remarks>Response to ThreatListRequest (313). Contains: Ordered list of entities with threat values.</remarks>
    ThreatListResponse = 333,
    
    /// <summary>Response to resurrection offer. Direction: Client→Server.</summary>
    /// <remarks>Player accepts or declines resurrection. On accept: Triggers resurrection sequence.</remarks>
    ResurrectionResponse = 334,

    // ═══════════════════════════════════════════════════════════════
    // CHAT (0400-0499)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Client sends chat message. Direction: Client→Server.</summary>
    /// <remarks>Payload: Channel, Message text. Server validates, filters profanity, applies rate limiting. Response: ChatMessageResponse (440).</remarks>
    ChatMessage = 400,
    
    /// <summary>Server broadcasts chat message to recipients. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Delivered to: Channel members, zone, party, guild as appropriate. Contains: Sender, Channel, Message, Timestamp.</remarks>
    ChatBroadcast = 401,
    
    /// <summary>Private message to specific player. Direction: Client→Server.</summary>
    /// <remarks>Payload: Target player name, Message. Server routes to target or returns error if offline/blocked.</remarks>
    ChatWhisper = 402,
    
    /// <summary>Response to whisper. Direction: Server→Client or Client→Server.</summary>
    /// <remarks>Confirms delivery status or contains the whisper content for recipient.</remarks>
    ChatWhisperResponse = 403,
    
    /// <summary>Party chat message. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Broadcast to all party members. Requires active party membership.</remarks>
    ChatParty = 404,
    
    /// <summary>Guild chat message. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Broadcast to all online guild members. Requires guild membership.</remarks>
    ChatGuild = 405,
    
    /// <summary>Raid chat message. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Broadcast to all raid members. Requires active raid membership.</remarks>
    ChatRaid = 406,
    
    /// <summary>Zone-wide chat message. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Visible to all players in current zone. May have level requirements or cooldowns.</remarks>
    ChatZone = 407,
    
    /// <summary>Trade channel message. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>For trading/economy communication. May be zone-scoped or global.</remarks>
    ChatTrade = 408,
    
    /// <summary>Looking for Group channel message. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>For finding party/raid members. Often zone or region scoped.</remarks>
    ChatLfg = 409,
    
    /// <summary>System message from server. Direction: Server→Client.</summary>
    /// <remarks>Server announcements, errors, info messages. Cannot be sent by players. Color-coded UI display.</remarks>
    ChatSystem = 410,
    
    /// <summary>Yell message heard by nearby players. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Larger radius than Say. Broadcast to extended area around player.</remarks>
    ChatYell = 411,
    
    /// <summary>Say message heard by very nearby players. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Small radius local chat. Broadcast to immediate vicinity only.</remarks>
    ChatSay = 412,
    
    /// <summary>Emote/roleplay action. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Emote text displayed as action ("PlayerName dances"). Broadcast to nearby players.</remarks>
    ChatEmote = 413,
    
    /// <summary>Away from keyboard status message. Direction: Client→Server.</summary>
    /// <remarks>Sets AFK flag. Auto-responses to whispers. Appears in player status.</remarks>
    ChatAfk = 414,
    
    /// <summary>Do not disturb status message. Direction: Client→Server.</summary>
    /// <remarks>Blocks whispers and invites. Auto-decline for requests.</remarks>
    ChatDnd = 415,
    
    /// <summary>Join custom chat channel. Direction: Client→Server.</summary>
    /// <remarks>Payload: Channel name, Password (if required). Response: ChatChannelJoinResponse (441).</remarks>
    ChatChannelJoin = 416,
    
    /// <summary>Leave custom chat channel. Direction: Client→Server.</summary>
    /// <remarks>Unsubscribe from channel. No longer receives messages.</remarks>
    ChatChannelLeave = 417,
    
    /// <summary>Request list of available channels. Direction: Client→Server.</summary>
    /// <remarks>Returns: Public channels, joined channels, popular channels with member counts.</remarks>
    ChatChannelList = 418,
    
    /// <summary>Create new custom chat channel. Direction: Client→Server.</summary>
    /// <remarks>Payload: Channel name, Password, Settings. Creator becomes owner. Response: ChatChannelCreateResponse (442).</remarks>
    ChatChannelCreate = 419,
    
    /// <summary>Delete custom chat channel. Direction: Client→Server.</summary>
    /// <remarks>Owner-only action. Removes channel and kicks all members. Response: ChatChannelDeleteResponse (443).</remarks>
    ChatChannelDelete = 420,
    
    /// <summary>Set/change channel password. Direction: Client→Server.</summary>
    /// <remarks>Owner/moderator action. Requires password for future joins. Response: ChatChannelPasswordResponse (444).</remarks>
    ChatChannelPassword = 421,
    
    /// <summary>Mute player in channel. Direction: Client→Server.</summary>
    /// <remarks>Moderator action. Prevents target from sending messages. Response: ChatChannelMuteResponse (445).</remarks>
    ChatChannelMute = 422,
    
    /// <summary>Unmute player in channel. Direction: Client→Server.</summary>
    /// <remarks>Moderator action. Restores messaging privileges.</remarks>
    ChatChannelUnmute = 423,
    
    /// <summary>Kick player from channel. Direction: Client→Server.</summary>
    /// <remarks>Moderator action. Removes player from channel. Player can rejoin unless banned.</remarks>
    ChatChannelKick = 424,
    
    /// <summary>Ban player from channel. Direction: Client→Server.</summary>
    /// <remarks>Moderator action. Permanent removal, prevents rejoining.</remarks>
    ChatChannelBan = 425,
    
    /// <summary>Transfer channel ownership. Direction: Client→Server.</summary>
    /// <remarks>Owner action. Transfers all permissions to new owner.</remarks>
    ChatChannelOwner = 426,
    
    /// <summary>Grant/revoke moderator status. Direction: Client→Server.</summary>
    /// <remarks>Owner action. Gives kick/mute/ban permissions.</remarks>
    ChatChannelModerator = 427,
    
    /// <summary>Message of the Day for channel. Direction: Server→Client.</summary>
    /// <remarks>Displayed on channel join. Set by owner/moderator.</remarks>
    ChatMotd = 428,
    
    /// <summary>Profanity/spam filter settings. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Client can adjust local filter sensitivity. Server enforces minimum standards.</remarks>
    ChatFilter = 429,
    
    /// <summary>Warning for chat spam/abuse. Direction: Server→Client.</summary>
    /// <remarks>Rate limiting warning before temporary mute. Contains: Violation count, Cooldown duration.</remarks>
    ChatSpamWarning = 430,
    
    /// <summary>Response to ChatMessage request. Direction: Server→Client.</summary>
    /// <remarks>Confirms delivery or provides error (RATE_LIMITED, MUTED, INVALID_CHANNEL, etc.).</remarks>
    ChatMessageResponse = 440,
    
    /// <summary>Response to ChatChannelJoin request. Direction: Server→Client.</summary>
    /// <remarks>Success with channel info or error (WRONG_PASSWORD, BANNED, FULL, etc.).</remarks>
    ChatChannelJoinResponse = 441,
    
    /// <summary>Response to ChatChannelCreate request. Direction: Server→Client.</summary>
    /// <remarks>Success with new channel ID or error (NAME_TAKEN, INVALID_NAME, LIMIT_REACHED, etc.).</remarks>
    ChatChannelCreateResponse = 442,
    
    /// <summary>Response to ChatChannelDelete request. Direction: Server→Client.</summary>
    /// <remarks>Confirms deletion or error (NOT_OWNER, CHANNEL_NOT_FOUND, etc.).</remarks>
    ChatChannelDeleteResponse = 443,
    
    /// <summary>Response to ChatChannelPassword request. Direction: Server→Client.</summary>
    /// <remarks>Confirms password change or error (NOT_OWNER, INVALID_PASSWORD, etc.).</remarks>
    ChatChannelPasswordResponse = 444,
    
    /// <summary>Response to ChatChannelMute request. Direction: Server→Client.</summary>
    /// <remarks>Confirms mute or error (NOT_MODERATOR, USER_NOT_FOUND, etc.).</remarks>
    ChatChannelMuteResponse = 445,

    // ═══════════════════════════════════════════════════════════════
    // ═══════════════════════════════════════════════════════════════
    // INVENTORY / ITEMS (0500-0599)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Complete inventory sync. Direction: Server→Client.</summary>
    /// <remarks>Full inventory state on zone in or significant changes.</remarks>
    InventoryUpdate = 500,
    
    /// <summary>Single inventory slot update. Direction: Server→Client.</summary>
    /// <remarks>Item added/removed/changed in specific slot.</remarks>
    InventorySlotUpdate = 501,
    
    /// <summary>Client requests to pick up item. Direction: Client→Server.</summary>
    /// <remarks>Loot or ground item pickup. Response: InventorySlotUpdate or ItemPickupFailed.</remarks>
    ItemPickup = 502,
    
    /// <summary>Item pickup failed. Direction: Server→Client.</summary>
    /// <remarks>Inventory full, too far, already looted, etc.</remarks>
    ItemPickupFailed = 503,
    
    /// <summary>Client requests to drop item. Direction: Client→Server.</summary>
    /// <remarks>Remove from inventory and place in world.</remarks>
    ItemDrop = 504,
    
    /// <summary>Client uses/consumes item. Direction: Client→Server.</summary>
    /// <remarks>Consumables, equipment, quest items. Response: ItemUseResponse.</remarks>
    ItemUse = 505,
    
    /// <summary>Item use result. Direction: Server→Client.</summary>
    /// <remarks>Success with effects or failure with reason.</remarks>
    ItemUseResult = 506,
    
    /// <summary>Destroy/delete item. Direction: Client→Server.</summary>
    /// <remarks>Permanent deletion. Response: ItemDeleteResponse.</remarks>
    ItemDestroy = 507,
    
    /// <summary>Split item stack. Direction: Client→Server.</summary>
    /// <remarks>Divide stack into two. Response: ItemSplitResponse.</remarks>
    ItemSplit = 508,
    
    /// <summary>Merge/stack items. Direction: Client→Server.</summary>
    /// <remarks>Combine compatible stacks.</remarks>
    ItemMerge = 509,
    
    /// <summary>Move item to different slot. Direction: Client→Server.</summary>
    /// <remarks>Within inventory or to bank/equipment. Response: ItemMoveResponse.</remarks>
    ItemMove = 510,
    
    /// <summary>Swap two items. Direction: Client→Server.</summary>
    /// <remarks>Exchange positions of two items.</remarks>
    ItemSwap = 511,
    
    /// <summary>Lock item to prevent sale/deletion. Direction: Client→Server.</summary>
    /// <remarks>Protection against accidental loss. Response: ItemLockResponse.</remarks>
    ItemLock = 512,
    
    /// <summary>Unlock protected item. Direction: Client→Server.</summary>
    /// <remarks>Allow normal item operations again.</remarks>
    ItemUnlock = 513,
    
    /// <summary>Item cooldown started. Direction: Server→Client.</summary>
    /// <remarks>Cannot use item until cooldown expires.</remarks>
    ItemCooldownStart = 514,
    
    /// <summary>Item cooldown completed. Direction: Server→Client.</summary>
    /// <remarks>Item available for use again.</remarks>
    ItemCooldownEnd = 515,
    
    /// <summary>Item durability changed. Direction: Server→Client.</summary>
    /// <remarks>From use/damage. May need repair when 0.</remarks>
    ItemDurabilityChange = 516,
    
    /// <summary>Repair single item. Direction: Client→Server.</summary>
    /// <remarks>Restore durability. Costs gold.</remarks>
    ItemRepair = 517,
    
    /// <summary>Repair all items. Direction: Client→Server.</summary>
    /// <remarks>Convenience for full inventory repair.</remarks>
    ItemRepairAll = 518,
    
    /// <summary>Enchant item. Direction: Client→Server.</summary>
    /// <remarks>Add magical enhancement. Response: ItemEnchantResult.</remarks>
    ItemEnchant = 519,
    
    /// <summary>Enchant result. Direction: Server→Client.</summary>
    /// <remarks>Success/failure with new item state.</remarks>
    ItemEnchantResult = 520,
    
    /// <summary>Socket gem in item. Direction: Client→Server.</summary>
    /// <remarks>Add gem to socket. Response: ItemSocketResult.</remarks>
    ItemSocket = 521,
    
    /// <summary>Socket result. Direction: Server→Client.</summary>
    /// <remarks>Gem successfully socketed or error.</remarks>
    ItemSocketResult = 522,
    
    /// <summary>Upgrade item quality/level. Direction: Client→Server.</summary>
    /// <remarks>Enhancement materials consumed. Response: ItemUpgradeResult.</remarks>
    ItemUpgrade = 523,
    
    /// <summary>Upgrade result. Direction: Server→Client.</summary>
    /// <remarks>Success with new stats or failure.</remarks>
    ItemUpgradeResult = 524,
    
    /// <summary>Transmog item appearance. Direction: Client→Server.</summary>
    /// <remarks>Change visual without affecting stats. Response: ItemTransmogResult.</remarks>
    ItemTransmog = 525,
    
    /// <summary>Transmog result. Direction: Server→Client.</summary>
    /// <remarks>Appearance changed successfully.</remarks>
    ItemTransmogResult = 526,
    
    /// <summary>Salvage item for materials. Direction: Client→Server.</summary>
    /// <remarks>Destroy item, receive components. Response: ItemSalvageResult.</remarks>
    ItemSalvage = 527,
    
    /// <summary>Salvage result. Direction: Server→Client.</summary>
    /// <remarks>Materials received from salvage.</remarks>
    ItemSalvageResult = 528,
    
    /// <summary>Identify unknown item. Direction: Client→Server.</summary>
    /// <remarks>Reveal item properties. Response: ItemIdentifyResult.</remarks>
    ItemIdentify = 529,
    
    /// <summary>Identify result. Direction: Server→Client.</summary>
    /// <remarks>Item properties now known.</remarks>
    ItemIdentifyResult = 530,
    
    /// <summary>Auto-sort bag. Direction: Client→Server.</summary>
    /// <remarks>Organize inventory. Response: ItemSortResponse.</remarks>
    BagSort = 531,
    
    /// <summary>Expand bag capacity. Direction: Client→Server.</summary>
    /// <remarks>Purchase additional slots. Response: BagExpandResponse.</remarks>
    BagExpand = 532,
    
    /// <summary>Request item tooltip data. Direction: Client→Server.</summary>
    /// <remarks>Detailed item info for UI. Response: ItemTooltipResponse.</remarks>
    ItemTooltipRequest = 533,
    
    /// <summary>Item tooltip data. Direction: Server→Client.</summary>
    /// <remarks>Stats, requirements, lore text.</remarks>
    ItemTooltipResponse = 534,
    
    /// <summary>Item link in chat. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Share item reference in chat.</remarks>
    ItemLink = 535,
    
    /// <summary>Move response. Direction: Server→Client.</summary>
    ItemMoveResponse = 536,
    
    /// <summary>Split response. Direction: Server→Client.</summary>
    ItemSplitResponse = 537,
    
    /// <summary>Use response. Direction: Server→Client.</summary>
    ItemUseResponse = 538,
    
    /// <summary>Delete response. Direction: Server→Client.</summary>
    ItemDeleteResponse = 539,
    
    /// <summary>Stack response. Direction: Server→Client.</summary>
    ItemStackResponse = 540,
    
    /// <summary>Sort response. Direction: Server→Client.</summary>
    ItemSortResponse = 541,
    
    /// <summary>Lock response. Direction: Server→Client.</summary>
    ItemLockResponse = 542,
    
    /// <summary>Bag expand response. Direction: Server→Client.</summary>
    BagExpandResponse = 543,

    // ═══════════════════════════════════════════════════════════════
    // CHARACTER / STATS / PROGRESSION (0600-0699)
    // ═══════════════════════════════════════════════════════════════
    LevelUp = 600,
    XpGain = 601,
    StatUpdate = 602,
    StatFullSync = 603,
    ResourceUpdate = 604,
    ResourceRegen = 605,
    CharacterInfo = 606,
    CharacterInfoRequest = 607,
    SkillPointGain = 608,
    TalentPointGain = 609,
    ReputationChange = 610,
    ReputationListRequest = 611,
    ReputationListResponse = 612,
    TitleUnlocked = 613,
    TitleSelect = 614,
    AppearanceChange = 617,
    AppearancePreview = 618,
    RaceChange = 619,
    ClassChange = 620,
    NameChange = 621,
    GenderChange = 622,
    RestXpUpdate = 623,
    RestStateChange = 624,
    AttributeIncreaseResponse = 650,
    CharacterCustomizeResponse = 651,
    TalentLearnResponse = 652,
    TalentResetResponse = 653,
    SpecializationChangeResponse = 654,
    TitleChangeResponse = 655,

    // ═══════════════════════════════════════════════════════════════
    // GROUP / PARTY (0700-0799)
    // ═══════════════════════════════════════════════════════════════
    PartyInvite = 700,
    PartyInviteResponse = 701,
    PartyLeave = 702,
    PartyKick = 703,
    PartyUpdate = 704,
    PartyDisband = 705,
    PartyLeaderChange = 706,
    PartyLootChange = 707,
    PartyReadyCheck = 708,
    PartyReadyResponse = 709,
    PartyMemberUpdate = 710,
    PartyPositionUpdate = 711,
    PartyHealthUpdate = 712,
    PartyResourceUpdate = 713,
    PartyBuffUpdate = 714,
    PartyTargetUpdate = 715,
    PartyRoleSet = 716,
    PartyRoleCheck = 717,
    PartyConvertToRaid = 718,
    PartySync = 719,
    PartySummon = 720,
    PartySummonResponse = 721,
    PartyMarkerSet = 722,
    PartyMarkerClear = 723,
    PartyDifficultyVote = 724,
    PartyDifficultySet = 725,
    PartyAcceptResponse = 740,
    PartyLeaveResponse = 741,
    PartyKickResponse = 742,
    PartyPromoteResponse = 743,
    PartyDisbandResponse = 744,
    PartyLootModeResponse = 745,
    PartyReadyCheckStartResponse = 746,

    // ═══════════════════════════════════════════════════════════════
    // GUILD (0800-0899)
    // ═══════════════════════════════════════════════════════════════
    GuildInvite = 800,
    GuildInviteResponse = 801,
    GuildLeave = 802,
    GuildKick = 803,
    GuildUpdate = 804,
    GuildDisband = 805,
    GuildPromote = 806,
    GuildDemote = 807,
    GuildMotd = 808,
    GuildMotdSet = 809,
    GuildRosterRequest = 810,
    GuildRosterResponse = 811,
    GuildRankCreate = 812,
    GuildRankDelete = 813,
    GuildRankEdit = 814,
    GuildRankReorder = 815,
    GuildPermissionSet = 816,
    GuildInfoEdit = 817,
    GuildTabardChange = 818,
    GuildBankOpen = 819,
    GuildBankDeposit = 820,
    GuildBankWithdraw = 821,
    GuildBankLog = 822,
    GuildBankTabCreate = 823,
    GuildBankTabEdit = 824,
    GuildBankPermission = 825,
    GuildAchievement = 826,
    GuildNews = 827,
    GuildEventCreate = 828,
    GuildEventEdit = 829,
    GuildEventDelete = 830,
    GuildEventSignup = 831,
    GuildSearch = 832,
    GuildApply = 833,
    GuildApplicationList = 834,
    GuildApplicationResponse = 835,
    GuildAllianceInvite = 836,
    GuildAllianceResponse = 837,
    GuildAllianceLeave = 838,
    GuildCreateResponse = 840,
    GuildLeaveResponse = 841,
    GuildKickResponse = 842,
    GuildDisbandResponse = 843,
    GuildPromoteResponse = 844,
    GuildDemoteResponse = 845,
    GuildRankEditResponse = 846,
    GuildMOTDResponse = 847,
    GuildMessageResponse = 848,
    GuildBankDepositResponse = 849,
    GuildBankWithdrawResponse = 850,

    // ═══════════════════════════════════════════════════════════════
    // ═══════════════════════════════════════════════════════════════
    // PING / LATENCY / SYSTEM (0900-0999)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Client latency measurement request. Direction: Client→Server.</summary>
    /// <remarks>Contains: Timestamp, Sequence number. Server responds with Pong (901). Used for RTT calculation and connection monitoring.</remarks>
    Ping = 900,
    
    /// <summary>Server response to Ping. Direction: Server→Client.</summary>
    /// <remarks>Echoes: Client timestamp, Sequence number. Adds: Server time. Client calculates RTT. DO NOT bundle in MessageBundle (950).</remarks>
    Pong = 901,
    
    /// <summary>Server sends latency statistics report. Direction: Server→Client.</summary>
    /// <remarks>Periodic report (every 30s): Average latency, Min, Max, Packet loss, Jitter. Used for connection quality monitoring.</remarks>
    LatencyReport = 902,
    
    /// <summary>Network statistics update. Direction: Server→Client.</summary>
    /// <remarks>Contains: Bytes sent/received, Messages sent/received, Bandwidth usage. For debugging and monitoring.</remarks>
    NetworkStats = 903,
    
    /// <summary>Connection quality assessment. Direction: Server→Client.</summary>
    /// <remarks>Periodic (every 10s): Quality level (Excellent/Good/Fair/Poor/Bad), Latency, Packet loss. Triggers UI indicator.</remarks>
    ConnectionQuality = 904,
    
    /// <summary>Server error message notification. Direction: Server→Client.</summary>
    /// <remarks>Generic error messages with severity (Info/Warning/Error/Fatal). Contains: Error code, Message, Details. May disconnect on Fatal.</remarks>
    ErrorMessage = 910,
    
    /// <summary>Server-wide announcement. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Admin announcements, events, news. Contains: Type (Info/Warning/Event), Message, Priority. Displayed prominently.</remarks>
    ServerAnnouncement = 911,
    
    /// <summary>Player kick notification. Direction: Server→Client.</summary>
    /// <remarks>Admin kicked player. Contains: Reason, Admin name, Duration. Connection terminates after displaying message.</remarks>
    KickNotification = 912,
    
    /// <summary>Scheduled maintenance warning. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Countdown to shutdown: 60min, 30min, 10min, 1min intervals. Contains: Minutes remaining, Reason, Expected restart time.</remarks>
    MaintenanceWarning = 913,
    
    /// <summary>Server shutting down. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Final warning before shutdown. Contains: Reason, Expected restart time. All connections close shortly after.</remarks>
    ServerShutdown = 914,
    
    /// <summary>Client version incompatible with server. Direction: Server→Client.</summary>
    /// <remarks>Client version too old/new. Contains: Required version, Download URL. Connection rejected.</remarks>
    VersionMismatch = 915,
    
    /// <summary>Account banned notification. Direction: Server→Client.</summary>
    /// <remarks>Account ban details. Contains: Reason, Duration/Permanent, Appeal information. Connection terminates.</remarks>
    BanNotification = 916,
    
    /// <summary>Rate limit warning. Direction: Server→Client.</summary>
    /// <remarks>Too many requests/actions. Contains: Action type, Current rate, Limit, Cooldown. Temporary throttling applied.</remarks>
    RateLimitWarning = 917,
    
    /// <summary>Server status information. Direction: Server→Client.</summary>
    /// <remarks>Server health info: Population, Uptime, Load. Used for server selection and monitoring.</remarks>
    ServerStatus = 918,
    
    /// <summary>Server Message of the Day. Direction: Server→Client.</summary>
    /// <remarks>MOTD displayed on login. Contains: Message, Last updated timestamp. Can contain news/events/patch notes.</remarks>
    ServerMotd = 919,
    
    /// <summary>Server authoritative time sync. Direction: Server→Client.</summary>
    /// <remarks>Server timestamp for time sync. Client adjusts clock for server time display. Periodic (every few minutes).</remarks>
    ServerTime = 920,
    
    /// <summary>Server configuration update. Direction: Server→Client.</summary>
    /// <remarks>Dynamic config changes: Feature flags, rates, limits. Client updates behavior without restart.</remarks>
    ServerConfig = 921,
    
    /// <summary>Client-specific configuration. Direction: Server→Client.</summary>
    /// <remarks>Personalized settings: UI options, permissions, unlocks. Based on account tier/privileges.</remarks>
    ClientConfig = 922,
    
    /// <summary>Feature toggle enable/disable. Direction: Server→Client.</summary>
    /// <remarks>Enable/disable features dynamically: Beta features, seasonal content, A/B testing. Updates client UI/functionality.</remarks>
    FeatureToggle = 923,
    
    /// <summary>Anti-cheat violation warning. Direction: Server→Client.</summary>
    /// <remarks>Suspicious behavior detected. Contains: Warning type (speed_anomaly, teleport, etc.), Violation count. Escalates to kick.</remarks>
    AntiCheatWarning = 924,
    
    /// <summary>Anti-cheat automatic kick. Direction: Server→Client.</summary>
    /// <remarks>Confirmed cheat detection. Contains: Detection type, Evidence, Can reconnect flag. Connection terminates.</remarks>
    AntiCheatKick = 925,

    /// <summary>
    /// Message bundle for batching multiple outgoing messages per tick per client.
    /// Reduces TCP overhead by combining multiple messages into a single frame.
    /// Used for high-frequency updates like entity positions, state changes, etc.
    /// DO NOT bundle: ForceDisconnect (5), MovementCorrection (202), Pong (901).
    /// </summary>
    MessageBundle = 950,

    // ═══════════════════════════════════════════════════════════════
    // QUEST (1000-1099)
    // ═══════════════════════════════════════════════════════════════
    QuestAccept = 1000,
    QuestAcceptResult = 1001,
    QuestAbandon = 1002,
    QuestProgress = 1003,
    QuestComplete = 1004,
    QuestCompleteResult = 1005,
    QuestRewardChoose = 1006,
    QuestRewardReceive = 1007,
    QuestListRequest = 1008,
    QuestListResponse = 1009,
    QuestLogUpdate = 1010,
    QuestShare = 1011,
    QuestShareResponse = 1012,
    QuestTrack = 1013,
    QuestUntrack = 1014,
    QuestObjectiveUpdate = 1015,
    QuestPoiRequest = 1016,
    QuestPoiResponse = 1017,
    QuestGiverStatus = 1018,
    QuestGiverList = 1019,
    DailyQuestReset = 1020,
    WeeklyQuestReset = 1021,
    QuestChainUpdate = 1022,
    QuestRepeatableReset = 1023,

    // ═══════════════════════════════════════════════════════════════
    // TRADING (1100-1199)
    // ═══════════════════════════════════════════════════════════════
    TradeRequest = 1100,
    TradeRequestResponse = 1101,
    TradeUpdate = 1102,
    TradeSetItem = 1103,
    TradeRemoveItem = 1104,
    TradeSetGold = 1105,
    TradeConfirm = 1106,
    TradeUnconfirm = 1107,
    TradeLock = 1108,
    TradeCancel = 1109,
    TradeComplete = 1110,
    TradeError = 1111,
    TradeBusy = 1112,
    TradeTargetBusy = 1113,

    // ═══════════════════════════════════════════════════════════════
    // TARGETING (1200-1299)
    // ═══════════════════════════════════════════════════════════════
    TargetSelect = 1200,
    TargetClear = 1201,
    TargetUpdate = 1202,
    TargetInfoRequest = 1203,
    TargetInfoResponse = 1204,
    TargetOfTarget = 1205,
    TargetOfTargetUpdate = 1206,
    FocusTarget = 1207,
    FocusClear = 1208,
    AssistTarget = 1209,
    MarkTarget = 1210,
    MarkClear = 1211,
    MarkClearAll = 1212,
    MouseoverTarget = 1213,
    TabTarget = 1214,
    NearestEnemyTarget = 1215,
    NearestFriendTarget = 1216,
    TargetSelectResponse = 1220,
    AssistTargetResponse = 1221,
    MarkTargetResponse = 1222,
    TabTargetResponse = 1223,
    NearestEnemyTargetResponse = 1224,
    NearestFriendTargetResponse = 1225,

    // ═══════════════════════════════════════════════════════════════
    // NPC / DIALOG / VENDOR (1300-1399)
    // ═══════════════════════════════════════════════════════════════
    NpcInteract = 1300,
    NpcInteractResult = 1301,
    NpcDialogOpen = 1302,
    NpcDialogChoice = 1303,
    NpcDialogClose = 1304,
    NpcGossipRequest = 1305,
    NpcGossipResponse = 1306,
    VendorOpen = 1310,
    VendorClose = 1311,
    VendorListRequest = 1312,
    VendorListResponse = 1313,
    VendorBuy = 1314,
    VendorBuyResult = 1315,
    VendorSell = 1316,
    VendorSellResult = 1317,
    VendorBuyback = 1318,
    VendorBuybackResult = 1319,
    VendorRepair = 1320,
    VendorRepairAll = 1321,
    VendorRepairResult = 1322,
    TrainerOpen = 1330,
    TrainerClose = 1331,
    TrainerListRequest = 1332,
    TrainerListResponse = 1333,
    TrainerLearn = 1334,
    TrainerLearnResult = 1335,
    InnkeeperBind = 1340,
    InnkeeperBindResult = 1341,
    FlightmasterOpen = 1342,
    FlightmasterList = 1343,
    BankerOpen = 1344,
    AuctioneerOpen = 1345,
    MailboxOpen = 1346,
    StablemasterOpen = 1347,
    BarberOpen = 1348,
    TransmogOpen = 1349,

    // ═══════════════════════════════════════════════════════════════
    // ENTITY SPAWNING / SYNC (1400-1499)
    // ═══════════════════════════════════════════════════════════════
    EntitySpawn = 1400,
    EntitySpawnBatch = 1401,
    EntityDespawn = 1402,
    EntityDespawnBatch = 1403,
    EntityUpdate = 1404,
    EntityUpdateBatch = 1405,
    EntityListRequest = 1406,
    EntityListResponse = 1407,
    EntityPathUpdate = 1408,
    EntityStateChange = 1409,
    EntityAnimation = 1410,
    EntityAnimationBatch = 1411,
    EntityNameplate = 1412,
    EntityNameplateUpdate = 1413,
    EntityFaction = 1414,
    EntityScale = 1415,
    EntityMountUpdate = 1416,
    EntityEquipmentUpdate = 1417,
    EntityAuraUpdate = 1418,
    EntityEmote = 1419,
    EntitySay = 1420,
    EntityYell = 1421,
    LootableSpawn = 1430,
    LootableDespawn = 1431,
    ResourceNodeSpawn = 1432,
    ResourceNodeDespawn = 1433,
    ResourceNodeState = 1434,

    // ═══════════════════════════════════════════════════════════════
    // BUFFS / DEBUFFS / AURAS (1500-1599)
    // ═══════════════════════════════════════════════════════════════
    BuffApplied = 1500,
    BuffRemoved = 1501,
    BuffRefreshed = 1502,
    BuffStackUpdate = 1503,
    DebuffApplied = 1504,
    DebuffRemoved = 1505,
    AuraListSync = 1506,
    AuraUpdate = 1507,
    DispelRequest = 1508,
    DispelResult = 1509,
    StealRequest = 1510,
    StealResult = 1511,
    PurgeRequest = 1512,
    PurgeResult = 1513,
    AuraImmune = 1514,
    AuraResist = 1515,
    BuffCategoryUpdate = 1516,

    // ═══════════════════════════════════════════════════════════════
    // CRAFTING / PROFESSIONS (1600-1699)
    // ═══════════════════════════════════════════════════════════════
    CraftingOpen = 1600,
    CraftingClose = 1601,
    CraftingRecipeList = 1602,
    CraftingStart = 1603,
    CraftingProgress = 1604,
    CraftingComplete = 1605,
    CraftingFailed = 1606,
    CraftingCancel = 1607,
    CraftingQueue = 1608,
    CraftingQueueAdd = 1609,
    CraftingQueueRemove = 1610,
    RecipeLearn = 1611,
    RecipeUnlearn = 1612,
    RecipeDiscovery = 1613,
    ProfessionInfo = 1620,
    ProfessionLevelUp = 1621,
    ProfessionSkillUp = 1622,
    GatheringStart = 1630,
    GatheringProgress = 1631,
    GatheringComplete = 1632,
    GatheringFailed = 1633,
    GatheringInterrupt = 1634,

    // ═══════════════════════════════════════════════════════════════
    // AUCTION HOUSE / MARKET (1700-1799)
    // ═══════════════════════════════════════════════════════════════
    AuctionOpen = 1700,
    AuctionClose = 1701,
    AuctionSearch = 1702,
    AuctionSearchResults = 1703,
    AuctionCreate = 1704,
    AuctionCreateResult = 1705,
    AuctionBid = 1706,
    AuctionBidResult = 1707,
    AuctionBuyout = 1708,
    AuctionBuyoutResult = 1709,
    AuctionCancel = 1710,
    AuctionCancelResult = 1711,
    AuctionExpired = 1712,
    AuctionSold = 1713,
    AuctionOutbid = 1714,
    AuctionWon = 1715,
    AuctionListOwned = 1716,
    AuctionListBids = 1717,
    AuctionPriceHistory = 1718,
    AuctionFavorite = 1719,
    AuctionFavoriteList = 1720,

    // ═══════════════════════════════════════════════════════════════
    // MAIL SYSTEM (1800-1899)
    // ═══════════════════════════════════════════════════════════════
    MailInboxRequest = 1800,
    MailInboxResponse = 1801,
    MailSend = 1802,
    MailSendResult = 1803,
    MailRead = 1804,
    MailMarkRead = 1805,
    MailTakeAttachment = 1806,
    MailTakeAttachmentResult = 1807,
    MailTakeGold = 1808,
    MailTakeGoldResult = 1809,
    MailTakeAll = 1810,
    MailDelete = 1811,
    MailReturn = 1812,
    MailNotification = 1813,
    MailCashOnDelivery = 1814,
    MailCashOnDeliveryPay = 1815,
    MailExpired = 1816,

    // ═══════════════════════════════════════════════════════════════
    // ACHIEVEMENTS / TITLES (1900-1999)
    // ═══════════════════════════════════════════════════════════════
    AchievementUnlocked = 1900,
    AchievementProgress = 1901,
    AchievementListRequest = 1902,
    AchievementListResponse = 1903,
    AchievementCriteriaUpdate = 1904,
    AchievementPointsUpdate = 1905,
    AchievementToast = 1906,
    AchievementLink = 1907,
    AchievementCompare = 1908,
    AchievementCompareResult = 1909,
    TitleUnlock = 1920,
    TitleSelectMsg = 1921,
    TitleClear = 1922,
    TitleListRequest = 1923,
    TitleListResponse = 1924,

    // ═══════════════════════════════════════════════════════════════
    // MOUNTS / PETS / COMPANIONS (2000-2099)
    // ═══════════════════════════════════════════════════════════════
    MountSummon = 2000,
    MountSummonResult = 2001,
    MountDismount = 2002,
    MountListRequest = 2003,
    MountListResponse = 2004,
    MountFavorite = 2005,
    MountUnfavorite = 2006,
    MountRandomFavorite = 2007,
    PetSummon = 2020,
    PetSummonResult = 2021,
    PetDismiss = 2022,
    PetRename = 2023,
    PetCommand = 2024,
    PetCommandResult = 2025,
    PetUpdate = 2026,
    PetFeed = 2027,
    PetTrain = 2028,
    PetAbandon = 2029,
    PetStable = 2030,
    PetUnstable = 2031,
    PetListRequest = 2032,
    PetListResponse = 2033,
    CompanionSummon = 2050,
    CompanionDismiss = 2051,
    CompanionInteract = 2052,
    CompanionListRequest = 2053,
    CompanionListResponse = 2054,

    // ═══════════════════════════════════════════════════════════════
    // SOCIAL (FRIENDS, BLOCK) (2100-2199)
    // ═══════════════════════════════════════════════════════════════
    FriendRequest = 2100,
    FriendRequestResult = 2101,
    FriendAccept = 2102,
    FriendDecline = 2103,
    FriendRemove = 2104,
    FriendListRequest = 2105,
    FriendListResponse = 2106,
    FriendOnline = 2107,
    FriendOffline = 2108,
    FriendUpdate = 2109,
    FriendNote = 2110,
    BlockPlayer = 2120,
    BlockPlayerResult = 2121,
    UnblockPlayer = 2122,
    BlockListRequest = 2123,
    BlockListResponse = 2124,
    IgnorePlayer = 2125,
    UnignorePlayer = 2126,
    WhoRequest = 2130,
    WhoResponse = 2131,
    PlayerLocation = 2132,

    // ═══════════════════════════════════════════════════════════════
    // EMOTES / ANIMATIONS / COSMETICS (2200-2299)
    // ═══════════════════════════════════════════════════════════════
    EmoteRequest = 2200,
    EmoteBroadcast = 2201,
    EmoteTargeted = 2202,
    AnimationTrigger = 2203,
    AnimationCancel = 2204,
    DanceStart = 2210,
    DanceStop = 2211,
    SitRequest = 2212,
    StandRequest = 2213,
    SleepRequest = 2214,
    KneelRequest = 2215,
    CosmeticEquip = 2230,
    CosmeticUnequip = 2231,
    CosmeticPreview = 2232,
    TransmogApply = 2233,
    TransmogRemove = 2234,
    TransmogSave = 2235,
    TransmogLoad = 2236,
    ToyUse = 2240,
    ToyListRequest = 2241,
    ToyListResponse = 2242,

    // ═══════════════════════════════════════════════════════════════
    // ADMIN / GM TOOLS (2300-2399)
    // ═══════════════════════════════════════════════════════════════
    AdminCommand = 2300,
    AdminCommandResult = 2301,
    AdminTeleport = 2302,
    AdminTeleportPlayer = 2303,
    AdminKick = 2304,
    AdminBan = 2305,
    AdminUnban = 2306,
    AdminMute = 2307,
    AdminUnmute = 2308,
    AdminSpawn = 2309,
    AdminDespawn = 2310,
    AdminKill = 2311,
    AdminRevive = 2312,
    AdminHeal = 2313,
    AdminGodMode = 2314,
    AdminInvisible = 2315,
    AdminFreeze = 2316,
    AdminUnfreeze = 2317,
    AdminGiveItem = 2318,
    AdminRemoveItem = 2319,
    AdminSetLevel = 2320,
    AdminSetStat = 2321,
    AdminSetReputation = 2322,
    AdminAddGold = 2323,
    AdminRemoveGold = 2324,
    AdminAnnounce = 2325,
    AdminWhisper = 2326,
    AdminSummonPlayer = 2327,
    AdminAppearPlayer = 2328,
    AdminPlayerInfo = 2329,
    AdminServerInfo = 2330,
    AdminReloadConfig = 2331,
    AdminReloadScripts = 2332,
    AdminShutdown = 2333,
    AdminRestart = 2334,
    AdminMaintenance = 2335,
    AdminLog = 2336,

    // ═══════════════════════════════════════════════════════════════
    // INSTANCING / DUNGEONS / RAIDS (2400-2499)
    // ═══════════════════════════════════════════════════════════════
    InstanceCreate = 2400,
    InstanceCreateResult = 2401,
    InstanceJoin = 2402,
    InstanceJoinResult = 2403,
    InstanceLeave = 2404,
    InstanceReset = 2405,
    InstanceResetResult = 2406,
    InstanceLockout = 2407,
    InstanceLockoutList = 2408,
    InstanceDifficultySet = 2409,
    InstanceDifficultyVote = 2410,
    InstanceSaved = 2411,
    InstanceExtend = 2412,
    InstanceEncounterStart = 2420,
    InstanceEncounterEnd = 2421,
    InstanceEncounterUpdate = 2422,
    InstanceBossKill = 2423,
    InstanceWipe = 2424,
    InstanceCheckpoint = 2425,
    RaidConvert = 2430,
    RaidDisband = 2431,
    RaidGroupSet = 2432,
    RaidTargetSet = 2433,
    RaidReadyCheck = 2434,
    RaidReadyResponse = 2435,
    DungeonFinderJoin = 2440,
    DungeonFinderLeave = 2441,
    DungeonFinderUpdate = 2442,
    DungeonFinderProposal = 2443,
    DungeonFinderAccept = 2444,
    DungeonFinderDecline = 2445,
    RaidFinderJoin = 2450,
    RaidFinderLeave = 2451,
    RaidFinderUpdate = 2452,

    // ═══════════════════════════════════════════════════════════════
    // PVP / ARENA / BATTLEGROUND (2500-2599)
    // ═══════════════════════════════════════════════════════════════
    PvpFlagRequest = 2500,
    PvpFlagUpdate = 2501,
    PvpFlagExpiring = 2502,
    PvpKill = 2503,
    PvpDeath = 2504,
    PvpHonorGain = 2505,
    PvpHonorUpdate = 2506,
    PvpRankUpdate = 2507,
    ArenaTeamCreate = 2520,
    ArenaTeamDisband = 2521,
    ArenaTeamInvite = 2522,
    ArenaTeamLeave = 2523,
    ArenaTeamKick = 2524,
    ArenaTeamUpdate = 2525,
    ArenaJoinQueue = 2530,
    ArenaLeaveQueue = 2531,
    ArenaQueueUpdate = 2532,
    ArenaMatchFound = 2533,
    ArenaMatchStart = 2534,
    ArenaMatchEnd = 2535,
    ArenaMatchResult = 2536,
    ArenaRatingUpdate = 2537,
    BattlegroundJoinQueue = 2550,
    BattlegroundLeaveQueue = 2551,
    BattlegroundQueueUpdate = 2552,
    BattlegroundJoin = 2553,
    BattlegroundLeave = 2554,
    BattlegroundStart = 2555,
    BattlegroundEnd = 2556,
    BattlegroundScore = 2557,
    BattlegroundScoreUpdate = 2558,
    BattlegroundObjective = 2559,
    BattlegroundFlag = 2560,
    WorldPvpObjective = 2570,
    WorldPvpZoneUpdate = 2571,

    // ═══════════════════════════════════════════════════════════════
    // WORLD STATE (WEATHER, TIME, EVENTS) (2600-2699)
    // ═══════════════════════════════════════════════════════════════
    WeatherUpdate = 2600,
    WeatherForecast = 2601,
    TimeOfDayUpdate = 2602,
    TimeOfDaySync = 2603,
    DayNightCycle = 2604,
    MoonPhase = 2605,
    SeasonChange = 2606,
    WorldEventStart = 2620,
    WorldEventEnd = 2621,
    WorldEventProgress = 2622,
    WorldEventObjective = 2623,
    WorldBossSpawn = 2624,
    WorldBossKill = 2625,
    WorldBossAnnounce = 2626,
    HolidayStart = 2630,
    HolidayEnd = 2631,
    HolidayInfo = 2632,
    ServerFirstAnnounce = 2640,
    ServerFirstList = 2641,
    ZoneControlUpdate = 2650,
    TerritoryCapture = 2651,

    // ═══════════════════════════════════════════════════════════════
    // MATCHMAKING / QUEUE (2700-2799)
    // ═══════════════════════════════════════════════════════════════
    QueueJoin = 2700,
    QueueJoinResult = 2701,
    QueueLeave = 2702,
    QueueUpdate = 2703,
    QueueEstimate = 2704,
    QueuePop = 2705,
    QueueAccept = 2706,
    QueueDecline = 2707,
    QueueTimeout = 2708,
    QueueKick = 2709,
    QueueDeserter = 2710,
    RoleSelect = 2720,
    RoleConfirm = 2721,
    RoleShortage = 2722,
    SkirmishJoin = 2730,
    SkirmishLeave = 2731,
    SkirmishUpdate = 2732,

    // ═══════════════════════════════════════════════════════════════
    // LEADERBOARD / RANKINGS (2800-2899)
    // ═══════════════════════════════════════════════════════════════
    LeaderboardRequest = 2800,
    LeaderboardResponse = 2801,
    LeaderboardUpdate = 2802,
    RankingPersonal = 2803,
    RankingGuild = 2804,
    PvpRatingRequest = 2810,
    PvpRatingResponse = 2811,
    PvpSeasonInfo = 2812,
    PvpSeasonEnd = 2813,
    PvpSeasonReward = 2814,
    MythicRankingRequest = 2820,
    MythicRankingResponse = 2821,
    RaidProgressRankingRequest = 2822,
    RaidProgressRankingResponse = 2823,
    AchievementRankingRequest = 2824,
    AchievementRankingResponse = 2825,

    // ═══════════════════════════════════════════════════════════════
    // TUTORIAL / GUIDE SYSTEM (2900-2999)
    // ═══════════════════════════════════════════════════════════════
    TutorialStart = 2900,
    TutorialStep = 2901,
    TutorialComplete = 2902,
    TutorialSkipRequest = 2903,
    TutorialSkipResponse = 2904,
    TutorialResetRequest = 2905,
    TutorialResetResponse = 2906,
    TutorialStepAck = 2907,
    TutorialStateSyncRequest = 2908,
    TutorialStateSyncResponse = 2909,
    HintShow = 2910,
    HintDismissRequest = 2911,
    HintDismissResponse = 2912,
    HintDisableRequest = 2913,
    HintDisableResponse = 2914,
    TipOfTheDay = 2920,
    NewFeatureHighlight = 2921,
    GuideOpenRequest = 2930,
    GuideOpenResponse = 2931,
    GuideCloseRequest = 2932,
    GuideProgress = 2933,

    // ═══════════════════════════════════════════════════════════════
    // SETTINGS / PREFERENCES SYNC (3000-3099)
    // ═══════════════════════════════════════════════════════════════
    SettingsLoad = 3000,
    SettingsLoadResult = 3001,
    SettingsSave = 3002,
    SettingsSaveResult = 3003,
    SettingsReset = 3004,
    SettingsResetResult = 3005,
    KeybindingsLoad = 3010,
    KeybindingsLoadResult = 3011,
    KeybindingsSave = 3012,
    KeybindingsSaveResult = 3013,
    KeybindingsReset = 3014,
    KeybindingsResetResult = 3015,
    UiLayoutLoad = 3020,
    UiLayoutLoadResult = 3021,
    UiLayoutSave = 3022,
    UiLayoutSaveResult = 3023,
    UiLayoutReset = 3024,
    UiLayoutResetResult = 3025,
    MacroCreate = 3030,
    MacroCreateResult = 3031,
    MacroEdit = 3032,
    MacroEditResult = 3033,
    MacroDelete = 3034,
    MacroDeleteResult = 3035,
    MacroSync = 3036,
    AddonDataLoad = 3040,
    AddonDataLoadResult = 3041,
    AddonDataSave = 3042,
    AddonDataSaveResult = 3043,

    // ═══════════════════════════════════════════════════════════════
    // LOOT / REWARDS (3100-3199)
    // ═══════════════════════════════════════════════════════════════
    LootWindowOpen = 3100,
    LootWindowClose = 3101,
    LootItem = 3102,
    LootItemResult = 3103,
    LootGold = 3104,
    LootAll = 3105,
    LootWindowCloseResponse = 3106,
    LootGoldResult = 3107,
    LootAllResult = 3108,
    LootRollStart = 3110,
    LootRollNeed = 3111,
    LootRollGreed = 3112,
    LootRollPass = 3113,
    LootRollResult = 3114,
    LootRollWinner = 3115,
    LootRollVoteResponse = 3116,
    LootMasterAssign = 3120,
    LootRulesChange = 3121,
    LootThresholdChange = 3122,
    LootMasterAssignResult = 3123,
    LootRulesChangeResult = 3124,
    LootThresholdChangeResult = 3125,
    PersonalLoot = 3130,
    BonusRollPrompt = 3131,
    BonusRollUse = 3132,
    BonusRollResult = 3133,
    RewardChoicePrompt = 3140,
    RewardChoiceSelect = 3141,
    RewardChoiceResult = 3142,

    // ═══════════════════════════════════════════════════════════════
    // COOLDOWNS / TIMERS (3200-3299)
    // ═══════════════════════════════════════════════════════════════
    CooldownStart = 3200,
    CooldownEnd = 3201,
    CooldownUpdate = 3202,
    CooldownReset = 3203,
    CooldownSync = 3204,
    GlobalCooldownStart = 3210,
    GlobalCooldownEnd = 3211,
    CastStart = 3220,
    CastUpdate = 3221,
    CastInterrupt = 3222,
    CastComplete = 3223,
    CastFailed = 3224,
    ChannelStart = 3230,
    ChannelTick = 3231,
    ChannelInterrupt = 3232,
    ChannelComplete = 3233,
    ChargeUpdate = 3240,
    ChargeRestore = 3241,

    // ═══════════════════════════════════════════════════════════════
    // INSPECTION / CHARACTER INFO (3300-3399)
    // ═══════════════════════════════════════════════════════════════
    InspectRequest = 3300,
    InspectResponse = 3301,
    InspectEquipment = 3302,
    InspectTalents = 3303,
    InspectAchievements = 3304,
    InspectPvp = 3305,
    InspectGuild = 3306,
    ArmoryRequest = 3310,
    ArmoryResponse = 3311,
    GearScoreCalculate = 3320,
    GearScoreCalculateResponse = 3321,
    GearScoreUpdate = 3322,
    ItemLevelUpdate = 3323,
    StatisticsRequest = 3330,
    StatisticsResponse = 3331,
    StatisticsUpdate = 3332,
    PlayedTimeRequest = 3340,
    PlayedTimeResponse = 3341,

    // ═══════════════════════════════════════════════════════════════
    // MAP / MINIMAP / WAYPOINTS (3400-3499)
    // ═══════════════════════════════════════════════════════════════
    MapExplore = 3400,
    MapExploreUpdate = 3401,
    MapFogReveal = 3402,
    MapExploreResponse = 3403,
    WaypointSet = 3410,
    WaypointSetResponse = 3411,
    WaypointClear = 3412,
    WaypointClearResponse = 3413,
    WaypointShare = 3414,
    WaypointShareResponse = 3415,
    WaypointAccept = 3416,
    WaypointAcceptResponse = 3417,
    WaypointUpdatedEvent = 3418,
    PingMap = 3420,
    PingMapResponse = 3421,
    PingMapEvent = 3422,
    FlightpathDiscover = 3430,
    FlightpathListRequest = 3431,
    FlightpathListResponse = 3432,
    FlightpathRequest = 3433,
    FlightpathRequestResponse = 3434,
    FlightpathStart = 3435,
    MapMarkerAdd = 3440,
    MapMarkerAddResponse = 3441,
    MapMarkerRemove = 3442,
    MapMarkerRemoveResponse = 3443,
    MapMarkerUpdate = 3444,
    MapMarkerSyncEvent = 3445,
    WorldMapRequest = 3450,
    WorldMapResponse = 3451,
    MinimapUpdate = 3452,
    AreaDiscovered = 3453,
    MapDiscoveryDeltaEvent = 3454,
    MapStateSyncRequest = 3455,
    MapStateSyncResponse = 3456,

    // ═══════════════════════════════════════════════════════════════
    // VOICE CHAT / AUDIO (3500-3599)
    // ═══════════════════════════════════════════════════════════════
    VoiceJoinChannel = 3500,
    VoiceJoinResult = 3501,
    VoiceLeaveChannel = 3502,
    VoiceChannelList = 3503,
    VoiceMute = 3510,
    VoiceUnmute = 3511,
    VoiceDeafen = 3512,
    VoiceUndeafen = 3513,
    VoiceSpeaking = 3514,
    VoiceVolume = 3515,
    VoiceData = 3520,
    AudioTrigger = 3530,
    AudioStop = 3531,
    MusicChange = 3532,
    AmbienceChange = 3533,

    // ═══════════════════════════════════════════════════════════════
    // REPORTING / MODERATION (3600-3699)
    // ═══════════════════════════════════════════════════════════════
    ReportPlayer = 3600,
    ReportPlayerResult = 3601,
    ReportChat = 3602,
    ReportChatResult = 3603,
    ReportBug = 3604,
    ReportBugResult = 3605,
    ReportSuggestion = 3606,
    ReportSuggestionResult = 3607,
    ReportExploit = 3608,
    ReportExploitResult = 3609,
    AppealRequest = 3610,
    AppealResult = 3611,
    ReportStatusRequest = 3612,
    ReportStatusResponse = 3613,
    ReportEvidenceAdd = 3614,
    ReportEvidenceAddResult = 3615,
    ModerationAction = 3620,
    ModerationWarning = 3621,
    ModerationMute = 3622,
    ModerationBan = 3623,
    FeedbackPrompt = 3630,
    FeedbackSubmit = 3631,
    FeedbackSubmitResult = 3632,
    SurveyShow = 3633,
    SurveySubmit = 3634,
    SurveySubmitResult = 3635,
    RatingPrompt = 3636,
    RatingSubmit = 3637,
    RatingSubmitResult = 3638,
    ReportReceivedEvent = 3640,

    // ═══════════════════════════════════════════════════════════════
    // ECONOMY / CURRENCY (3700-3799)
    // ═══════════════════════════════════════════════════════════════
    CurrencyUpdate = 3700,
    CurrencyListRequest = 3701,
    CurrencyListResponse = 3702,
    GoldUpdate = 3703,
    GoldTransaction = 3704,
    CurrencyExchange = 3710,
    CurrencyExchangeResult = 3711,
    CurrencyCap = 3712,
    TokenPurchase = 3720,
    TokenPurchaseResult = 3721,
    TokenRedeem = 3722,
    TokenRedeemResult = 3723,
    PremiumCurrencyUpdate = 3724,
    BountyPlace = 3730,
    BountyList = 3731,
    BountyClaim = 3732,
    BountyClaimResult = 3733,

    // ═══════════════════════════════════════════════════════════════
    // SKILLS / TALENTS / ABILITIES (3800-3899)
    // ═══════════════════════════════════════════════════════════════
    SkillListRequest = 3800,
    SkillListResponse = 3801,
    SkillLearn = 3802,
    SkillLearnResult = 3803,
    SkillUnlearn = 3804,
    SkillUpgrade = 3805,
    SkillUpgradeResult = 3806,
    TalentListRequest = 3810,
    TalentListResponse = 3811,
    TalentLearn = 3812,
    TalentLearnResult = 3813,
    TalentReset = 3814,
    TalentResetResult = 3815,
    TalentPreview = 3816,
    SpecializationList = 3820,
    SpecializationChange = 3821,
    SpecializationChangeResult = 3822,
    AbilityBarUpdate = 3830,
    AbilityBarSlotSet = 3831,
    AbilityBarSlotClear = 3832,
    AbilityBarSwap = 3833,
    PassiveListRequest = 3840,
    PassiveListResponse = 3841,
    PassiveUpdate = 3842,
    GlyphApply = 3850,
    GlyphRemove = 3851,
    GlyphListRequest = 3852,
    GlyphListResponse = 3853,

    // ═══════════════════════════════════════════════════════════════
    // EQUIPMENT / GEAR (3900-3999)
    // ═══════════════════════════════════════════════════════════════
    EquipItem = 3900,
    EquipItemResult = 3901,
    UnequipItem = 3902,
    UnequipItemResult = 3903,
    EquipmentSync = 3904,
    EquipmentSlotUpdate = 3905,
    DurabilityUpdate = 3910,
    DurabilityWarning = 3911,
    ItemBroken = 3912,
    GemSocket = 3920,
    GemSocketResult = 3921,
    GemRemove = 3922,
    EnchantApply = 3930,
    EnchantApplyResult = 3931,
    EnchantRemove = 3932,
    ReforgeOpen = 3940,
    ReforgePreview = 3941,
    ReforgeConfirm = 3942,
    ReforgeResult = 3943,
    SetBonusUpdate = 3950,
    SetBonusActivate = 3951,
    SetBonusDeactivate = 3952,
    WeaponSwapRequest = 3960,
    WeaponSwapResult = 3961,
    OutfitSave = 3970,
    OutfitLoad = 3971,
    OutfitDelete = 3972,
    OutfitList = 3973,

    // ═══════════════════════════════════════════════════════════════
    // BANK / STORAGE (4000-4099)
    // ═══════════════════════════════════════════════════════════════
    BankOpen = 4000,
    BankClose = 4001,
    BankDeposit = 4002,
    BankDepositResult = 4003,
    BankWithdraw = 4004,
    BankWithdrawResult = 4005,
    BankSlotPurchase = 4006,
    BankSlotPurchaseResult = 4007,
    BankTabPurchase = 4008,
    BankSync = 4009,
    GuildBankOpenMsg = 4020,
    GuildBankCloseMsg = 4021,
    GuildBankDepositMsg = 4022,
    GuildBankWithdrawMsg = 4023,
    GuildBankLogMsg = 4024,
    GuildBankTabInfo = 4025,
    GuildBankSyncMsg = 4026,
    VoidStorageOpen = 4040,
    VoidStorageClose = 4041,
    VoidStorageDeposit = 4042,
    VoidStorageWithdraw = 4043,
    VoidStorageSync = 4044,
    ReagentBankOpen = 4050,
    ReagentBankDeposit = 4051,
    ReagentBankSync = 4052,

    // ═══════════════════════════════════════════════════════════════
    // DEATH / RESPAWN / GHOST (4100-4199)
    // ═══════════════════════════════════════════════════════════════
    DeathNotification = 4100,
    DeathRecap = 4101,
    GhostModeStart = 4110,
    GhostModeEnd = 4111,
    GhostPosition = 4112,
    CorpseLocation = 4113,
    CorpseRevive = 4114,
    RespawnRequest = 4120,
    RespawnAtGraveyard = 4121,
    RespawnAtCheckpoint = 4122,
    RespawnTimer = 4123,
    RespawnComplete = 4124,
    ResurrectOffer = 4130,
    ResurrectAccept = 4131,
    ResurrectDecline = 4132,
    ResurrectComplete = 4133,
    SoulstoneResurrect = 4134,
    BattleResurrect = 4135,
    ReleaseSpirit = 4140,
    RetrieveCorpse = 4141,
    SpiritHealerRevive = 4142,
    ResurrectionSickness = 4143,

    // ═══════════════════════════════════════════════════════════════
    // TRANSPORTATION (4200-4299)
    // ═══════════════════════════════════════════════════════════════
    FlightStart = 4200,
    FlightEnd = 4201,
    FlightCancel = 4202,
    FlightPathUpdate = 4203,
    PortalUse = 4210,
    PortalCreate = 4211,
    PortalExpire = 4212,
    HearthstoneUse = 4220,
    HearthstoneSet = 4221,
    HearthstoneCooldown = 4222,
    SummonRequest = 4230,
    SummonAccept = 4231,
    SummonDecline = 4232,
    SummonComplete = 4233,
    SummonFailed = 4234,
    MeetingStoneQueue = 4235,
    MeetingStoneResult = 4236,
    VehicleMount = 4240,
    VehicleDismount = 4241,
    VehicleControl = 4242,
    VehicleAbility = 4243,
    BoatArrival = 4250,
    BoatDeparture = 4251,
    ZeppelinArrival = 4252,
    ZeppelinDeparture = 4253,
    TramArrival = 4254,
    TramDeparture = 4255,
    TaxiRequest = 4260,
    TaxiConfirm = 4261,

    // ═══════════════════════════════════════════════════════════════
    // NOTIFICATIONS / ALERTS (4300-4399)
    // ═══════════════════════════════════════════════════════════════
    NotificationShow = 4300,
    NotificationDismiss = 4301,
    NotificationQueue = 4302,
    AlertPopup = 4310,
    AlertConfirm = 4311,
    AlertDismiss = 4312,
    ToastMessage = 4320,
    ToastAchievement = 4321,
    ToastLevelUp = 4322,
    ToastLoot = 4323,
    BossWarning = 4330,
    BossAbility = 4331,
    BossPhase = 4332,
    CountdownStart = 4340,
    CountdownUpdate = 4341,
    CountdownCancel = 4342,
    ScreenEffect = 4350,
    ScreenShake = 4351,
    ScreenFlash = 4352,
    ScreenFade = 4353,

    // ═══════════════════════════════════════════════════════════════
    // CUTSCENES / CINEMATICS (4400-4499)
    // ═══════════════════════════════════════════════════════════════
    CutsceneStart = 4400,
    CutsceneEnd = 4401,
    CutsceneSkip = 4402,
    CutscenePause = 4403,
    CutsceneResume = 4404,
    CutsceneProgress = 4405,

    // ═══════════════════════════════════════════════════════════════
    // HOUSING / PLAYER BUILDINGS (4500-4599)
    // ═══════════════════════════════════════════════════════════════
    HousingEnter = 4500,
    HousingLeave = 4501,
    HousingEdit = 4502,
    HousingPlace = 4503,
    HousingRemove = 4504,
    HousingSave = 4505,

    // ═══════════════════════════════════════════════════════════════
    // EVENTS / SEASONAL CONTENT (4600-4699)
    // ═══════════════════════════════════════════════════════════════
    EventStart = 4600,
    EventEnd = 4601,
    EventProgress = 4602,
    SeasonalStart = 4603,
    SeasonalEnd = 4604,
    EventListRequest = 4605,
    EventListResponse = 4606,
    EventDiscoveryEvent = 4607,
    EventSubscribeRequest = 4608,
    EventSubscribeResponse = 4609,
    EventSubscribedEvent = 4610,
    EventUnsubscribeRequest = 4611,
    EventUnsubscribeResponse = 4612,
    EventSubscriptionDroppedEvent = 4613,
    EventJoinRequest = 4614,
    EventJoinResponse = 4615,
    EventLeaveRequest = 4616,
    EventLeaveResponse = 4617,
    EventParticipantUpdateEvent = 4618,
    EventPhaseChangedEvent = 4619,
    EventProgressDeltaEvent = 4620,
    EventProgressSnapshot = 4621,
    EventObjectiveUpdateEvent = 4622,
    EventStateSyncRequest = 4623,
    EventStateSyncResponse = 4624,
    EventRewardAvailableEvent = 4625,
    EventRewardClaimRequest = 4626,
    EventRewardClaimResponse = 4627,
    EventRewardDeliveredEvent = 4628,
    EventContributionUpdateEvent = 4629,
    EventThrottleNotice = 4630,
    EventRevisionMismatch = 4631,
    EventRejoinRequest = 4632,
    EventRejoinResponse = 4633,
    EventInstanceDisbandEvent = 4634,
    EventSubscriptionHeartbeat = 4635,
    EventSubscriptionAck = 4636,
    EventSeasonInfoRequest = 4637,
    EventSeasonInfoResponse = 4638,
    EventMilestoneUnlockedEvent = 4639,
    EventPhasePreviewRequest = 4640,
    EventPhasePreviewResponse = 4641,
    EventParticipationSummaryRequest = 4642,
    EventParticipationSummaryResponse = 4643,
    EventAdminCommand = 4644,
    EventAdminCommandResponse = 4645,

    // ═══════════════════════════════════════════════════════════════
    // DEBUG / DEVELOPMENT (4900-4999)
    // ═══════════════════════════════════════════════════════════════
    DebugCommand = 4900,
    DebugResponse = 4901,
    DebugLog = 4902,
    DebugTeleport = 4903,
    DebugSpawn = 4904,

    // ═══════════════════════════════════════════════════════════════
    // ═══════════════════════════════════════════════════════════════
    // SERVER-TO-SERVER (5000-5099)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>S2S: Initial handshake between cluster nodes. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY - Never sent by clients. Establishes trust between servers in cluster.</remarks>
    ClusterHandshakeRequest = 5000,
    
    /// <summary>S2S: Handshake response. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Confirms cluster membership and shared secrets.</remarks>
    ClusterHandshakeResponse = 5001,
    
    /// <summary>S2S: Authentication challenge. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Mutual authentication between cluster nodes.</remarks>
    ClusterAuthChallengeRequest = 5002,
    
    /// <summary>S2S: Auth challenge response. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Provides authentication proof.</remarks>
    ClusterAuthChallengeResponse = 5003,
    
    /// <summary>S2S: Encryption key rotation request. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Triggers security key refresh across cluster.</remarks>
    ClusterKeyRotationRequest = 5004,
    
    /// <summary>S2S: Key rotation confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Acknowledges key rotation completion.</remarks>
    ClusterKeyRotationResponse = 5005,
    
    /// <summary>S2S: Health check request. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Monitors server health: CPU, memory, active players.</remarks>
    HealthStatusRequest = 5006,
    
    /// <summary>S2S: Health status report. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Returns: Load level, player count, capacity.</remarks>
    HealthStatusResponse = 5007,
    
    /// <summary>S2S: Load metrics report request. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. For load balancing decisions.</remarks>
    LoadReportRequest = 5008,
    
    /// <summary>S2S: Load metrics data. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Detailed load: Tick time, bandwidth, zone populations.</remarks>
    LoadReportResponse = 5009,
    
    /// <summary>S2S: Time synchronization request. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Ensures consistent server timestamps across cluster.</remarks>
    TimeSyncRequest = 5010,
    
    /// <summary>S2S: Synchronized time response. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Returns authoritative time for clock adjustment.</remarks>
    TimeSyncResponse = 5011,
    
    /// <summary>S2S: Graceful node shutdown preparation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Triggers player migration before shutdown.</remarks>
    NodeDrainRequest = 5012,
    
    /// <summary>S2S: Node drain acknowledgment. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Confirms drain readiness or completion.</remarks>
    NodeDrainResponse = 5013,
    
    /// <summary>S2S: Prepare player transfer between zones/shards. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Two-phase commit: Lock player state for transfer.</remarks>
    PlayerTransferPrepareRequest = 5014,
    
    /// <summary>S2S: Transfer preparation confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Ready to receive player or reports error.</remarks>
    PlayerTransferPrepareResponse = 5015,
    
    /// <summary>S2S: Commit player transfer. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Finalizes transfer, releases source state.</remarks>
    PlayerTransferCommitRequest = 5016,
    
    /// <summary>S2S: Transfer commit confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Player now active on target server.</remarks>
    PlayerTransferCommitResponse = 5017,
    
    /// <summary>S2S: Abort player transfer. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Rollback transfer on error, unlock state.</remarks>
    PlayerTransferAbortRequest = 5018,
    
    /// <summary>S2S: Transfer abort confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. State restored on source server.</remarks>
    PlayerTransferAbortResponse = 5019,
    
    /// <summary>S2S: Hand off entity ownership. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Transfer entity control between zone servers.</remarks>
    EntityHandoffRequest = 5020,
    
    /// <summary>S2S: Entity handoff confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Target assumes entity ownership.</remarks>
    EntityHandoffResponse = 5021,
    
    /// <summary>S2S: Acquire distributed lease. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Distributed locking for: Guild leadership, rare spawns, singletons.</remarks>
    LeaseAcquireRequest = 5022,
    
    /// <summary>S2S: Lease acquisition result. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Granted or denied with current holder info.</remarks>
    LeaseAcquireResponse = 5023,
    
    /// <summary>S2S: Renew existing lease. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Extend lease before expiration.</remarks>
    LeaseRenewRequest = 5024,
    
    /// <summary>S2S: Lease renewal confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. New expiration time or renewal failed.</remarks>
    LeaseRenewResponse = 5025,
    
    /// <summary>S2S: Release held lease. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Voluntary lease release.</remarks>
    LeaseReleaseRequest = 5026,
    
    /// <summary>S2S: Lease release confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Lease now available for acquisition.</remarks>
    LeaseReleaseResponse = 5027,
    
    /// <summary>S2S: Query partition ownership. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Find which server owns data partition (guild, zone shard, etc.).</remarks>
    PartitionOwnershipQueryRequest = 5028,
    
    /// <summary>S2S: Partition owner info. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Returns: Owner server ID, routing info.</remarks>
    PartitionOwnershipQueryResponse = 5029,
    
    /// <summary>S2S: Publish event for replication. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Pub/sub for cross-zone events: World bosses, server announcements.</remarks>
    EventReplicationPublishRequest = 5030,
    
    /// <summary>S2S: Event publish acknowledgment. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Confirms event delivery.</remarks>
    EventReplicationPublishResponse = 5031,
    
    /// <summary>S2S: Synchronize guild data. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Cross-server guild state sync.</remarks>
    GuildSyncRequest = 5032,
    
    /// <summary>S2S: Guild sync data. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Guild roster, bank, perms across zones.</remarks>
    GuildSyncResponse = 5033,
    
    /// <summary>S2S: Synchronize party data. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Cross-server party state for member in different zones.</remarks>
    PartySyncRequest = 5034,
    
    /// <summary>S2S: Party sync data. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Party members, health, position updates.</remarks>
    PartySyncResponse = 5035,
    
    /// <summary>S2S: Register chat routing endpoint. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Announce chat relay capability to cluster.</remarks>
    ChatRouteRegisterRequest = 5036,
    
    /// <summary>S2S: Chat route registration confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Registered in routing table.</remarks>
    ChatRouteRegisterResponse = 5037,
    
    /// <summary>S2S: Relay chat message across zones. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Cross-zone guild/party chat routing.</remarks>
    ChatEnvelopeRelayRequest = 5038,
    
    /// <summary>S2S: Chat relay acknowledgment. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Message delivered to target zone.</remarks>
    ChatEnvelopeRelayResponse = 5039,
    
    /// <summary>S2S: Broadcast admin command cluster-wide. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Admin actions: Server shutdown, announcements, bans.</remarks>
    AdminBroadcastRequest = 5040,
    
    /// <summary>S2S: Admin broadcast confirmation. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Command executed on node.</remarks>
    AdminBroadcastResponse = 5041,
    
    /// <summary>S2S: Trigger config reload. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Hot-reload config changes without restart.</remarks>
    ConfigReloadRequest = 5042,
    
    /// <summary>S2S: Config reload result. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Success or error loading new config.</remarks>
    ConfigReloadResponse = 5043,
    
    /// <summary>S2S: Query circuit breaker state. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Check if service circuit is open/closed/half-open.</remarks>
    CircuitBreakerStateRequest = 5044,
    
    /// <summary>S2S: Circuit breaker state report. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Current state and trip conditions.</remarks>
    CircuitBreakerStateResponse = 5045,
    
    /// <summary>S2S: Backpressure alert. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Server overloaded, requesting reduced traffic.</remarks>
    BackpressureAlertRequest = 5046,
    
    /// <summary>S2S: Backpressure acknowledgment. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Will throttle requests to overloaded node.</remarks>
    BackpressureAlertResponse = 5047,
    
    /// <summary>S2S: Validate session token. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Gateway validates session against auth service.</remarks>
    SessionValidateS2SRequest = 5048,
    
    /// <summary>S2S: Session validation result. Direction: Server→Server.</summary>
    /// <remarks>INTERNAL ONLY. Session valid with account info or invalid.</remarks>
    SessionValidateS2SResponse = 5049
}
