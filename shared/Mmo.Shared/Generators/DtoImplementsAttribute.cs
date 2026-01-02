namespace Mmo.Shared.Generators;

/// <summary>
///     Includes explicit interfaces which will be implemented in the DTO.
///     Can be used multiple times for different interfaces.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public class DtoImplementsAttribute : Attribute
{
    public DtoImplementsAttribute(Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException($"{interfaceType.Name} is not an interface", nameof(interfaceType));
        InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }
}
