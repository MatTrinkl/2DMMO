namespace Mmo.Shared.Generators;

/// <summary>
///     Marks an interface as a Delta DTO Union root for polymorphic serialization.
///     The generator will create the Union interface for Delta DTOs.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class GenerateDeltaDtoUnionAttribute : Attribute
{
    /// <summary>
    ///     Name of the generated Delta Union interface.
    ///     Default: {InterfaceName without I}DeltaUnion
    /// </summary>
    public string? UnionName { get; set; }

    /// <summary>
    ///     Namespace for the generated Delta Union.
    ///     Default: {OriginalNamespace}.Dtos
    /// </summary>
    public string? Namespace { get; set; }
}

/// <summary>
///     Marks an interface as a member of a Delta DTO Union.
///     The generated Delta DTO will implement the Union interface.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class DeltaDtoUnionMemberAttribute : Attribute
{
    public DeltaDtoUnionMemberAttribute(int unionIndex, Type unionType)
    {
        UnionIndex = unionIndex;
        UnionType = unionType;
    }

    /// <summary>
    ///     The Union index for MessagePack serialization.
    /// </summary>
    public int UnionIndex { get; }

    /// <summary>
    ///     The parent Union interface type.
    /// </summary>
    public Type UnionType { get; }
}
