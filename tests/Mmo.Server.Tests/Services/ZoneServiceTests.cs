using Mmo.Server.Tests.Helpers;
using Mmo.Server.Zones;
using Mmo.Server.Zones.Records;
using Mmo.Server.Zones.Services;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Core;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Zones.Structs;

namespace Mmo.Server.Tests.Services;

[Collection("IdRegistry")]
public class ZoneServiceTests : IDisposable
{
    private readonly MockBroadcastService _broadcastService;
    private readonly MockLog _log = new();
    private readonly ZoneManager _zoneManager;
    private readonly ZoneService _zoneService;

    public ZoneServiceTests()
    {
        IdRegistry.Instance.Clear();

        _zoneManager = new ZoneManager(0);
        _broadcastService = new MockBroadcastService();
        _zoneService = new ZoneService(_zoneManager, _broadcastService, _log);

        // Register test zones
        var zone0 = new Zone(0, "Starter Zone", new ZoneBounds(0, 0, 1000, 1000));
        var zone1 = new Zone(1, "Forest Zone", new ZoneBounds(0, 0, 2000, 2000));
        _zoneManager.RegisterZone(zone0);
        _zoneManager.RegisterZone(zone1);
    }

    public void Dispose() => IdRegistry.Instance.Clear();

    [Fact]
    public void GetZoneInfo_ExistingZone_ReturnsInfo()
    {
        ZoneInfo? zoneInfo = _zoneService.GetZoneInfo(0);

        Assert.NotNull(zoneInfo);
        Assert.Equal((ushort)0, zoneInfo.ZoneId);
        Assert.Equal("Starter Zone", zoneInfo.Name);
    }

    [Fact]
    public void GetZoneInfo_NonExistentZone_ReturnsNull()
    {
        ZoneInfo? zoneInfo = _zoneService.GetZoneInfo(999);

        Assert.Null(zoneInfo);
    }

    [Fact]
    public void ZoneExists_ExistingZone_ReturnsTrue()
    {
        Assert.True(_zoneService.ZoneExists(0));
        Assert.True(_zoneService.ZoneExists(1));
    }

    [Fact]
    public void ZoneExists_NonExistentZone_ReturnsFalse() => Assert.False(_zoneService.ZoneExists(999));

    [Fact]
    public void GetPlayerCount_EmptyZone_ReturnsZero()
    {
        int count = _zoneService.GetPlayerCount(0);

        Assert.Equal(0, count);
    }

    [Fact]
    public void GetPlayerCount_WithPlayers_ReturnsCorrectCount()
    {
        // Add players to zone
        var player1 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player1", new Position(100, 100));
        var player2 = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "Player2", new Position(200, 200));

        player1.SetEntityId(IdRegistry.Instance.GetNextLocalId(0), 0);
        player2.SetEntityId(IdRegistry.Instance.GetNextLocalId(0), 0);

        IdRegistry.Instance.RegisterEntity(player1);
        IdRegistry.Instance.RegisterEntity(player2);

        Zone? zone = _zoneManager.GetZone(0);
        zone?.AddEntity(player1.PersistentId);
        zone?.AddEntity(player2.PersistentId);

        int count = _zoneService.GetPlayerCount(0);

        Assert.Equal(2, count);
    }

    [Fact]
    public void GetPlayersInZone_EmptyZone_ReturnsEmpty()
    {
        var players = _zoneService.GetPlayersInZone(0).ToList();

        Assert.Empty(players);
    }

    [Fact]
    public void GetPlayersInZone_WithPlayers_ReturnsAllPlayerIds()
    {
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player1 = new PlayerEntity(player1Id, Guid.NewGuid(), "Player1", new Position(100, 100));
        var player2 = new PlayerEntity(player2Id, Guid.NewGuid(), "Player2", new Position(200, 200));

        player1.SetEntityId(IdRegistry.Instance.GetNextLocalId(0), 0);
        player2.SetEntityId(IdRegistry.Instance.GetNextLocalId(0), 0);

        IdRegistry.Instance.RegisterEntity(player1);
        IdRegistry.Instance.RegisterEntity(player2);

        Zone? zone = _zoneManager.GetZone(0);
        zone?.AddEntity(player1Id);
        zone?.AddEntity(player2Id);

        var players = _zoneService.GetPlayersInZone(0).ToList();

        Assert.Equal(2, players.Count);
        Assert.Contains(player1Id, players);
        Assert.Contains(player2Id, players);
    }

    [Fact]
    public void RequestZoneTransfer_InvalidTargetZone_ReturnsFailure()
    {
        var playerId = Guid.NewGuid();

        ZoneTransferResult result = _zoneService.RequestZoneTransferAsync(playerId, 999);

        Assert.False(result.Success);
        Assert.Equal("TARGET_ZONE_NOT_FOUND", result.Error);
    }

    [Fact]
    public void RequestZoneTransfer_PlayerNotFound_ReturnsFailure()
    {
        var playerId = Guid.NewGuid();

        ZoneTransferResult result = _zoneService.RequestZoneTransferAsync(playerId, 1);

        Assert.False(result.Success);
        Assert.Equal("PLAYER_NOT_FOUND", result.Error);
    }

    [Fact]
    public void RequestZoneTransfer_ValidTransfer_SucceedsAndMovesPlayer()
    {
        // Setup player in zone 0
        var playerId = Guid.NewGuid();
        var player = new PlayerEntity(playerId, Guid.NewGuid(), "Player", new Position(100, 100));

        player.SetEntityId(IdRegistry.Instance.GetNextLocalId(0), 0);
        IdRegistry.Instance.RegisterEntity(player);

        Zone? zone0 = _zoneManager.GetZone(0);
        zone0?.AddEntity(playerId);

        // Transfer to zone 1
        ZoneTransferResult result = _zoneService.RequestZoneTransferAsync(playerId, 1, new Position(500, 500));

        Assert.True(result.Success);
        Assert.Equal((ushort)1, result.NewZoneId);
        Assert.NotNull(result.SpawnPosition);
        Assert.Equal(500, result.SpawnPosition.X);
        Assert.Equal(500, result.SpawnPosition.Y);

        // Verify player is in new zone
        Assert.Equal(1, player.RuntimeId.ZoneId);

        // Verify player removed from old zone
        Zone? oldZone = _zoneManager.GetZone(0);
        Assert.False(oldZone?.HasEntity(playerId));

        // Verify player added to new zone
        Zone? newZone = _zoneManager.GetZone(1);
        Assert.True(newZone?.HasEntity(playerId));
    }

    [Fact]
    public void GetPlayersInZone_NonExistentZone_ReturnsEmpty()
    {
        var players = _zoneService.GetPlayersInZone(999).ToList();

        Assert.Empty(players);
    }
}
