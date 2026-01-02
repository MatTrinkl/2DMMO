namespace Mmo.Shared.Generators;

/// <summary>
///     Marks a class or a struct for automatic DTO Generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class GenerateDtoAttribute : Attribute
{
    /// <summary>
    ///     When true, übernimmt das DTO alle Interfaces der Source-Class.
    ///     Default: false
    /// </summary>
    public bool InheritInterfaces { get; set; } = false;

    /// <summary>
    ///     Custom Name für das generierte DTO.
    ///     Default: {ClassName}Dto
    /// </summary>
    public string? DtoName { get; set; }

    /// <summary>
    ///     Namespace für das generierte DTO.
    ///     Default: gleicher Namespace wie Source
    /// </summary>
    public string? DtoNamespace { get; set; }

    /// <summary>
    ///     Suffix für das generierte DTO.
    ///     Default: "Dto"
    /// </summary>
    public string DtoSuffix { get; set; } = "Dto";

    /// <summary>
    ///     When true and applied to an interface, the generated DTO will implement the source interface.
    ///     Default: true for interfaces
    /// </summary>
    public bool ImplementSourceInterface { get; set; } = true;
}
