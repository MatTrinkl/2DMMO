namespace Mmo.Shared.Generators;

/// <summary>
///     Optional configuration for a property in the generated DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DtoPropertyAttribute : Attribute
{
    /// <summary>
    ///     Override the property name in the DTO.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Explicit MessagePack Key Index.
    ///     Default: automatic
    /// </summary>
    public int Key { get; set; } = -1;
}
