namespace Mmo.Shared.Generators.Attributes;

/// <summary>
///     Marks a class/struct for ListEntry DTO generation.
///     Can be applied multiple times for different entry types.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public sealed class GenerateListEntryAttribute : Attribute
{
    public GenerateListEntryAttribute(string entryTypeName)
    {
        EntryTypeName = entryTypeName;
    }

    /// <summary>
    ///     The name of the generated ListEntry class (e.g., "ZoneListEntry").
    /// </summary>
    public string EntryTypeName { get; }

    /// <summary>
    ///     Optional namespace for the generated class.
    ///     Defaults to source class namespace + ".Generated".
    /// </summary>
    public string? Namespace { get; set; }
}
