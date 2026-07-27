using System.Threading;
using Avalonia.Media;

namespace Material3.Avalonia.Icons;

internal static class MaterialSymbolGeometry
{
    public static Geometry Get(ref Geometry? cache, string path)
    {
        var geometry = Volatile.Read(ref cache);
        if (geometry is not null)
            return geometry;

        geometry = StreamGeometry.Parse(path);
        return Interlocked.CompareExchange(ref cache, geometry, null) ?? geometry;
    }
}
