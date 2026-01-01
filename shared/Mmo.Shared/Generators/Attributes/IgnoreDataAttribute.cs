using System;

namespace Mmo.Shared.Generators.Attributes;

/// <summary>
///     Explicitly marks a property to be excluded from all generated ListEntry types.
///     Properties without any attribute are also ignored by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IgnoreDataAttribute : Attribute
{
}
