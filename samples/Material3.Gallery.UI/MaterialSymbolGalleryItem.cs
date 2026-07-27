using Avalonia.Media;
using Material3.Avalonia.Icons;

namespace Material3.Gallery.UI;

public sealed class MaterialSymbolGalleryItem
{
    private readonly MaterialSymbolInfo _symbol;
    private readonly bool _filled;

    public MaterialSymbolGalleryItem(MaterialSymbolInfo symbol, bool filled)
    {
        _symbol = symbol;
        _filled = filled;
    }

    public string Name => _symbol.Name;

    public string ApiName => $"{(_filled ? nameof(MaterialSymbolsFilled) : nameof(MaterialSymbols))}.{_symbol.PropertyName}";

    public Geometry Geometry => MaterialSymbolCatalog.GetGeometry(_symbol.Index, _filled);
}
