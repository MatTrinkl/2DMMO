using System.Reflection;
using FluentAssertions;
using MessagePack;
using Mmo.Shared.Character.Entities;
using Mmo.Shared.Character.Enums;
using Mmo.Shared.Character.Interfaces;
using Mmo.Shared.Core.Records;
using Mmo.Shared.Generators;

namespace Mmo.Shared.Tests.Sync;

/// <summary>
///     Tests for DTO synchronization between PlayerEntity and PlayerEntityDto.
/// </summary>
public class EntityDtoSyncTests
{
    [Fact]
    public void PlayerEntityDto_Implements_IPlayerData()
    {
        // Arrange & Act
        bool isAssignable = typeof(IPlayerData).IsAssignableFrom(typeof(PlayerEntityDto));

        // Assert
        isAssignable.Should().BeTrue("PlayerEntityDto should implement IPlayerData");
    }

    [Fact]
    public void PlayerEntity_Implements_IPlayerData()
    {
        // Arrange & Act
        bool isAssignable = typeof(IPlayerData).IsAssignableFrom(typeof(PlayerEntity));

        // Assert
        isAssignable.Should().BeTrue("PlayerEntity should implement IPlayerData");
    }

    [Fact]
    public void FromEntity_Maps_All_Properties_Correctly()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var position = new Position(100.5f, 200.7f);
        var entity = new PlayerEntity(characterId, accountId, "TestPlayer", position)
        {
            Level = 10,
            CurrentHealth = 500,
            MaxHealth = 1000,
            CurrentResource = 300,
            MaxResource = 500,
            Race = Race.Elf,
            Class = CharacterClass.Mage,
            Gender = Gender.Female,
            Title = "The Brave",
            IsPvpFlagged = true,
            HonorPoints = 1500,
            PvpKills = 25,
            PvpDeaths = 5,
            State = CharacterState.Alive,
            AttackPower = 150,
            Armor = 75,
            MovementSpeed = 7.5f
        };
        entity.SetEntityId(42, 100);

        // Act
        var dto = PlayerEntityDto.FromEntity(entity);

        // Assert - From CombatEntity
        dto.RuntimeId.Should().Be(entity.RuntimeId);
        dto.PersistentId.Should().Be(entity.PersistentId);
        dto.Position.Should().Be(entity.Position);
        dto.PrefabId.Should().Be(entity.PrefabId);
        dto.DisplayName.Should().Be(entity.DisplayName);
        dto.Level.Should().Be(entity.Level);
        dto.CurrentHealth.Should().Be(entity.CurrentHealth);
        dto.MaxHealth.Should().Be(entity.MaxHealth);
        dto.CurrentResource.Should().Be(entity.CurrentResource);
        dto.MaxResource.Should().Be(entity.MaxResource);
        dto.CombatResourceType.Should().Be(entity.CombatResourceType);
        dto.IsInCombat.Should().Be(entity.IsInCombat);
        dto.TargetEntityId.Should().Be(entity.TargetEntityId);
        dto.Faction.Should().Be(entity.Faction);
        dto.AttackPower.Should().Be(entity.AttackPower);
        dto.Armor.Should().Be(entity.Armor);
        dto.MovementSpeed.Should().Be(entity.MovementSpeed);
        dto.BaseMovementSpeed.Should().Be(entity.BaseMovementSpeed);

        // Assert - From PlayerEntity
        dto.CharacterId.Should().Be(entity.CharacterId);
        dto.AccountId.Should().Be(entity.AccountId);
        dto.Race.Should().Be(entity.Race);
        dto.Class.Should().Be(entity.Class);
        dto.Gender.Should().Be(entity.Gender);
        dto.Title.Should().Be(entity.Title);
        dto.IsPvpFlagged.Should().Be(entity.IsPvpFlagged);
        dto.PvpFlagExpires.Should().Be(entity.PvpFlagExpires);
        dto.HonorPoints.Should().Be(entity.HonorPoints);
        dto.PvpKills.Should().Be(entity.PvpKills);
        dto.PvpDeaths.Should().Be(entity.PvpDeaths);
        dto.State.Should().Be(entity.State);
        dto.MovementFlags.Should().Be(entity.MovementFlags);
        dto.HearthstoneLocation.Should().Be(entity.HearthstoneLocation);
        dto.LastSafePosition.Should().Be(entity.LastSafePosition);
    }

    [Fact]
    public void ServerOnly_Properties_Not_In_Dto()
    {
        // Arrange
        Type dtoType = typeof(PlayerEntityDto);

        // Act
        PropertyInfo? experienceProperty = dtoType.GetProperty("Experience");
        PropertyInfo? goldProperty = dtoType.GetProperty("Gold");

        // Assert
        experienceProperty.Should().BeNull("Experience is a server-only property and should not exist in DTO");
        goldProperty.Should().BeNull("Gold is a server-only property and should not exist in DTO");
    }

    [Fact]
    public void ServerOnly_Properties_Exist_In_Entity()
    {
        // Arrange
        Type entityType = typeof(PlayerEntity);

        // Act
        PropertyInfo? experienceProperty = entityType.GetProperty("Experience");
        PropertyInfo? goldProperty = entityType.GetProperty("Gold");

        // Assert
        experienceProperty.Should().NotBeNull("Experience should exist in PlayerEntity");
        goldProperty.Should().NotBeNull("Gold should exist in PlayerEntity");
    }

    [Fact]
    public void ServerOnly_Attributes_Applied_Correctly()
    {
        // Arrange
        Type entityType = typeof(PlayerEntity);
        PropertyInfo? experienceProperty = entityType.GetProperty("Experience");
        PropertyInfo? goldProperty = entityType.GetProperty("Gold");

        // Act
        bool experienceHasServerOnly =
            experienceProperty?.GetCustomAttributes(typeof(ServerOnlyAttribute), false).Any() ?? false;
        bool goldHasServerOnly = goldProperty?.GetCustomAttributes(typeof(ServerOnlyAttribute), false).Any() ?? false;

        // Assert
        experienceHasServerOnly.Should().BeTrue("Experience should have [ServerOnly] attribute");
        goldHasServerOnly.Should().BeTrue("Gold should have [ServerOnly] attribute");
    }

    [Fact]
    public void FromEntity_Does_Not_Include_ServerOnly_Data()
    {
        // Arrange
        var entity = new PlayerEntity(Guid.NewGuid(), Guid.NewGuid(), "RichPlayer", new Position(0, 0))
        {
            Experience = 999999,
            Gold = 1000000
        };

        // Act
        var dto = PlayerEntityDto.FromEntity(entity);

        // Assert - DTO should not have these properties at all
        var dtoType = dto.GetType();
        dtoType.GetProperty("Experience").Should().BeNull();
        dtoType.GetProperty("Gold").Should().BeNull();
    }

    [Fact]
    public void PlayerEntityDto_MessagePack_Serialization_RoundTrip()
    {
        // Arrange
        var characterId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var entity = new PlayerEntity(characterId, accountId, "TestPlayer", new Position(100.5f, 200.7f))
        {
            Level = 5,
            CurrentHealth = 250,
            MaxHealth = 500,
            Race = Race.Human,
            Class = CharacterClass.Warrior,
            Gender = Gender.Male,
            Title = "The Brave"
        };
        entity.SetEntityId(42, 100);

        var dto = PlayerEntityDto.FromEntity(entity);

        // Act
        byte[] serialized = MessagePackSerializer.Serialize(dto);
        var deserialized = MessagePackSerializer.Deserialize<PlayerEntityDto>(serialized);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.RuntimeId.Should().Be(dto.RuntimeId);
        deserialized.PersistentId.Should().Be(dto.PersistentId);
        deserialized.DisplayName.Should().Be(dto.DisplayName);
        deserialized.Level.Should().Be(dto.Level);
        deserialized.CurrentHealth.Should().Be(dto.CurrentHealth);
        deserialized.MaxHealth.Should().Be(dto.MaxHealth);
        deserialized.Race.Should().Be(dto.Race);
        deserialized.Class.Should().Be(dto.Class);
        deserialized.Gender.Should().Be(dto.Gender);
        deserialized.Title.Should().Be(dto.Title);
        deserialized.CharacterId.Should().Be(dto.CharacterId);
        deserialized.AccountId.Should().Be(dto.AccountId);
    }
}
