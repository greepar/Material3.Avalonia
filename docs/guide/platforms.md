# Platforms

The theme is pure managed code on top of Avalonia — the same package runs on desktop, Android, iOS and WebAssembly. The repo's `samples/` folder is a working reference for all four heads sharing one UI project.

```
samples/
├── Material3.Gallery.UI/        shared UserControl UI (App + MainView)
├── Material3.Gallery/           desktop head (NativeAOT in Release)
├── Material3.Gallery.Android/   net10.0-android
├── Material3.Gallery.iOS/       net10.0-ios
└── Material3.Gallery.Browser/   net10.0-browser (WASM)
```

## Desktop

Nothing special — apply the theme and go. The sample publishes as a single-file NativeAOT binary:

```bash
dotnet publish samples/Material3.Gallery -c Release -r osx-arm64 --self-contained
```

The library declares `IsAotCompatible` and uses zero reflection, so trimming/AOT work out of the box.

## Single-view platforms (Android / iOS / Browser)

Support both lifetimes in your `App`:

```csharp
public override void OnFrameworkInitializationCompleted()
{
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        desktop.MainWindow = new MainWindow();
    else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        singleView.MainView = new MainView();   // a UserControl

    base.OnFrameworkInitializationCompleted();
}
```

### Android

```bash
dotnet build samples/Material3.Gallery.Android -t:Run   # device or emulator attached
```

- The activity must use an **AppCompat** theme (`Theme.AppCompat.DayNight.NoActionBar`), because `AvaloniaActivity` derives from AppCompatActivity.
- `minSdk` 23+.

### iOS

```bash
dotnet build samples/Material3.Gallery.iOS -t:Run       # simulator
```

### WebAssembly

```bash
dotnet run --project samples/Material3.Gallery.Browser
```

`Program.cs` is three lines:

```csharp
public static Task Main(string[] args) => BuildAvaloniaApp()
    .WithInterFont()
    .StartBrowserAppAsync("out");   // mounts into <div id="out"> in index.html

public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>();
```

A hosted AOT build of the gallery is available as the [live demo](../demo/){target="_blank"}. GitHub Pages serves precompressed `.br` resources through the browser's native Brotli `DecompressionStream`, with automatic fallback to uncompressed resources.

## Touch adaptation notes

- All interactive controls meet the 48dp touch-target metric (`Md3MinTouchTarget`).
- Ripples originate at the touch point and cancel automatically when a scroll gesture takes over the pointer.
- For phone layouts, pair the theme with `SplitView` (overlay drawer + hamburger) as shown in `MainView.axaml` — the gallery switches automatically below 900 px width.
