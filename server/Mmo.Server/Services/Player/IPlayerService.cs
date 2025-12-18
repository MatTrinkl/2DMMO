using Mmo.Server.Entities;
using Mmo.Server.Networking;
using Mmo.Shared.Enums;

namespace Mmo.Server.Services.Player;

/// <summary>
///     Service für Spieler-Operationen (Spawn, Load, Save).
/// </summary>
public interface IPlayerService
{
    /// <summary>
    ///     Spawnt einen neuen Spieler oder lädt existierenden.
    /// </summary>
    Task<ServerPlayerCharacter> SpawnPlayerAsync(
        Guid accountId,
        string characterName,
        ClientConnection connection);

    /// <summary>
    ///     Lädt Charakter-Liste für Account.
    /// </summary>
    Task<List<CharacterInfo>> GetCharacterListAsync(Guid accountId);

    /// <summary>
    ///     Lädt einen einzelnen Charakter.
    /// </summary>
    Task<CharacterData?> LoadCharacterAsync(Guid accountId, Guid characterId);

    /// <summary>
    ///     Erstellt neuen Charakter.
    /// </summary>
    Task<CharacterCreateResult> CreateCharacterAsync(
        Guid accountId,
        string name,
        Race race,
        CharacterClass characterClass,
        Gender gender);

    /// <summary>
    ///     Löscht einen Charakter.
    /// </summary>
    Task<bool> DeleteCharacterAsync(Guid accountId, Guid characterId);

    /// <summary>
    ///     Entfernt Spieler (Logout/Disconnect).
    /// </summary>
    Task RemovePlayerAsync(Guid connectionId);

    /// <summary>
    ///     Speichert Spieler-Daten und entfernt ihn aus dem Manager.
    /// </summary>
    Task SaveAndRemovePlayerAsync(Guid connectionId);

    /// <summary>
    ///     Speichert Spieler-Daten.
    /// </summary>
    Task SavePlayerAsync(ServerPlayerCharacter playerCharacter);
}
