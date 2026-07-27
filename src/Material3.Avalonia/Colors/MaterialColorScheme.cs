// Ported from material-color-utilities (https://github.com/material-foundation/material-color-utilities), Apache-2.0.

namespace Material3.Avalonia.Colors;

/// <summary>
/// The full set of Material 3 color roles, as ARGB values.
/// </summary>
public sealed record MaterialColorScheme
{
    public required uint Primary { get; init; }
    public required uint OnPrimary { get; init; }
    public required uint PrimaryContainer { get; init; }
    public required uint OnPrimaryContainer { get; init; }
    public required uint Secondary { get; init; }
    public required uint OnSecondary { get; init; }
    public required uint SecondaryContainer { get; init; }
    public required uint OnSecondaryContainer { get; init; }
    public required uint Tertiary { get; init; }
    public required uint OnTertiary { get; init; }
    public required uint TertiaryContainer { get; init; }
    public required uint OnTertiaryContainer { get; init; }
    public required uint Error { get; init; }
    public required uint OnError { get; init; }
    public required uint ErrorContainer { get; init; }
    public required uint OnErrorContainer { get; init; }
    public required uint Surface { get; init; }
    public required uint SurfaceDim { get; init; }
    public required uint SurfaceBright { get; init; }
    public required uint SurfaceContainerLowest { get; init; }
    public required uint SurfaceContainerLow { get; init; }
    public required uint SurfaceContainer { get; init; }
    public required uint SurfaceContainerHigh { get; init; }
    public required uint SurfaceContainerHighest { get; init; }
    public required uint OnSurface { get; init; }
    public required uint OnSurfaceVariant { get; init; }
    public required uint Outline { get; init; }
    public required uint OutlineVariant { get; init; }
    public required uint Shadow { get; init; }
    public required uint Scrim { get; init; }
    public required uint SurfaceTint { get; init; }
    public required uint InverseSurface { get; init; }
    public required uint InverseOnSurface { get; init; }
    public required uint InversePrimary { get; init; }
    public required uint PrimaryFixed { get; init; }
    public required uint PrimaryFixedDim { get; init; }
    public required uint OnPrimaryFixed { get; init; }
    public required uint OnPrimaryFixedVariant { get; init; }
    public required uint SecondaryFixed { get; init; }
    public required uint SecondaryFixedDim { get; init; }
    public required uint OnSecondaryFixed { get; init; }
    public required uint OnSecondaryFixedVariant { get; init; }
    public required uint TertiaryFixed { get; init; }
    public required uint TertiaryFixedDim { get; init; }
    public required uint OnTertiaryFixed { get; init; }
    public required uint OnTertiaryFixedVariant { get; init; }
}
