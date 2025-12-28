namespace Mmo.Shared.Generators;

/// <summary>
///     Marks an interface as a DTO Union root for polymorphic serialization.
///     The generator will create the Union interface and polymorphic ToUnionDto extensions.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class GenerateDtoUnionAttribute : Attribute
{
    /// <summary>
    ///     Name of the generated Union interface.
    ///     Default:  {InterfaceName without I}DtoUnion
    /// </summary>
    public string? UnionName { get; set; }

    /// <summary>
    ///     Namespace for the generated Union.
    ///     Default: {OriginalNamespace}. Dtos
    /// </summary>
    public string? Namespace { get; set; }
}

/// <summary>
///     Marks an interface as a member of a DTO Union.
///     The generated DTO will implement the Union interface.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class DtoUnionMemberAttribute : Attribute
{
    public DtoUnionMemberAttribute(int unionIndex, Type unionType)
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
