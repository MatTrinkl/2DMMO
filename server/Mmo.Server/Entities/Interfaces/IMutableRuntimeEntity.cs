using Mmo.Shared.Entities.Structs;

namespace Mmo.Server.Entities.Interfaces;

/// <summary>
///     Interface for entities that need mutable RuntimeId access.
///     Used to work around the member hiding issue where derived classes
///     use 'new' to provide a settable RuntimeId property.
/// </summary>
public interface IMutableRuntimeEntity
{
    /// <summary>
    ///     Runtime identity with getter access for polymorphic RuntimeId reading.
    ///     This avoids the issue where accessing RuntimeId through BaseEntity
    ///     returns the init-only base property instead of the hidden derived property.
    /// </summary>
    EntityIdentity RuntimeId { get; }
}
