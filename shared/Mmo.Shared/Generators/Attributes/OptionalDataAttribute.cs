namespace Mmo.Shared.Generators.Attributes;

/// <summary>
///     Marks a property to be included only in specific ListEntry types.
///     The property will be nullable in the generated DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OptionalDataAttribute : Attribute
{
    public OptionalDataAttribute(params string[] entryTypeNames)
    {
        EntryTypeNames = entryTypeNames;
    }

    /// <summary>
    ///     The ListEntry type names where this property should be included.
    /// </summary>
    public string[] EntryTypeNames { get; }
}
