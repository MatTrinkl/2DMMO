using Mmo.Server. Entities;
using Mmo. Server.Networking;
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
    Task<ServerPlayer> SpawnPlayerAsync(
        Guid accountId,
        string characterName,
        ClientConnection connection);

    /// <summary>
    ///     Lädt Charakter-Liste für Account.
    /// </summary>
    Task<List<CharacterInfo>> GetCharacterListAsync(Guid accountId);

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
    ///     Entfernt Spieler (Logout/Disconnect).
    /// </summary>
    Task RemovePlayerAsync(Guid connectionId);

    /// <summary>
    ///     Speichert Spieler-Daten.
    /// </summary>
    Task SavePlayerAsync(ServerPlayer player);
}




