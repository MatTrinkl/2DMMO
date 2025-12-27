namespace Mmo.Shared.Generators;

/// <summary>
///     Mars a property as server-only.
///     This property will not be used while generating the DTO. It will be missing in the final structure.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ServerOnlyAttribute : Attribute
{
}
