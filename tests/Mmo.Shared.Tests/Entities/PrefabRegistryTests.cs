using Mmo.Shared.Prefab;
using Mmo.Shared.Prefab.Configurations;

namespace Mmo.Shared.Tests.Entities;

public class PrefabRegistryTests
{
    private PrefabRegistry CreateTestRegistry()
    {
        var registry = new PrefabRegistry();
        var config = new PrefabsConfig(new[]
        {
            new PrefabConfig(1, "playerDefault", "player", "Default player"),
            new PrefabConfig(100, "questgiverOldMan", "npc", "Old man NPC"),
            new PrefabConfig(1000, "goblin", "mob", "Goblin enemy")
        });
        registry.LoadFromConfig(config);
        return registry;
    }

    [Fact]
    public void LoadFromConfig_LoadsPrefabs()
    {
        PrefabRegistry registry = CreateTestRegistry();

        Assert.Equal(3, registry.Count);
    }

    [Fact]
    public void GetId_WithValidCategoryAndName_ReturnsCorrectId()
    {
        PrefabRegistry registry = CreateTestRegistry();

        ushort id = registry.GetId("player", "playerDefault");

        Assert.Equal((ushort)1, id);
    }

    [Fact]
    public void GetId_WithInvalidName_ThrowsKeyNotFoundException()
    {
        PrefabRegistry registry = CreateTestRegistry();

        Assert.Throws<KeyNotFoundException>(() => registry.GetId("player", "invalid"));
    }

    [Fact]
    public void GetConfig_WithValidId_ReturnsConfig()
    {
        PrefabRegistry registry = CreateTestRegistry();

        PrefabConfig config = registry.GetConfig(1);

        Assert.Equal("playerDefault", config.Name);
        Assert.Equal("player", config.Category);
    }

    [Fact]
    public void GetConfig_WithInvalidId_ThrowsKeyNotFoundException()
    {
        PrefabRegistry registry = CreateTestRegistry();

        Assert.Throws<KeyNotFoundException>(() => registry.GetConfig(999));
    }

    [Fact]
    public void TryGetId_WithValidName_ReturnsTrue()
    {
        PrefabRegistry registry = CreateTestRegistry();

        bool result = registry.TryGetId("mob", "goblin", out ushort id);

        Assert.True(result);
        Assert.Equal((ushort)1000, id);
    }

    [Fact]
    public void TryGetId_WithInvalidName_ReturnsFalse()
    {
        PrefabRegistry registry = CreateTestRegistry();

        bool result = registry.TryGetId("mob", "invalid", out ushort id);

        Assert.False(result);
        Assert.Equal((ushort)0, id);
    }

    [Fact]
    public void HasPrefab_WithValidId_ReturnsTrue()
    {
        PrefabRegistry registry = CreateTestRegistry();

        Assert.True(registry.HasPrefab(1));
        Assert.True(registry.HasPrefab(100));
        Assert.True(registry.HasPrefab(1000));
    }

    [Fact]
    public void HasPrefab_WithInvalidId_ReturnsFalse()
    {
        PrefabRegistry registry = CreateTestRegistry();

        Assert.False(registry.HasPrefab(999));
    }

    [Fact]
    public void GetPrefabsByCategory_ReturnsCorrectPrefabs()
    {
        PrefabRegistry registry = CreateTestRegistry();

        var npcs = registry.GetPrefabsByCategory("npc").ToList();

        Assert.Single(npcs);
        Assert.Equal("questgiverOldMan", npcs[0].Name);
    }

    [Fact]
    public void LoadFromConfig_WithDuplicateId_ThrowsException()
    {
        var registry = new PrefabRegistry();
        var config = new PrefabsConfig(new[]
        {
            new PrefabConfig(1, "player1", "player"),
            new PrefabConfig(1, "player2", "player")
        });

        Assert.Throws<InvalidOperationException>(() => registry.LoadFromConfig(config));
    }

    [Fact]
    public void LoadFromConfig_WithDuplicateName_ThrowsException()
    {
        var registry = new PrefabRegistry();
        var config = new PrefabsConfig(new[]
        {
            new PrefabConfig(1, "playerDefault", "player"),
            new PrefabConfig(2, "playerDefault", "player")
        });

        Assert.Throws<InvalidOperationException>(() => registry.LoadFromConfig(config));
    }
}
