namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a class to generate a DTO for network synchronization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GenerateDtoAttribute : Attribute
{
    /// <summary>
    ///     Optional custom name for the generated DTO. If null, appends "Dto" to class name.
    /// </summary>
    public string? DtoName { get; set; }

    /// <summary>
    ///     Property names to exclude from the DTO.
    /// </summary>
    public string[] Exclude { get; set; } = Array.Empty<string>();
}

/// <summary>
///     Marks a property as server-only, preventing it from being synchronized to clients.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ServerOnlyAttribute : Attribute { }

/// <summary>
///     Explicitly marks a property for synchronization to clients.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SyncToClientAttribute : Attribute { }
