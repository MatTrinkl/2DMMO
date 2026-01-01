using System;

namespace Mmo.Shared.Generators.Attributes;

/// <summary>
///     Marks a property to be included in ALL generated ListEntry types.
///     The property will be non-nullable in the generated DTO.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BaseDataAttribute : Attribute
{
}
