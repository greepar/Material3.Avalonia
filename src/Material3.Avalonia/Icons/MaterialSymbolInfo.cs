namespace Material3.Avalonia.Icons;

/// <summary>
/// Describes an entry in the Material Symbols catalog.
/// </summary>
/// <param name="Index">The stable index used to retrieve the icon geometry.</param>
/// <param name="Name">The original Google Material Symbols name.</param>
/// <param name="PropertyName">The generated C# property name.</param>
public readonly record struct MaterialSymbolInfo(int Index, string Name, string PropertyName);
