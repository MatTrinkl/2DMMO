namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a property that should be ignored in the DTO.
///     Alias for [ServerOnly] with clear semantic.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DtoIgnoreAttribute : Attribute
{
}
