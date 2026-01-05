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
    
    /// <summary>Character leveled up. Direction: Server→Client.</summary>
    /// <remarks>Contains: New level, Stat increases, Rewards unlocked. Triggers celebration effects.</remarks>
    LevelUp = 600,
    
    /// <summary>Experience points gained. Direction: Server→Client.</summary>
    /// <remarks>From: Combat, quests, exploration. Contains: XP amount, Source, Progress to next level.</remarks>
    XpGain = 601,
    
    /// <summary>Single stat changed. Direction: Server→Client.</summary>
    /// <remarks>Individual stat update (Strength, Agility, etc.). Triggers UI refresh.</remarks>
    StatUpdate = 602,
    
    /// <summary>Full stats synchronization. Direction: Server→Client.</summary>
    /// <remarks>Complete stat sheet on zone in or major changes. All base and modified stats.</remarks>
    StatFullSync = 603,
    
    /// <summary>Resource update (health/mana/energy). Direction: Server→Client.</summary>
    /// <remarks>Current and max values for primary resources. High frequency.</remarks>
    ResourceUpdate = 604,
    
    /// <summary>Resource regeneration tick. Direction: Server→Client.</summary>
    /// <remarks>Periodic regen from: Resting, buffs, equipment. Amount per tick.</remarks>
    ResourceRegen = 605,
    
    /// <summary>Character info snapshot. Direction: Server→Client.</summary>
    /// <remarks>Complete character data: Stats, level, class, race, equipment summary.</remarks>
    CharacterInfo = 606,
    
    /// <summary>Request character info. Direction: Client→Server.</summary>
    /// <remarks>For self or inspecting others. Response: CharacterInfo (606).</remarks>
    CharacterInfoRequest = 607,
    
    /// <summary>Skill points gained. Direction: Server→Client.</summary>
    /// <remarks>From leveling. Used to learn/upgrade skills. Contains: Amount, Total available.</remarks>
    SkillPointGain = 608,
    
    /// <summary>Talent points gained. Direction: Server→Client.</summary>
    /// <remarks>From leveling. Used in talent trees. Contains: Amount, Total available.</remarks>
    TalentPointGain = 609,
    
    /// <summary>Reputation changed with faction. Direction: Server→Client.</summary>
    /// <remarks>Increase/decrease reputation. Contains: Faction, Amount, New standing level.</remarks>
    ReputationChange = 610,
    
    /// <summary>Request reputation standings. Direction: Client→Server.</summary>
    /// <remarks>List all faction reputations. Response: ReputationListResponse (612).</remarks>
    ReputationListRequest = 611,
    
    /// <summary>Reputation standings list. Direction: Server→Client.</summary>
    /// <remarks>All faction reputations with current standing and progress.</remarks>
    ReputationListResponse = 612,
    
    /// <summary>New title unlocked. Direction: Server→Client.</summary>
    /// <remarks>From achievements, reputation, quests. Contains: Title ID, Name, Requirements met.</remarks>
    TitleUnlocked = 613,
    
    /// <summary>Select active title. Direction: Client→Server.</summary>
    /// <remarks>Set displayed title. Response: TitleChangeResponse (655).</remarks>
    TitleSelect = 614,
    
    /// <summary>Change character appearance. Direction: Client→Server.</summary>
    /// <remarks>Modify cosmetic features. May require item/payment. Response: CharacterCustomizeResponse (651).</remarks>
    AppearanceChange = 617,
    
    /// <summary>Preview appearance change. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Test appearance before committing. No cost to preview.</remarks>
    AppearancePreview = 618,
    
    /// <summary>Change character race. Direction: Client→Server.</summary>
    /// <remarks>Paid service. Major change. May affect available classes/abilities.</remarks>
    RaceChange = 619,
    
    /// <summary>Change character class. Direction: Client→Server.</summary>
    /// <remarks>Paid service. Resets talents/skills. Validate class availability for race.</remarks>
    ClassChange = 620,
    
    /// <summary>Change character name. Direction: Client→Server.</summary>
    /// <remarks>Paid service. Subject to profanity filter and uniqueness check.</remarks>
    NameChange = 621,
    
    /// <summary>Change character gender. Direction: Client→Server.</summary>
    /// <remarks>Paid service. Cosmetic change only.</remarks>
    GenderChange = 622,
    
    /// <summary>Rested XP bonus updated. Direction: Server→Client.</summary>
    /// <remarks>Bonus XP from resting in inn/city. Contains: Bonus amount, Percentage.</remarks>
    RestXpUpdate = 623,
    
    /// <summary>Rest state changed. Direction: Server→Client.</summary>
    /// <remarks>Entered/exited rested area. Affects XP gain and regen rates.</remarks>
    RestStateChange = 624,
    
    /// <summary>Attribute increase response. Direction: Server→Client.</summary>
    AttributeIncreaseResponse = 650,
    
    /// <summary>Character customization response. Direction: Server→Client.</summary>
    CharacterCustomizeResponse = 651,
    
    /// <summary>Talent learn response. Direction: Server→Client.</summary>
    TalentLearnResponse = 652,
    
    /// <summary>Talent reset response. Direction: Server→Client.</summary>
    TalentResetResponse = 653,
    
    /// <summary>Specialization change response. Direction: Server→Client.</summary>
    SpecializationChangeResponse = 654,
    
    /// <summary>Title change response. Direction: Server→Client.</summary>
    TitleChangeResponse = 655,

    // ═══════════════════════════════════════════════════════════════
    // GROUP / PARTY (0700-0799)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Invite player to party. Direction: Client→Server.</summary>
    /// <remarks>Payload: Target player name/ID. Response: PartyAcceptResponse (740) or PartyInviteResponse (701).</remarks>
    PartyInvite = 700,
    
    /// <summary>Party invite response from invitee. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Accept or decline invitation. Creates party if first member.</remarks>
    PartyInviteResponse = 701,
    
    /// <summary>Leave current party. Direction: Client→Server.</summary>
    /// <remarks>Voluntary party exit. Response: PartyLeaveResponse (741). Broadcast to remaining members.</remarks>
    PartyLeave = 702,
    
    /// <summary>Kick member from party. Direction: Client→Server.</summary>
    /// <remarks>Leader/assistant only. Payload: Target player. Response: PartyKickResponse (742).</remarks>
    PartyKick = 703,
    
    /// <summary>Party state update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Member list changes, settings updates. Sent to all party members.</remarks>
    PartyUpdate = 704,
    
    /// <summary>Disband party. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Removes all members. Response: PartyDisbandResponse (744).</remarks>
    PartyDisband = 705,
    
    /// <summary>Party leadership changed. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>New leader assigned. May be manual promotion or automated on leader leave.</remarks>
    PartyLeaderChange = 706,
    
    /// <summary>Loot distribution method changed. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Leader sets: Free-for-all, Round-robin, Master looter, Need/Greed.</remarks>
    PartyLootChange = 707,
    
    /// <summary>Ready check initiated. Direction: Client→Server or Server→Client (Broadcast).</summary>
    /// <remarks>Leader asks if members are ready. Triggers ready check UI for all.</remarks>
    PartyReadyCheck = 708,
    
    /// <summary>Ready check response. Direction: Client→Server.</summary>
    /// <remarks>Member indicates ready/not ready. Aggregated for leader.</remarks>
    PartyReadyResponse = 709,
    
    /// <summary>Party member data update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Level, class, equipment changes. For party UI updates.</remarks>
    PartyMemberUpdate = 710,
    
    /// <summary>Party member position update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>For party frames and map markers. Cross-zone support.</remarks>
    PartyPositionUpdate = 711,
    
    /// <summary>Party member health update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Current/max health for party frames. High frequency.</remarks>
    PartyHealthUpdate = 712,
    
    /// <summary>Party member resource update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Mana/energy/rage for party frames. High frequency.</remarks>
    PartyResourceUpdate = 713,
    
    /// <summary>Party member buff/debuff update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Active auras on party members. For dispel/cleanse coordination.</remarks>
    PartyBuffUpdate = 714,
    
    /// <summary>Party member target update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>What party member is targeting. For coordination.</remarks>
    PartyTargetUpdate = 715,
    
    /// <summary>Set party member role. Direction: Client→Server.</summary>
    /// <remarks>Tank/Healer/DPS designation. For dungeon finder and organization.</remarks>
    PartyRoleSet = 716,
    
    /// <summary>Check party roles. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Verify role assignments before queuing. Response with role coverage.</remarks>
    PartyRoleCheck = 717,
    
    /// <summary>Convert party to raid. Direction: Client→Server.</summary>
    /// <remarks>Expand 5-player party to raid (up to 40). Leader only.</remarks>
    PartyConvertToRaid = 718,
    
    /// <summary>Synchronize party state. Direction: Server→Client.</summary>
    /// <remarks>Full party data on join or reconnect. All members, settings, loot rules.</remarks>
    PartySync = 719,
    
    /// <summary>Summon party to location. Direction: Client→Server.</summary>
    /// <remarks>Warlock/mage summon ability. Response: PartySummonResponse (721) to target.</remarks>
    PartySummon = 720,
    
    /// <summary>Party summon response. Direction: Client→Server.</summary>
    /// <remarks>Target accepts or declines summon.</remarks>
    PartySummonResponse = 721,
    
    /// <summary>Set raid marker on target. Direction: Client→Server.</summary>
    /// <remarks>Visual markers (skull, cross, etc.) for coordination. Leader/assistant only.</remarks>
    PartyMarkerSet = 722,
    
    /// <summary>Clear raid marker. Direction: Client→Server.</summary>
    /// <remarks>Remove specific or all markers.</remarks>
    PartyMarkerClear = 723,
    
    /// <summary>Vote on dungeon difficulty. Direction: Client→Server.</summary>
    /// <remarks>Normal/Heroic/Mythic voting. Requires majority.</remarks>
    PartyDifficultyVote = 724,
    
    /// <summary>Difficulty set confirmed. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>New difficulty active. Affects loot and enemy stats.</remarks>
    PartyDifficultySet = 725,
    
    /// <summary>Party accept response. Direction: Server→Client.</summary>
    PartyAcceptResponse = 740,
    
    /// <summary>Party leave response. Direction: Server→Client.</summary>
    PartyLeaveResponse = 741,
    
    /// <summary>Party kick response. Direction: Server→Client.</summary>
    PartyKickResponse = 742,
    
    /// <summary>Party promote response. Direction: Server→Client.</summary>
    PartyPromoteResponse = 743,
    
    /// <summary>Party disband response. Direction: Server→Client.</summary>
    PartyDisbandResponse = 744,
    
    /// <summary>Loot mode change response. Direction: Server→Client.</summary>
    PartyLootModeResponse = 745,
    
    /// <summary>Ready check start response. Direction: Server→Client.</summary>
    PartyReadyCheckStartResponse = 746,

    // ═══════════════════════════════════════════════════════════════
    // GUILD (0800-0899)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Invite player to guild. Direction: Client→Server.</summary>
    /// <remarks>Officer/leader action. Payload: Target player. Response: GuildInviteResponse (801).</remarks>
    GuildInvite = 800,
    
    /// <summary>Guild invite response. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Target accepts or declines invitation.</remarks>
    GuildInviteResponse = 801,
    
    /// <summary>Leave guild. Direction: Client→Server.</summary>
    /// <remarks>Voluntary guild exit. Response: GuildLeaveResponse (841).</remarks>
    GuildLeave = 802,
    
    /// <summary>Kick member from guild. Direction: Client→Server.</summary>
    /// <remarks>Officer action. Payload: Target member. Response: GuildKickResponse (842).</remarks>
    GuildKick = 803,
    
    /// <summary>Guild data update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>General guild info changes. Sent to all online members.</remarks>
    GuildUpdate = 804,
    
    /// <summary>Disband guild. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Permanent action. Response: GuildDisbandResponse (843).</remarks>
    GuildDisband = 805,
    
    /// <summary>Promote guild member. Direction: Client→Server.</summary>
    /// <remarks>Leader action. Increase rank. Response: GuildPromoteResponse (844).</remarks>
    GuildPromote = 806,
    
    /// <summary>Demote guild member. Direction: Client→Server.</summary>
    /// <remarks>Leader action. Decrease rank. Response: GuildDemoteResponse (845).</remarks>
    GuildDemote = 807,
    
    /// <summary>Guild Message of the Day. Direction: Server→Client.</summary>
    /// <remarks>Displayed on login. Set by officers/leader.</remarks>
    GuildMotd = 808,
    
    /// <summary>Set guild MOTD. Direction: Client→Server.</summary>
    /// <remarks>Officer action. Response: GuildMOTDResponse (847).</remarks>
    GuildMotdSet = 809,
    
    /// <summary>Request guild roster. Direction: Client→Server.</summary>
    /// <remarks>Get member list. Response: GuildRosterResponse (811).</remarks>
    GuildRosterRequest = 810,
    
    /// <summary>Guild roster data. Direction: Server→Client.</summary>
    /// <remarks>All members with: Name, rank, level, online status, last login.</remarks>
    GuildRosterResponse = 811,
    
    /// <summary>Create new guild rank. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Define rank name and permissions.</remarks>
    GuildRankCreate = 812,
    
    /// <summary>Delete guild rank. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Move members to default rank first.</remarks>
    GuildRankDelete = 813,
    
    /// <summary>Edit guild rank. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Change name/permissions. Response: GuildRankEditResponse (846).</remarks>
    GuildRankEdit = 814,
    
    /// <summary>Reorder guild ranks. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Change rank hierarchy.</remarks>
    GuildRankReorder = 815,
    
    /// <summary>Set guild permissions. Direction: Client→Server.</summary>
    /// <remarks>Leader only. Fine-grained permission control per rank.</remarks>
    GuildPermissionSet = 816,
    
    /// <summary>Edit guild info. Direction: Client→Server.</summary>
    /// <remarks>Change guild description, rules, requirements. Leader action.</remarks>
    GuildInfoEdit = 817,
    
    /// <summary>Change guild tabard. Direction: Client→Server.</summary>
    /// <remarks>Modify guild emblem/colors. Leader only.</remarks>
    GuildTabardChange = 818,
    
    /// <summary>Open guild bank. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access shared storage. Permissions-based access.</remarks>
    GuildBankOpen = 819,
    
    /// <summary>Deposit to guild bank. Direction: Client→Server.</summary>
    /// <remarks>Add items/gold. Logged for audit. Response: GuildBankDepositResponse (849).</remarks>
    GuildBankDeposit = 820,
    
    /// <summary>Withdraw from guild bank. Direction: Client→Server.</summary>
    /// <remarks>Take items/gold. Permission and daily limit checks. Response: GuildBankWithdrawResponse (850).</remarks>
    GuildBankWithdraw = 821,
    
    /// <summary>Guild bank transaction log. Direction: Server→Client.</summary>
    /// <remarks>Audit trail of deposits/withdrawals.</remarks>
    GuildBankLog = 822,
    
    /// <summary>Create guild bank tab. Direction: Client→Server.</summary>
    /// <remarks>Expand storage. Costs gold. Leader only.</remarks>
    GuildBankTabCreate = 823,
    
    /// <summary>Edit guild bank tab. Direction: Client→Server.</summary>
    /// <remarks>Rename, set permissions. Officer action.</remarks>
    GuildBankTabEdit = 824,
    
    /// <summary>Set guild bank permissions. Direction: Client→Server.</summary>
    /// <remarks>Control who can withdraw/deposit per tab and rank.</remarks>
    GuildBankPermission = 825,
    
    /// <summary>Guild achievement unlocked. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Guild-wide achievements. Broadcast to all members.</remarks>
    GuildAchievement = 826,
    
    /// <summary>Guild news/activity feed. Direction: Server→Client.</summary>
    /// <remarks>Recent events: Achievements, member joins, level ups.</remarks>
    GuildNews = 827,
    
    /// <summary>Create guild event. Direction: Client→Server.</summary>
    /// <remarks>Schedule raid/activity. Officer action.</remarks>
    GuildEventCreate = 828,
    
    /// <summary>Edit guild event. Direction: Client→Server.</summary>
    /// <remarks>Modify event details. Creator/officer action.</remarks>
    GuildEventEdit = 829,
    
    /// <summary>Delete guild event. Direction: Client→Server.</summary>
    /// <remarks>Cancel event. Creator/officer action.</remarks>
    GuildEventDelete = 830,
    
    /// <summary>Sign up for guild event. Direction: Client→Server.</summary>
    /// <remarks>Member indicates attendance. Role selection.</remarks>
    GuildEventSignup = 831,
    
    /// <summary>Search for guilds. Direction: Client→Server.</summary>
    /// <remarks>Find guilds to join. Filter by: Activity, size, focus.</remarks>
    GuildSearch = 832,
    
    /// <summary>Apply to guild. Direction: Client→Server.</summary>
    /// <remarks>Submit application with message.</remarks>
    GuildApply = 833,
    
    /// <summary>List pending applications. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Officer view of applicants.</remarks>
    GuildApplicationList = 834,
    
    /// <summary>Application decision. Direction: Client→Server.</summary>
    /// <remarks>Officer accepts or declines application.</remarks>
    GuildApplicationResponse = 835,
    
    /// <summary>Invite guild to alliance. Direction: Client→Server.</summary>
    /// <remarks>Multi-guild cooperation. Leader action.</remarks>
    GuildAllianceInvite = 836,
    
    /// <summary>Alliance invite response. Direction: Client→Server.</summary>
    /// <remarks>Accept or decline alliance.</remarks>
    GuildAllianceResponse = 837,
    
    /// <summary>Leave guild alliance. Direction: Client→Server.</summary>
    /// <remarks>Break alliance. Leader action.</remarks>
    GuildAllianceLeave = 838,
    
    /// <summary>Guild create response. Direction: Server→Client.</summary>
    GuildCreateResponse = 840,
    
    /// <summary>Guild leave response. Direction: Server→Client.</summary>
    GuildLeaveResponse = 841,
    
    /// <summary>Guild kick response. Direction: Server→Client.</summary>
    GuildKickResponse = 842,
    
    /// <summary>Guild disband response. Direction: Server→Client.</summary>
    GuildDisbandResponse = 843,
    
    /// <summary>Guild promote response. Direction: Server→Client.</summary>
    GuildPromoteResponse = 844,
    
    /// <summary>Guild demote response. Direction: Server→Client.</summary>
    GuildDemoteResponse = 845,
    
    /// <summary>Guild rank edit response. Direction: Server→Client.</summary>
    GuildRankEditResponse = 846,
    
    /// <summary>Guild MOTD response. Direction: Server→Client.</summary>
    GuildMOTDResponse = 847,
    
    /// <summary>Guild message response. Direction: Server→Client.</summary>
    GuildMessageResponse = 848,
    
    /// <summary>Guild bank deposit response. Direction: Server→Client.</summary>
    GuildBankDepositResponse = 849,
    
    /// <summary>Guild bank withdraw response. Direction: Server→Client.</summary>
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
    
    /// <summary>Accept quest from NPC/object. Direction: Client→Server.</summary>
    /// <remarks>Payload: Quest ID. Response: QuestAcceptResult (1001) with success or requirements not met.</remarks>
    QuestAccept = 1000,
    
    /// <summary>Quest accept result. Direction: Server→Client.</summary>
    /// <remarks>Success adds to quest log. Failure indicates: Level too low, prerequisite missing, log full.</remarks>
    QuestAcceptResult = 1001,
    
    /// <summary>Abandon/drop quest. Direction: Client→Server.</summary>
    /// <remarks>Remove from quest log. Progress lost. Some quests cannot be abandoned.</remarks>
    QuestAbandon = 1002,
    
    /// <summary>Quest progress update. Direction: Server→Client.</summary>
    /// <remarks>Objective completion: Kill counts, item collection, exploration. Updates quest tracker.</remarks>
    QuestProgress = 1003,
    
    /// <summary>Complete/turn in quest. Direction: Client→Server.</summary>
    /// <remarks>Submit to NPC. All objectives met. Response: QuestCompleteResult (1005).</remarks>
    QuestComplete = 1004,
    
    /// <summary>Quest completion result. Direction: Server→Client.</summary>
    /// <remarks>Rewards granted: XP, gold, items, reputation. Quest removed from log.</remarks>
    QuestCompleteResult = 1005,
    
    /// <summary>Choose quest reward option. Direction: Client→Server.</summary>
    /// <remarks>When multiple reward options available. Selection before completion.</remarks>
    QuestRewardChoose = 1006,
    
    /// <summary>Receive quest reward. Direction: Server→Client.</summary>
    /// <remarks>Items/gold added to inventory. May fail if inventory full.</remarks>
    QuestRewardReceive = 1007,
    
    /// <summary>Request quest log. Direction: Client→Server.</summary>
    /// <remarks>Get all active quests. Response: QuestListResponse (1009).</remarks>
    QuestListRequest = 1008,
    
    /// <summary>Quest log data. Direction: Server→Client.</summary>
    /// <remarks>All active quests with current progress.</remarks>
    QuestListResponse = 1009,
    
    /// <summary>Quest log updated. Direction: Server→Client.</summary>
    /// <remarks>Quest added, removed, or progress changed. Incremental update.</remarks>
    QuestLogUpdate = 1010,
    
    /// <summary>Share quest with party. Direction: Client→Server.</summary>
    /// <remarks>Offer quest to party members. Response: QuestShareResponse (1012) from each member.</remarks>
    QuestShare = 1011,
    
    /// <summary>Quest share response. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Party member accepts or declines shared quest.</remarks>
    QuestShareResponse = 1012,
    
    /// <summary>Track quest. Direction: Client→Server.</summary>
    /// <remarks>Pin quest for on-screen tracker. Client preference.</remarks>
    QuestTrack = 1013,
    
    /// <summary>Untrack quest. Direction: Client→Server.</summary>
    /// <remarks>Remove from on-screen tracker.</remarks>
    QuestUntrack = 1014,
    
    /// <summary>Quest objective progress. Direction: Server→Client.</summary>
    /// <remarks>Specific objective updated: "Kill 5/10 wolves". Real-time feedback.</remarks>
    QuestObjectiveUpdate = 1015,
    
    /// <summary>Request quest point of interest. Direction: Client→Server.</summary>
    /// <remarks>Where to go for quest. Response: QuestPoiResponse (1017) with map coordinates.</remarks>
    QuestPoiRequest = 1016,
    
    /// <summary>Quest POI data. Direction: Server→Client.</summary>
    /// <remarks>Map markers for quest objectives and turn-in location.</remarks>
    QuestPoiResponse = 1017,
    
    /// <summary>Quest giver status. Direction: Server→Client.</summary>
    /// <remarks>NPC quest availability: Available, complete, in-progress. Visual indicators (!, ?).</remarks>
    QuestGiverStatus = 1018,
    
    /// <summary>Quest givers in area. Direction: Server→Client.</summary>
    /// <remarks>NPCs with quests nearby. For minimap markers.</remarks>
    QuestGiverList = 1019,
    
    /// <summary>Daily quests reset. Direction: Server→Client.</summary>
    /// <remarks>Daily quest cooldowns cleared. Can accept again.</remarks>
    DailyQuestReset = 1020,
    
    /// <summary>Weekly quests reset. Direction: Server→Client.</summary>
    /// <remarks>Weekly quest cooldowns cleared.</remarks>
    WeeklyQuestReset = 1021,
    
    /// <summary>Quest chain progression. Direction: Server→Client.</summary>
    /// <remarks>Next quest in chain unlocked. Story progression.</remarks>
    QuestChainUpdate = 1022,
    
    /// <summary>Repeatable quest available again. Direction: Server→Client.</summary>
    /// <remarks>Cooldown expired for repeatable quest.</remarks>
    QuestRepeatableReset = 1023,

    // ═══════════════════════════════════════════════════════════════
    // TRADING (1100-1199)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Request player-to-player trade. Direction: Client→Server.</summary>
    /// <remarks>Initiate trade with nearby player. Response: TradeRequestResponse (1101).</remarks>
    TradeRequest = 1100,
    
    /// <summary>Trade request response. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Target accepts or declines trade. Opens trade window on accept.</remarks>
    TradeRequestResponse = 1101,
    
    /// <summary>Trade state update. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Items/gold changed. Sent to both traders.</remarks>
    TradeUpdate = 1102,
    
    /// <summary>Add item to trade. Direction: Client→Server.</summary>
    /// <remarks>Place item in trade window. Unlocks both sides.</remarks>
    TradeSetItem = 1103,
    
    /// <summary>Remove item from trade. Direction: Client→Server.</summary>
    /// <remarks>Take item back. Unlocks both sides.</remarks>
    TradeRemoveItem = 1104,
    
    /// <summary>Set gold amount in trade. Direction: Client→Server.</summary>
    /// <remarks>Offer gold. Unlocks both sides if changed.</remarks>
    TradeSetGold = 1105,
    
    /// <summary>Confirm trade. Direction: Client→Server.</summary>
    /// <remarks>Ready to complete. Both must confirm for completion.</remarks>
    TradeConfirm = 1106,
    
    /// <summary>Unconfirm trade. Direction: Client→Server.</summary>
    /// <remarks>Cancel ready status. Change items again.</remarks>
    TradeUnconfirm = 1107,
    
    /// <summary>Lock trade for final confirm. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Both locked and confirmed = trade executes.</remarks>
    TradeLock = 1108,
    
    /// <summary>Cancel trade. Direction: Client→Server.</summary>
    /// <remarks>Abort trade. Items returned.</remarks>
    TradeCancel = 1109,
    
    /// <summary>Trade completed successfully. Direction: Server→Client.</summary>
    /// <remarks>Items and gold exchanged. Trade window closes.</remarks>
    TradeComplete = 1110,
    
    /// <summary>Trade error. Direction: Server→Client.</summary>
    /// <remarks>Trade failed: Inventory full, insufficient gold, item bound, etc.</remarks>
    TradeError = 1111,
    
    /// <summary>Trade request declined - busy. Direction: Server→Client.</summary>
    /// <remarks>You are already in trade/combat/busy.</remarks>
    TradeBusy = 1112,
    
    /// <summary>Trade target busy. Direction: Server→Client.</summary>
    /// <remarks>Target player is in trade/combat/busy.</remarks>
    TradeTargetBusy = 1113,

    // ═══════════════════════════════════════════════════════════════
    // TARGETING (1200-1299)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Select target entity. Direction: Client→Server.</summary>
    /// <remarks>Set active target for abilities/info. Response: TargetSelectResponse (1220).</remarks>
    TargetSelect = 1200,
    
    /// <summary>Clear current target. Direction: Client→Server.</summary>
    /// <remarks>Deselect target. UI updated.</remarks>
    TargetClear = 1201,
    
    /// <summary>Target changed update. Direction: Server→Client.</summary>
    /// <remarks>Confirms new target. Sends target info.</remarks>
    TargetUpdate = 1202,
    
    /// <summary>Request target info. Direction: Client→Server.</summary>
    /// <remarks>Get detailed target data. Response: TargetInfoResponse (1204).</remarks>
    TargetInfoRequest = 1203,
    
    /// <summary>Target info data. Direction: Server→Client.</summary>
    /// <remarks>Target name, level, health, buffs, hostility.</remarks>
    TargetInfoResponse = 1204,
    
    /// <summary>Target of target. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>What your target is targeting. Tactical info.</remarks>
    TargetOfTarget = 1205,
    
    /// <summary>Target of target update. Direction: Server→Client.</summary>
    /// <remarks>Your target changed their target.</remarks>
    TargetOfTargetUpdate = 1206,
    
    /// <summary>Set focus target. Direction: Client→Server.</summary>
    /// <remarks>Secondary target for monitoring. Separate from main target.</remarks>
    FocusTarget = 1207,
    
    /// <summary>Clear focus target. Direction: Client→Server.</summary>
    /// <remarks>Remove focus target.</remarks>
    FocusClear = 1208,
    
    /// <summary>Assist target. Direction: Client→Server.</summary>
    /// <remarks>Target same as party member. Response: AssistTargetResponse (1221).</remarks>
    AssistTarget = 1209,
    
    /// <summary>Mark target with icon. Direction: Client→Server.</summary>
    /// <remarks>Raid marker on enemy. Leader action. Response: MarkTargetResponse (1222).</remarks>
    MarkTarget = 1210,
    
    /// <summary>Clear single mark. Direction: Client→Server.</summary>
    /// <remarks>Remove marker from target.</remarks>
    MarkClear = 1211,
    
    /// <summary>Clear all marks. Direction: Client→Server.</summary>
    /// <remarks>Remove all raid markers.</remarks>
    MarkClearAll = 1212,
    
    /// <summary>Mouseover target. Direction: Client→Server.</summary>
    /// <remarks>Get info on hovered entity. Tooltip data.</remarks>
    MouseoverTarget = 1213,
    
    /// <summary>Tab to next target. Direction: Client→Server.</summary>
    /// <remarks>Cycle through nearby targets. Response: TabTargetResponse (1223).</remarks>
    TabTarget = 1214,
    
    /// <summary>Target nearest enemy. Direction: Client→Server.</summary>
    /// <remarks>Auto-target closest hostile. Response: NearestEnemyTargetResponse (1224).</remarks>
    NearestEnemyTarget = 1215,
    
    /// <summary>Target nearest friend. Direction: Client→Server.</summary>
    /// <remarks>Auto-target closest friendly. Response: NearestFriendTargetResponse (1225).</remarks>
    NearestFriendTarget = 1216,
    
    /// <summary>Target select response. Direction: Server→Client.</summary>
    TargetSelectResponse = 1220,
    
    /// <summary>Assist target response. Direction: Server→Client.</summary>
    AssistTargetResponse = 1221,
    
    /// <summary>Mark target response. Direction: Server→Client.</summary>
    MarkTargetResponse = 1222,
    
    /// <summary>Tab target response. Direction: Server→Client.</summary>
    TabTargetResponse = 1223,
    
    /// <summary>Nearest enemy target response. Direction: Server→Client.</summary>
    NearestEnemyTargetResponse = 1224,
    
    /// <summary>Nearest friend target response. Direction: Server→Client.</summary>
    NearestFriendTargetResponse = 1225,

    // ═══════════════════════════════════════════════════════════════
    // NPC / DIALOG / VENDOR (1300-1399)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Interact with NPC. Direction: Client→Server.</summary>
    /// <remarks>Right-click NPC. Response: NpcInteractResult (1301) with interaction type (dialog/vendor/quest).</remarks>
    NpcInteract = 1300,
    
    /// <summary>NPC interaction result. Direction: Server→Client.</summary>
    /// <remarks>Opens: Dialog window, vendor, quest giver, or shows error (too far, busy).</remarks>
    NpcInteractResult = 1301,
    
    /// <summary>NPC dialog window opened. Direction: Server→Client.</summary>
    /// <remarks>Dialog text and choice options. Story/lore interaction.</remarks>
    NpcDialogOpen = 1302,
    
    /// <summary>Select dialog option. Direction: Client→Server.</summary>
    /// <remarks>Choose conversation branch. May lead to quest, vendor, or new dialog.</remarks>
    NpcDialogChoice = 1303,
    
    /// <summary>Close NPC dialog. Direction: Client→Server.</summary>
    /// <remarks>End conversation.</remarks>
    NpcDialogClose = 1304,
    
    /// <summary>Request NPC gossip menu. Direction: Client→Server.</summary>
    /// <remarks>Get available interaction options. Response: NpcGossipResponse (1306).</remarks>
    NpcGossipRequest = 1305,
    
    /// <summary>NPC gossip menu options. Direction: Server→Client.</summary>
    /// <remarks>List of available actions: Talk, Buy, Sell, Train, etc.</remarks>
    NpcGossipResponse = 1306,
    
    /// <summary>Open vendor window. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access NPC shop. Triggers VendorListRequest automatically.</remarks>
    VendorOpen = 1310,
    
    /// <summary>Close vendor window. Direction: Client→Server.</summary>
    /// <remarks>Exit shop interface.</remarks>
    VendorClose = 1311,
    
    /// <summary>Request vendor inventory. Direction: Client→Server.</summary>
    /// <remarks>Get items for sale. Response: VendorListResponse (1313).</remarks>
    VendorListRequest = 1312,
    
    /// <summary>Vendor inventory list. Direction: Server→Client.</summary>
    /// <remarks>Items with prices, stock limits, required reputation.</remarks>
    VendorListResponse = 1313,
    
    /// <summary>Buy item from vendor. Direction: Client→Server.</summary>
    /// <remarks>Purchase item. Response: VendorBuyResult (1315) with success or error.</remarks>
    VendorBuy = 1314,
    
    /// <summary>Vendor buy result. Direction: Server→Client.</summary>
    /// <remarks>Success adds item. Failure: Insufficient gold, inventory full, out of stock.</remarks>
    VendorBuyResult = 1315,
    
    /// <summary>Sell item to vendor. Direction: Client→Server.</summary>
    /// <remarks>Sell inventory item. Response: VendorSellResult (1317) with gold earned.</remarks>
    VendorSell = 1316,
    
    /// <summary>Vendor sell result. Direction: Server→Client.</summary>
    /// <remarks>Gold added. Item removed. Some items cannot be sold.</remarks>
    VendorSellResult = 1317,
    
    /// <summary>Buyback recently sold item. Direction: Client→Server.</summary>
    /// <remarks>Repurchase from vendor buyback tab. Response: VendorBuybackResult (1319).</remarks>
    VendorBuyback = 1318,
    
    /// <summary>Vendor buyback result. Direction: Server→Client.</summary>
    /// <remarks>Item restored for original sell price.</remarks>
    VendorBuybackResult = 1319,
    
    /// <summary>Repair single item. Direction: Client→Server.</summary>
    /// <remarks>Pay vendor to restore durability. Response: VendorRepairResult (1322).</remarks>
    VendorRepair = 1320,
    
    /// <summary>Repair all equipped items. Direction: Client→Server.</summary>
    /// <remarks>Convenience repair. Total cost calculated. Response: VendorRepairResult (1322).</remarks>
    VendorRepairAll = 1321,
    
    /// <summary>Vendor repair result. Direction: Server→Client.</summary>
    /// <remarks>Durability restored. Gold deducted. Or error: Insufficient gold.</remarks>
    VendorRepairResult = 1322,
    
    /// <summary>Open trainer window. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Skill/spell trainer access.</remarks>
    TrainerOpen = 1330,
    
    /// <summary>Close trainer window. Direction: Client→Server.</summary>
    TrainerClose = 1331,
    
    /// <summary>Request trainer skills. Direction: Client→Server.</summary>
    /// <remarks>Available skills/spells. Response: TrainerListResponse (1333).</remarks>
    TrainerListRequest = 1332,
    
    /// <summary>Trainer skill list. Direction: Server→Client.</summary>
    /// <remarks>Skills with: Cost, level requirement, already known status.</remarks>
    TrainerListResponse = 1333,
    
    /// <summary>Learn skill from trainer. Direction: Client→Server.</summary>
    /// <remarks>Pay gold to learn. Response: TrainerLearnResult (1335).</remarks>
    TrainerLearn = 1334,
    
    /// <summary>Trainer learn result. Direction: Server→Client.</summary>
    /// <remarks>Skill learned or error: Insufficient gold, level too low, already known.</remarks>
    TrainerLearnResult = 1335,
    
    /// <summary>Bind to inn/home location. Direction: Client→Server.</summary>
    /// <remarks>Set hearthstone return point. Response: InnkeeperBindResult (1341).</remarks>
    InnkeeperBind = 1340,
    
    /// <summary>Inn bind result. Direction: Server→Client.</summary>
    /// <remarks>Home location updated.</remarks>
    InnkeeperBindResult = 1341,
    
    /// <summary>Open flight master. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Fast travel interface.</remarks>
    FlightmasterOpen = 1342,
    
    /// <summary>Flight path list. Direction: Server→Client.</summary>
    /// <remarks>Available destinations, costs, travel times, discovered status.</remarks>
    FlightmasterList = 1343,
    
    /// <summary>Open banker. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access personal bank storage.</remarks>
    BankerOpen = 1344,
    
    /// <summary>Open auctioneer. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access auction house.</remarks>
    AuctioneerOpen = 1345,
    
    /// <summary>Open mailbox. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access mail system.</remarks>
    MailboxOpen = 1346,
    
    /// <summary>Open stablemaster. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Manage pet/mount storage.</remarks>
    StablemasterOpen = 1347,
    
    /// <summary>Open barber. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Character appearance modification.</remarks>
    BarberOpen = 1348,
    
    /// <summary>Open transmogrifier. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Item appearance modification interface.</remarks>
    TransmogOpen = 1349,

    // ═══════════════════════════════════════════════════════════════
    // ENTITY SPAWNING / SYNC (1400-1499)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Spawn single entity. Direction: Server→Client.</summary>
    /// <remarks>New NPC/player visible. Contains: ID, position, type, appearance.</remarks>
    EntitySpawn = 1400,
    
    /// <summary>Batch spawn multiple entities. Direction: Server→Client.</summary>
    /// <remarks>Efficient zone loading. Array of entities. Used on zone entry.</remarks>
    EntitySpawnBatch = 1401,
    
    /// <summary>Despawn single entity. Direction: Server→Client.</summary>
    /// <remarks>Entity left zone or died. Client removes from world.</remarks>
    EntityDespawn = 1402,
    
    /// <summary>Batch despawn multiple entities. Direction: Server→Client.</summary>
    /// <remarks>Efficient cleanup. Array of entity IDs.</remarks>
    EntityDespawnBatch = 1403,
    
    /// <summary>Entity state update. Direction: Server→Client.</summary>
    /// <remarks>Position, health, or other property changed. Incremental update.</remarks>
    EntityUpdate = 1404,
    
    /// <summary>Batch entity updates. Direction: Server→Client.</summary>
    /// <remarks>Multiple entity states. Bandwidth optimization for crowded areas.</remarks>
    EntityUpdateBatch = 1405,
    
    /// <summary>Request entity list. Direction: Client→Server.</summary>
    /// <remarks>Get all entities in area. Response: EntityListResponse (1407).</remarks>
    EntityListRequest = 1406,
    
    /// <summary>Entity list data. Direction: Server→Client.</summary>
    /// <remarks>Complete list of nearby entities. For UI purposes.</remarks>
    EntityListResponse = 1407,
    
    /// <summary>Entity path update. Direction: Server→Client.</summary>
    /// <remarks>NPC movement path. For smooth client-side movement prediction.</remarks>
    EntityPathUpdate = 1408,
    
    /// <summary>Entity state changed. Direction: Server→Client.</summary>
    /// <remarks>Combat state, sitting, swimming, flying. Animation state changes.</remarks>
    EntityStateChange = 1409,
    
    /// <summary>Entity animation trigger. Direction: Server→Client.</summary>
    /// <remarks>Play animation: Attack, cast, emote. Single entity.</remarks>
    EntityAnimation = 1410,
    
    /// <summary>Batch entity animations. Direction: Server→Client.</summary>
    /// <remarks>Multiple animations. Combat scenarios.</remarks>
    EntityAnimationBatch = 1411,
    
    /// <summary>Entity nameplate data. Direction: Server→Client.</summary>
    /// <remarks>Name, guild, title, level for UI nameplate.</remarks>
    EntityNameplate = 1412,
    
    /// <summary>Nameplate updated. Direction: Server→Client.</summary>
    /// <remarks>Health bar, buffs, name changes reflected in nameplate.</remarks>
    EntityNameplateUpdate = 1413,
    
    /// <summary>Entity faction change. Direction: Server→Client.</summary>
    /// <remarks>Hostility updated. Affects targeting and combat availability.</remarks>
    EntityFaction = 1414,
    
    /// <summary>Entity scale change. Direction: Server→Client.</summary>
    /// <remarks>Size modification from buffs/abilities. Visual update.</remarks>
    EntityScale = 1415,
    
    /// <summary>Entity mount state. Direction: Server→Client.</summary>
    /// <remarks>Mounted or dismounted. Mount model displayed.</remarks>
    EntityMountUpdate = 1416,
    
    /// <summary>Entity equipment changed. Direction: Server→Client.</summary>
    /// <remarks>Visible gear updated. For character appearance.</remarks>
    EntityEquipmentUpdate = 1417,
    
    /// <summary>Entity aura/buff visuals. Direction: Server→Client.</summary>
    /// <remarks>Buff effects displayed on entity. Particle effects.</remarks>
    EntityAuraUpdate = 1418,
    
    /// <summary>Entity performed emote. Direction: Server→Client.</summary>
    /// <remarks>Gesture/animation from emote command.</remarks>
    EntityEmote = 1419,
    
    /// <summary>Entity local chat. Direction: Server→Client.</summary>
    /// <remarks>Speech bubble text. Nearby only.</remarks>
    EntitySay = 1420,
    
    /// <summary>Entity yell. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Loud speech. Wider radius.</remarks>
    EntityYell = 1421,
    
    /// <summary>Lootable corpse/chest spawned. Direction: Server→Client.</summary>
    /// <remarks>Interactable loot object. Contains loot table.</remarks>
    LootableSpawn = 1430,
    
    /// <summary>Lootable removed. Direction: Server→Client.</summary>
    /// <remarks>Looted or despawned.</remarks>
    LootableDespawn = 1431,
    
    /// <summary>Resource node spawned. Direction: Server→Client.</summary>
    /// <remarks>Mining/herb/skinning node. Profession resource.</remarks>
    ResourceNodeSpawn = 1432,
    
    /// <summary>Resource node removed. Direction: Server→Client.</summary>
    /// <remarks>Harvested or respawn timer.</remarks>
    ResourceNodeDespawn = 1433,
    
    /// <summary>Resource node state. Direction: Server→Client.</summary>
    /// <remarks>Available, being harvested, depleted.</remarks>
    ResourceNodeState = 1434,

    // ═══════════════════════════════════════════════════════════════
    // BUFFS / DEBUFFS / AURAS (1500-1599)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Buff applied to entity. Direction: Server→Client.</summary>
    /// <remarks>Beneficial effect. Contains: Buff ID, duration, stacks, caster.</remarks>
    BuffApplied = 1500,
    
    /// <summary>Buff removed from entity. Direction: Server→Client.</summary>
    /// <remarks>Expired, dispelled, or cancelled. Updates UI buff bar.</remarks>
    BuffRemoved = 1501,
    
    /// <summary>Buff refreshed/reapplied. Direction: Server→Client.</summary>
    /// <remarks>Duration reset. Stacks may increase.</remarks>
    BuffRefreshed = 1502,
    
    /// <summary>Buff stack count changed. Direction: Server→Client.</summary>
    /// <remarks>Stackable buff increased or decreased. Updates tooltip.</remarks>
    BuffStackUpdate = 1503,
    
    /// <summary>Debuff applied to entity. Direction: Server→Client.</summary>
    /// <remarks>Harmful effect. Contains: Debuff ID, duration, type (poison/disease/curse).</remarks>
    DebuffApplied = 1504,
    
    /// <summary>Debuff removed from entity. Direction: Server→Client.</summary>
    /// <remarks>Expired, cleansed, or resisted.</remarks>
    DebuffRemoved = 1505,
    
    /// <summary>Full aura list sync. Direction: Server→Client.</summary>
    /// <remarks>Complete list of active buffs/debuffs. On zone in or significant change.</remarks>
    AuraListSync = 1506,
    
    /// <summary>Aura data updated. Direction: Server→Client.</summary>
    /// <remarks>Duration changed, effect modified. Incremental update.</remarks>
    AuraUpdate = 1507,
    
    /// <summary>Request dispel/cleanse. Direction: Client→Server.</summary>
    /// <remarks>Remove debuff from target. Response: DispelResult (1509).</remarks>
    DispelRequest = 1508,
    
    /// <summary>Dispel result. Direction: Server→Client.</summary>
    /// <remarks>Success removes debuff. Failure: Immune, wrong type, out of range.</remarks>
    DispelResult = 1509,
    
    /// <summary>Steal buff from enemy. Direction: Client→Server.</summary>
    /// <remarks>Spellsteal mechanic. Response: StealResult (1511).</remarks>
    StealRequest = 1510,
    
    /// <summary>Buff steal result. Direction: Server→Client.</summary>
    /// <remarks>Buff transferred to caster or steal resisted.</remarks>
    StealResult = 1511,
    
    /// <summary>Purge buffs from enemy. Direction: Client→Server.</summary>
    /// <remarks>Remove beneficial effects. Response: PurgeResult (1513).</remarks>
    PurgeRequest = 1512,
    
    /// <summary>Purge result. Direction: Server→Client.</summary>
    /// <remarks>Buffs removed or purge failed.</remarks>
    PurgeResult = 1513,
    
    /// <summary>Target immune to aura. Direction: Server→Client.</summary>
    /// <remarks>Immunity prevented application. Boss mechanics, racial traits.</remarks>
    AuraImmune = 1514,
    
    /// <summary>Aura resisted. Direction: Server→Client.</summary>
    /// <remarks>Target resisted effect. Partial or full resistance.</remarks>
    AuraResist = 1515,
    
    /// <summary>Buff category update. Direction: Server→Client.</summary>
    /// <remarks>Group of related buffs changed. Batch update.</remarks>
    BuffCategoryUpdate = 1516,

    // ═══════════════════════════════════════════════════════════════
    // CRAFTING / PROFESSIONS (1600-1699)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Open crafting interface. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access profession crafting window.</remarks>
    CraftingOpen = 1600,
    
    /// <summary>Close crafting interface. Direction: Client→Server.</summary>
    CraftingClose = 1601,
    
    /// <summary>Request recipe list. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Known recipes for profession. Filtered by category.</remarks>
    CraftingRecipeList = 1602,
    
    /// <summary>Start crafting item. Direction: Client→Server.</summary>
    /// <remarks>Begin craft. Validates materials. Progress updates via CraftingProgress (1604).</remarks>
    CraftingStart = 1603,
    
    /// <summary>Crafting progress update. Direction: Server→Client.</summary>
    /// <remarks>Percentage complete. For progress bar. May have multiple steps.</remarks>
    CraftingProgress = 1604,
    
    /// <summary>Crafting completed. Direction: Server→Client.</summary>
    /// <remarks>Item created. Materials consumed. May proc quality bonus.</remarks>
    CraftingComplete = 1605,
    
    /// <summary>Crafting failed. Direction: Server→Client.</summary>
    /// <remarks>Interrupted, insufficient materials, or critical failure. Materials may be lost.</remarks>
    CraftingFailed = 1606,
    
    /// <summary>Cancel crafting. Direction: Client→Server.</summary>
    /// <remarks>Abort in-progress craft. Materials returned.</remarks>
    CraftingCancel = 1607,
    
    /// <summary>Crafting queue status. Direction: Server→Client.</summary>
    /// <remarks>Queued crafts. For batch production.</remarks>
    CraftingQueue = 1608,
    
    /// <summary>Add to craft queue. Direction: Client→Server.</summary>
    /// <remarks>Queue multiple items. Auto-craft sequentially.</remarks>
    CraftingQueueAdd = 1609,
    
    /// <summary>Remove from craft queue. Direction: Client→Server.</summary>
    /// <remarks>Cancel queued item.</remarks>
    CraftingQueueRemove = 1610,
    
    /// <summary>Learn new recipe. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>From trainer, quest reward, or world drop.</remarks>
    RecipeLearn = 1611,
    
    /// <summary>Forget/unlearn recipe. Direction: Client→Server.</summary>
    /// <remarks>Remove recipe knowledge. Rare use case.</remarks>
    RecipeUnlearn = 1612,
    
    /// <summary>Discover new recipe. Direction: Server→Client.</summary>
    /// <remarks>Random discovery from crafting. Research or experimentation.</remarks>
    RecipeDiscovery = 1613,
    
    /// <summary>Profession info. Direction: Server→Client.</summary>
    /// <remarks>Current skill level, max level, experience to next level.</remarks>
    ProfessionInfo = 1620,
    
    /// <summary>Profession leveled up. Direction: Server→Client.</summary>
    /// <remarks>Skill tier increased. New recipes unlocked.</remarks>
    ProfessionLevelUp = 1621,
    
    /// <summary>Profession skill gain. Direction: Server→Client.</summary>
    /// <remarks>Skill points from crafting. Progress toward next level.</remarks>
    ProfessionSkillUp = 1622,
    
    /// <summary>Start gathering resource. Direction: Client→Server.</summary>
    /// <remarks>Mine/herb/skin. Begin gather cast.</remarks>
    GatheringStart = 1630,
    
    /// <summary>Gathering progress. Direction: Server→Client.</summary>
    /// <remarks>Cast bar progress. Interruptible.</remarks>
    GatheringProgress = 1631,
    
    /// <summary>Gathering completed. Direction: Server→Client.</summary>
    /// <remarks>Resources added to inventory. Node may despawn.</remarks>
    GatheringComplete = 1632,
    
    /// <summary>Gathering failed. Direction: Server→Client.</summary>
    /// <remarks>Interrupted, too low skill, or node depleted.</remarks>
    GatheringFailed = 1633,
    
    /// <summary>Gathering interrupted. Direction: Server→Client.</summary>
    /// <remarks>Damage taken, movement, or player cancelled.</remarks>
    GatheringInterrupt = 1634,

    // ═══════════════════════════════════════════════════════════════
    // AUCTION HOUSE / MARKET (1700-1799)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Open auction house. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Access player market interface.</remarks>
    AuctionOpen = 1700,
    
    /// <summary>Close auction house. Direction: Client→Server.</summary>
    AuctionClose = 1701,
    
    /// <summary>Search auction listings. Direction: Client→Server.</summary>
    /// <remarks>Filter by: Item name, category, level, quality. Response: AuctionSearchResults (1703).</remarks>
    AuctionSearch = 1702,
    
    /// <summary>Auction search results. Direction: Server→Client.</summary>
    /// <remarks>Matching listings with: Item, seller, price, time remaining.</remarks>
    AuctionSearchResults = 1703,
    
    /// <summary>Create new auction. Direction: Client→Server.</summary>
    /// <remarks>List item for sale. Set: Buyout, starting bid, duration. Response: AuctionCreateResult (1705).</remarks>
    AuctionCreate = 1704,
    
    /// <summary>Auction creation result. Direction: Server→Client.</summary>
    /// <remarks>Success or error: Deposit required, auction house full, item cannot be sold.</remarks>
    AuctionCreateResult = 1705,
    
    /// <summary>Bid on auction. Direction: Client→Server.</summary>
    /// <remarks>Place bid. Must exceed current bid. Response: AuctionBidResult (1707).</remarks>
    AuctionBid = 1706,
    
    /// <summary>Bid result. Direction: Server→Client.</summary>
    /// <remarks>Bid accepted or error: Outbid, insufficient gold, auction ended.</remarks>
    AuctionBidResult = 1707,
    
    /// <summary>Buyout auction. Direction: Client→Server.</summary>
    /// <remarks>Instant purchase. Response: AuctionBuyoutResult (1709).</remarks>
    AuctionBuyout = 1708,
    
    /// <summary>Buyout result. Direction: Server→Client.</summary>
    /// <remarks>Item purchased or error: Insufficient gold, already sold.</remarks>
    AuctionBuyoutResult = 1709,
    
    /// <summary>Cancel own auction. Direction: Client→Server.</summary>
    /// <remarks>Remove listing early. Response: AuctionCancelResult (1711). May forfeit deposit.</remarks>
    AuctionCancel = 1710,
    
    /// <summary>Auction cancel result. Direction: Server→Client.</summary>
    /// <remarks>Auction cancelled. Item returned to seller.</remarks>
    AuctionCancelResult = 1711,
    
    /// <summary>Auction expired unsold. Direction: Server→Client.</summary>
    /// <remarks>Time ran out. No bids. Item returned to seller via mail.</remarks>
    AuctionExpired = 1712,
    
    /// <summary>Auction sold. Direction: Server→Client.</summary>
    /// <remarks>Item sold. Gold sent to seller via mail.</remarks>
    AuctionSold = 1713,
    
    /// <summary>Player was outbid. Direction: Server→Client.</summary>
    /// <remarks>Notification of higher bid. Bid gold returned.</remarks>
    AuctionOutbid = 1714,
    
    /// <summary>Won auction. Direction: Server→Client.</summary>
    /// <remarks>Auction ended. Player had highest bid. Item sent via mail.</remarks>
    AuctionWon = 1715,
    
    /// <summary>List owned auctions. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Player's active listings with current bid status.</remarks>
    AuctionListOwned = 1716,
    
    /// <summary>List active bids. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Auctions player has bid on. Shows if winning or outbid.</remarks>
    AuctionListBids = 1717,
    
    /// <summary>Item price history. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Historical pricing data for market trends.</remarks>
    AuctionPriceHistory = 1718,
    
    /// <summary>Mark auction as favorite. Direction: Client→Server.</summary>
    /// <remarks>Watch specific auction for quick access.</remarks>
    AuctionFavorite = 1719,
    
    /// <summary>Favorite auctions list. Direction: Server→Client.</summary>
    /// <remarks>Saved auction searches or items.</remarks>
    AuctionFavoriteList = 1720,

    // ═══════════════════════════════════════════════════════════════
    // MAIL SYSTEM (1800-1899)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Request mail inbox. Direction: Client→Server.</summary>
    /// <remarks>Get list of received mail. Response: MailInboxResponse (1801).</remarks>
    MailInboxRequest = 1800,
    
    /// <summary>Mail inbox data. Direction: Server→Client.</summary>
    /// <remarks>List of mail with: Sender, subject, has attachments, read status, time sent.</remarks>
    MailInboxResponse = 1801,
    
    /// <summary>Send mail. Direction: Client→Server.</summary>
    /// <remarks>Send letter with optional gold/items. Response: MailSendResult (1803).</remarks>
    MailSend = 1802,
    
    /// <summary>Mail send result. Direction: Server→Client.</summary>
    /// <remarks>Success or error: Recipient not found, insufficient postage, mailbox full.</remarks>
    MailSendResult = 1803,
    
    /// <summary>Read mail message. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Open mail. Displays body text and attachments.</remarks>
    MailRead = 1804,
    
    /// <summary>Mark mail as read. Direction: Client→Server.</summary>
    /// <remarks>Updates read status. For UI organization.</remarks>
    MailMarkRead = 1805,
    
    /// <summary>Take item attachment. Direction: Client→Server.</summary>
    /// <remarks>Remove item from mail to inventory. Response: MailTakeAttachmentResult (1807).</remarks>
    MailTakeAttachment = 1806,
    
    /// <summary>Take attachment result. Direction: Server→Client.</summary>
    /// <remarks>Item added to inventory or error: Inventory full.</remarks>
    MailTakeAttachmentResult = 1807,
    
    /// <summary>Take gold from mail. Direction: Client→Server.</summary>
    /// <remarks>Collect money. Response: MailTakeGoldResult (1809).</remarks>
    MailTakeGold = 1808,
    
    /// <summary>Take gold result. Direction: Server→Client.</summary>
    /// <remarks>Gold added to currency.</remarks>
    MailTakeGoldResult = 1809,
    
    /// <summary>Take all attachments. Direction: Client→Server.</summary>
    /// <remarks>Bulk collection of items and gold from mail.</remarks>
    MailTakeAll = 1810,
    
    /// <summary>Delete mail. Direction: Client→Server.</summary>
    /// <remarks>Remove from inbox. Must have no attachments.</remarks>
    MailDelete = 1811,
    
    /// <summary>Return mail to sender. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Undeliverable or refused. Attachments sent back.</remarks>
    MailReturn = 1812,
    
    /// <summary>New mail notification. Direction: Server→Client.</summary>
    /// <remarks>Alert player of incoming mail. Triggers UI indicator.</remarks>
    MailNotification = 1813,
    
    /// <summary>Cash on delivery mail. Direction: Server→Client.</summary>
    /// <remarks>Requires payment to receive attachments.</remarks>
    MailCashOnDelivery = 1814,
    
    /// <summary>Pay COD and take items. Direction: Client→Server.</summary>
    /// <remarks>Pay required amount to unlock attachments.</remarks>
    MailCashOnDeliveryPay = 1815,
    
    /// <summary>Mail expired. Direction: Server→Client.</summary>
    /// <remarks>Unread mail auto-deleted after 30 days. Attachments returned to sender.</remarks>
    MailExpired = 1816,

    // ═══════════════════════════════════════════════════════════════
    // ACHIEVEMENTS / TITLES (1900-1999)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Achievement completed. Direction: Server→Client.</summary>
    /// <remarks>Achievement earned. Triggers toast notification, rewards, points.</remarks>
    AchievementUnlocked = 1900,
    
    /// <summary>Achievement progress update. Direction: Server→Client.</summary>
    /// <remarks>Criteria partially completed. Updates achievement tracker.</remarks>
    AchievementProgress = 1901,
    
    /// <summary>Request achievement list. Direction: Client→Server.</summary>
    /// <remarks>Get all achievements. Response: AchievementListResponse (1903).</remarks>
    AchievementListRequest = 1902,
    
    /// <summary>Achievement list data. Direction: Server→Client.</summary>
    /// <remarks>All achievements with completion status and progress.</remarks>
    AchievementListResponse = 1903,
    
    /// <summary>Achievement criteria updated. Direction: Server→Client.</summary>
    /// <remarks>Specific criterion progress: Kill count, exploration, etc.</remarks>
    AchievementCriteriaUpdate = 1904,
    
    /// <summary>Achievement points total. Direction: Server→Client.</summary>
    /// <remarks>Total points from completed achievements. For bragging rights.</remarks>
    AchievementPointsUpdate = 1905,
    
    /// <summary>Achievement toast notification. Direction: Server→Client.</summary>
    /// <remarks>Popup achievement completion announcement.</remarks>
    AchievementToast = 1906,
    
    /// <summary>Share achievement link. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Post achievement in chat.</remarks>
    AchievementLink = 1907,
    
    /// <summary>Compare achievements with player. Direction: Client→Server.</summary>
    /// <remarks>Inspect another player's achievement completion. Response: AchievementCompareResult (1909).</remarks>
    AchievementCompare = 1908,
    
    /// <summary>Achievement comparison data. Direction: Server→Client.</summary>
    /// <remarks>Side-by-side achievement completion between players.</remarks>
    AchievementCompareResult = 1909,
    
    /// <summary>Title unlocked. Direction: Server→Client.</summary>
    /// <remarks>New title available from achievement/reputation.</remarks>
    TitleUnlock = 1920,
    
    /// <summary>Select active title. Direction: Client→Server.</summary>
    /// <remarks>Set displayed title.</remarks>
    TitleSelectMsg = 1921,
    
    /// <summary>Clear active title. Direction: Client→Server.</summary>
    /// <remarks>Remove displayed title.</remarks>
    TitleClear = 1922,
    
    /// <summary>Request title list. Direction: Client→Server.</summary>
    /// <remarks>All unlocked titles. Response: TitleListResponse (1924).</remarks>
    TitleListRequest = 1923,
    
    /// <summary>Title list data. Direction: Server→Client.</summary>
    /// <remarks>Available titles with unlock status.</remarks>
    TitleListResponse = 1924,

    // ═══════════════════════════════════════════════════════════════
    // MOUNTS / PETS / COMPANIONS (2000-2099)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Summon mount. Direction: Client→Server.</summary>
    /// <remarks>Ride mount. Increases movement speed. Response: MountSummonResult (2001).</remarks>
    MountSummon = 2000,
    
    /// <summary>Mount summon result. Direction: Server→Client.</summary>
    /// <remarks>Mounted or error: In combat, indoors, wrong zone type.</remarks>
    MountSummonResult = 2001,
    
    /// <summary>Dismount. Direction: Client→Server.</summary>
    /// <remarks>Leave mount. Return to normal movement speed.</remarks>
    MountDismount = 2002,
    
    /// <summary>Request mount collection. Direction: Client→Server.</summary>
    /// <remarks>All owned mounts. Response: MountListResponse (2004).</remarks>
    MountListRequest = 2003,
    
    /// <summary>Mount collection data. Direction: Server→Client.</summary>
    /// <remarks>Mounts with: Unlock status, speed, type (ground/flying).</remarks>
    MountListResponse = 2004,
    
    /// <summary>Mark mount as favorite. Direction: Client→Server.</summary>
    /// <remarks>Add to favorite list for random mount.</remarks>
    MountFavorite = 2005,
    
    /// <summary>Remove mount from favorites. Direction: Client→Server.</summary>
    MountUnfavorite = 2006,
    
    /// <summary>Summon random favorite mount. Direction: Client→Server.</summary>
    /// <remarks>Picks random from favorites. Convenience feature.</remarks>
    MountRandomFavorite = 2007,
    
    /// <summary>Summon battle pet. Direction: Client→Server.</summary>
    /// <remarks>Call companion for pet battles. Response: PetSummonResult (2021).</remarks>
    PetSummon = 2020,
    
    /// <summary>Pet summon result. Direction: Server→Client.</summary>
    /// <remarks>Pet active or error: Pet limit, in combat.</remarks>
    PetSummonResult = 2021,
    
    /// <summary>Dismiss pet. Direction: Client→Server.</summary>
    /// <remarks>Remove pet from world.</remarks>
    PetDismiss = 2022,
    
    /// <summary>Rename pet. Direction: Client→Server.</summary>
    /// <remarks>Change pet name. Subject to profanity filter.</remarks>
    PetRename = 2023,
    
    /// <summary>Command pet action. Direction: Client→Server.</summary>
    /// <remarks>Attack, follow, stay commands. Response: PetCommandResult (2025).</remarks>
    PetCommand = 2024,
    
    /// <summary>Pet command result. Direction: Server→Client.</summary>
    /// <remarks>Pet executes command or cannot comply.</remarks>
    PetCommandResult = 2025,
    
    /// <summary>Pet state update. Direction: Server→Client.</summary>
    /// <remarks>Health, happiness, level changes.</remarks>
    PetUpdate = 2026,
    
    /// <summary>Feed pet. Direction: Client→Server.</summary>
    /// <remarks>Restore happiness. Requires food item.</remarks>
    PetFeed = 2027,
    
    /// <summary>Train pet ability. Direction: Client→Server.</summary>
    /// <remarks>Learn new skill. May require training points.</remarks>
    PetTrain = 2028,
    
    /// <summary>Abandon pet permanently. Direction: Client→Server.</summary>
    /// <remarks>Release pet. Cannot be undone.</remarks>
    PetAbandon = 2029,
    
    /// <summary>Store pet in stable. Direction: Client→Server.</summary>
    /// <remarks>Inactive pets. Storage limit applies.</remarks>
    PetStable = 2030,
    
    /// <summary>Retrieve pet from stable. Direction: Client→Server.</summary>
    /// <remarks>Make pet active again.</remarks>
    PetUnstable = 2031,
    
    /// <summary>Request pet list. Direction: Client→Server.</summary>
    /// <remarks>All owned pets. Response: PetListResponse (2033).</remarks>
    PetListRequest = 2032,
    
    /// <summary>Pet collection data. Direction: Server→Client.</summary>
    /// <remarks>Active and stabled pets with stats.</remarks>
    PetListResponse = 2033,
    
    /// <summary>Summon vanity companion. Direction: Client→Server.</summary>
    /// <remarks>Cosmetic pet. Follows player.</remarks>
    CompanionSummon = 2050,
    
    /// <summary>Dismiss vanity companion. Direction: Client→Server.</summary>
    CompanionDismiss = 2051,
    
    /// <summary>Interact with companion. Direction: Client→Server.</summary>
    /// <remarks>Pet tricks or emotes.</remarks>
    CompanionInteract = 2052,
    
    /// <summary>Request companion list. Direction: Client→Server.</summary>
    /// <remarks>Response: CompanionListResponse (2054).</remarks>
    CompanionListRequest = 2053,
    
    /// <summary>Companion collection data. Direction: Server→Client.</summary>
    CompanionListResponse = 2054,

    // ═══════════════════════════════════════════════════════════════
    // SOCIAL (FRIENDS, BLOCK) (2100-2199)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Send friend request to player. Direction: Client→Server.</summary>
    /// <remarks>Request friendship. Response: FriendRequestResult (2101).</remarks>
    FriendRequest = 2100,
    
    /// <summary>Friend request outcome. Direction: Server→Client.</summary>
    /// <remarks>Pending, accepted, declined, or error: Player not found, already friends.</remarks>
    FriendRequestResult = 2101,
    
    /// <summary>Accept friend request. Direction: Client→Server.</summary>
    /// <remarks>Confirm friendship. Both players added to friend lists.</remarks>
    FriendAccept = 2102,
    
    /// <summary>Decline friend request. Direction: Client→Server.</summary>
    /// <remarks>Reject friendship offer.</remarks>
    FriendDecline = 2103,
    
    /// <summary>Remove friend. Direction: Client→Server.</summary>
    /// <remarks>Delete from friend list. One-sided removal.</remarks>
    FriendRemove = 2104,
    
    /// <summary>Request friend list. Direction: Client→Server.</summary>
    /// <remarks>Get all friends. Response: FriendListResponse (2106).</remarks>
    FriendListRequest = 2105,
    
    /// <summary>Friend list data. Direction: Server→Client.</summary>
    /// <remarks>All friends with: Name, online status, zone/level, note.</remarks>
    FriendListResponse = 2106,
    
    /// <summary>Friend came online. Direction: Server→Client.</summary>
    /// <remarks>Notification when friend logs in.</remarks>
    FriendOnline = 2107,
    
    /// <summary>Friend went offline. Direction: Server→Client.</summary>
    /// <remarks>Notification when friend logs out.</remarks>
    FriendOffline = 2108,
    
    /// <summary>Friend info updated. Direction: Server→Client.</summary>
    /// <remarks>Level, zone, or status changed.</remarks>
    FriendUpdate = 2109,
    
    /// <summary>Set friend note. Direction: Client→Server.</summary>
    /// <remarks>Personal note about friend. Private text field.</remarks>
    FriendNote = 2110,
    
    /// <summary>Block player communications. Direction: Client→Server.</summary>
    /// <remarks>Prevent messages/invites. Response: BlockPlayerResult (2121).</remarks>
    BlockPlayer = 2120,
    
    /// <summary>Block player result. Direction: Server→Client.</summary>
    /// <remarks>Player blocked or error: Already blocked, cannot block self.</remarks>
    BlockPlayerResult = 2121,
    
    /// <summary>Unblock player. Direction: Client→Server.</summary>
    /// <remarks>Remove from block list. Allow communication again.</remarks>
    UnblockPlayer = 2122,
    
    /// <summary>Request block list. Direction: Client→Server.</summary>
    /// <remarks>Get all blocked players. Response: BlockListResponse (2124).</remarks>
    BlockListRequest = 2123,
    
    /// <summary>Block list data. Direction: Server→Client.</summary>
    /// <remarks>All blocked players with timestamps.</remarks>
    BlockListResponse = 2124,
    
    /// <summary>Ignore player (temp block). Direction: Client→Server.</summary>
    /// <remarks>Session-only block. Resets on logout.</remarks>
    IgnorePlayer = 2125,
    
    /// <summary>Unignore player. Direction: Client→Server.</summary>
    /// <remarks>Remove from temporary ignore.</remarks>
    UnignorePlayer = 2126,
    
    /// <summary>Who query for players. Direction: Client→Server.</summary>
    /// <remarks>Search online players by: Name, zone, level, class. Response: WhoResponse (2131).</remarks>
    WhoRequest = 2130,
    
    /// <summary>Who query results. Direction: Server→Client.</summary>
    /// <remarks>List of matching players with basic info.</remarks>
    WhoResponse = 2131,
    
    /// <summary>Player location query. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Find friend's current zone/position.</remarks>
    PlayerLocation = 2132,

    // ═══════════════════════════════════════════════════════════════
    // EMOTES / ANIMATIONS / COSMETICS (2200-2299)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Request to perform emote. Direction: Client→Server.</summary>
    /// <remarks>Trigger animation and text. Response: EmoteBroadcast (2201) to nearby players.</remarks>
    EmoteRequest = 2200,
    
    /// <summary>Broadcast emote to area. Direction: Server→Client (Broadcast).</summary>
    /// <remarks>Player performed emote. Nearby clients play animation.</remarks>
    EmoteBroadcast = 2201,
    
    /// <summary>Targeted emote. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Emote directed at specific player. Includes target in text.</remarks>
    EmoteTargeted = 2202,
    
    /// <summary>Trigger custom animation. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Play specific animation. From abilities or cinematics.</remarks>
    AnimationTrigger = 2203,
    
    /// <summary>Cancel ongoing animation. Direction: Client→Server.</summary>
    /// <remarks>Interrupt emote/animation.</remarks>
    AnimationCancel = 2204,
    
    /// <summary>Start dancing. Direction: Client→Server.</summary>
    /// <remarks>Continuous dance animation. Loops until stopped.</remarks>
    DanceStart = 2210,
    
    /// <summary>Stop dancing. Direction: Client→Server.</summary>
    /// <remarks>End dance animation.</remarks>
    DanceStop = 2211,
    
    /// <summary>Sit down. Direction: Client→Server.</summary>
    /// <remarks>Character sits. May trigger resting/regeneration.</remarks>
    SitRequest = 2212,
    
    /// <summary>Stand up. Direction: Client→Server.</summary>
    /// <remarks>Character stands. Exit sitting animation.</remarks>
    StandRequest = 2213,
    
    /// <summary>Sleep (lie down). Direction: Client→Server.</summary>
    /// <remarks>Sleeping animation. Roleplay action.</remarks>
    SleepRequest = 2214,
    
    /// <summary>Kneel. Direction: Client→Server.</summary>
    /// <remarks>Kneeling pose. Often used for roleplay/respect.</remarks>
    KneelRequest = 2215,
    
    /// <summary>Equip cosmetic item. Direction: Client→Server.</summary>
    /// <remarks>Wear appearance item. No stat impact.</remarks>
    CosmeticEquip = 2230,
    
    /// <summary>Unequip cosmetic item. Direction: Client→Server.</summary>
    /// <remarks>Remove appearance item.</remarks>
    CosmeticUnequip = 2231,
    
    /// <summary>Preview cosmetic. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Try appearance before purchase/applying.</remarks>
    CosmeticPreview = 2232,
    
    /// <summary>Apply transmog appearance. Direction: Client→Server.</summary>
    /// <remarks>Change item look. Costs transmog currency.</remarks>
    TransmogApply = 2233,
    
    /// <summary>Remove transmog. Direction: Client→Server.</summary>
    /// <remarks>Restore original item appearance.</remarks>
    TransmogRemove = 2234,
    
    /// <summary>Save transmog set. Direction: Client→Server.</summary>
    /// <remarks>Store appearance loadout for quick switching.</remarks>
    TransmogSave = 2235,
    
    /// <summary>Load transmog set. Direction: Client→Server.</summary>
    /// <remarks>Apply saved appearance loadout.</remarks>
    TransmogLoad = 2236,
    
    /// <summary>Use toy item. Direction: Client→Server.</summary>
    /// <remarks>Activate fun item effect. Cosmetic actions.</remarks>
    ToyUse = 2240,
    
    /// <summary>Request toy collection. Direction: Client→Server.</summary>
    /// <remarks>Get all owned toys. Response: ToyListResponse (2242).</remarks>
    ToyListRequest = 2241,
    
    /// <summary>Toy collection data. Direction: Server→Client.</summary>
    /// <remarks>Unlocked toys with cooldowns and effects.</remarks>
    ToyListResponse = 2242,

    // ═══════════════════════════════════════════════════════════════
    // ADMIN / GM TOOLS (2300-2399)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Execute admin command. Direction: Client→Server.</summary>
    /// <remarks>GM command with parameters. Response: AdminCommandResult (2301). Logged for audit.</remarks>
    AdminCommand = 2300,
    
    /// <summary>Admin command result. Direction: Server→Client.</summary>
    /// <remarks>Success confirmation or error message.</remarks>
    AdminCommandResult = 2301,
    
    /// <summary>Teleport self to location. Direction: Client→Server.</summary>
    /// <remarks>GM instant travel. Coordinates or zone name.</remarks>
    AdminTeleport = 2302,
    
    /// <summary>Teleport player to location. Direction: Client→Server.</summary>
    /// <remarks>Move player forcibly. Logged action.</remarks>
    AdminTeleportPlayer = 2303,
    
    /// <summary>Kick player from server. Direction: Client→Server.</summary>
    /// <remarks>Disconnect player. Optional reason message.</remarks>
    AdminKick = 2304,
    
    /// <summary>Ban player account. Direction: Client→Server.</summary>
    /// <remarks>Prevent login. Duration: Temporary or permanent. Requires reason.</remarks>
    AdminBan = 2305,
    
    /// <summary>Unban player account. Direction: Client→Server.</summary>
    /// <remarks>Lift ban. Restore access.</remarks>
    AdminUnban = 2306,
    
    /// <summary>Mute player chat. Direction: Client→Server.</summary>
    /// <remarks>Prevent chat messages. Duration-based.</remarks>
    AdminMute = 2307,
    
    /// <summary>Unmute player. Direction: Client→Server.</summary>
    /// <remarks>Restore chat privileges.</remarks>
    AdminUnmute = 2308,
    
    /// <summary>Spawn NPC/object. Direction: Client→Server.</summary>
    /// <remarks>Create entity. For testing or events.</remarks>
    AdminSpawn = 2309,
    
    /// <summary>Despawn entity. Direction: Client→Server.</summary>
    /// <remarks>Remove spawned NPC/object.</remarks>
    AdminDespawn = 2310,
    
    /// <summary>Kill player/NPC instantly. Direction: Client→Server.</summary>
    /// <remarks>Set health to 0. Trigger death.</remarks>
    AdminKill = 2311,
    
    /// <summary>Revive dead player. Direction: Client→Server.</summary>
    /// <remarks>Resurrect at current location. Full health/mana.</remarks>
    AdminRevive = 2312,
    
    /// <summary>Heal player fully. Direction: Client→Server.</summary>
    /// <remarks>Restore health and resources to maximum.</remarks>
    AdminHeal = 2313,
    
    /// <summary>Toggle god mode. Direction: Client→Server.</summary>
    /// <remarks>Invulnerability. For testing.</remarks>
    AdminGodMode = 2314,
    
    /// <summary>Toggle invisibility. Direction: Client→Server.</summary>
    /// <remarks>Hide from players and NPCs.</remarks>
    AdminInvisible = 2315,
    
    /// <summary>Freeze player. Direction: Client→Server.</summary>
    /// <remarks>Prevent movement and actions. Discipline tool.</remarks>
    AdminFreeze = 2316,
    
    /// <summary>Unfreeze player. Direction: Client→Server.</summary>
    /// <remarks>Restore movement.</remarks>
    AdminUnfreeze = 2317,
    
    /// <summary>Give item to player. Direction: Client→Server.</summary>
    /// <remarks>Add item directly to inventory. Specify quantity.</remarks>
    AdminGiveItem = 2318,
    
    /// <summary>Remove item from player. Direction: Client→Server.</summary>
    /// <remarks>Delete item from inventory.</remarks>
    AdminRemoveItem = 2319,
    
    /// <summary>Set player level. Direction: Client→Server.</summary>
    /// <remarks>Change character level directly.</remarks>
    AdminSetLevel = 2320,
    
    /// <summary>Modify player stat. Direction: Client→Server.</summary>
    /// <remarks>Change strength, agility, etc. For testing.</remarks>
    AdminSetStat = 2321,
    
    /// <summary>Set faction reputation. Direction: Client→Server.</summary>
    /// <remarks>Override reputation standing.</remarks>
    AdminSetReputation = 2322,
    
    /// <summary>Give gold to player. Direction: Client→Server.</summary>
    /// <remarks>Add currency directly.</remarks>
    AdminAddGold = 2323,
    
    /// <summary>Remove gold from player. Direction: Client→Server.</summary>
    /// <remarks>Deduct currency.</remarks>
    AdminRemoveGold = 2324,
    
    /// <summary>Broadcast announcement. Direction: Client→Server.</summary>
    /// <remarks>Server-wide message. All online players.</remarks>
    AdminAnnounce = 2325,
    
    /// <summary>Whisper as GM. Direction: Client→Server.</summary>
    /// <remarks>Private message with GM tag.</remarks>
    AdminWhisper = 2326,
    
    /// <summary>Summon player to GM. Direction: Client→Server.</summary>
    /// <remarks>Teleport player to admin location.</remarks>
    AdminSummonPlayer = 2327,
    
    /// <summary>Appear at player location. Direction: Client→Server.</summary>
    /// <remarks>Teleport to player for assistance/investigation.</remarks>
    AdminAppearPlayer = 2328,
    
    /// <summary>Request player detailed info. Direction: Client→Server.</summary>
    /// <remarks>Get account, character, session data. For support.</remarks>
    AdminPlayerInfo = 2329,
    
    /// <summary>Request server info. Direction: Client→Server.</summary>
    /// <remarks>Server stats: Population, uptime, performance.</remarks>
    AdminServerInfo = 2330,
    
    /// <summary>Reload configuration. Direction: Client→Server.</summary>
    /// <remarks>Hot-reload config files without restart.</remarks>
    AdminReloadConfig = 2331,
    
    /// <summary>Reload scripts/lua. Direction: Client→Server.</summary>
    /// <remarks>Refresh game scripts for rapid iteration.</remarks>
    AdminReloadScripts = 2332,
    
    /// <summary>Shutdown server gracefully. Direction: Client→Server.</summary>
    /// <remarks>Save state and close connections. Optional delay.</remarks>
    AdminShutdown = 2333,
    
    /// <summary>Restart server. Direction: Client→Server.</summary>
    /// <remarks>Shutdown and auto-restart. For updates.</remarks>
    AdminRestart = 2334,
    
    /// <summary>Enter maintenance mode. Direction: Client→Server.</summary>
    /// <remarks>Prevent new logins. Existing players can finish.</remarks>
    AdminMaintenance = 2335,
    
    /// <summary>View admin action log. Direction: Client→Server or Server→Client.</summary>
    /// <remarks>Audit trail of GM commands for accountability.</remarks>
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
