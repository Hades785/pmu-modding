using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using ReactiveUI;
using pmu_stubs;

namespace pmu_tileset_tool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        private Bitmap? _tile;
        public Bitmap? Tile
        {
            get => _tile;
            private set => this.RaiseAndSetIfChanged(ref _tile, value);
        }

        private Tileset? _tileset;
        public Tileset? Tileset
        {
            get => _tileset;
            private set
            {
                this.RaiseAndSetIfChanged(ref _tileset, value);
                TileId = 0;
            }
        }

        private int _currentTile = 0;
        public int TileId
        {
            get => _currentTile;
            private set
            {
                this.RaiseAndSetIfChanged(ref _currentTile, value);
                RefreshTile();
            }
        }

        public void PreviousTile() => TileId = (TileId + Tileset!.TileCount - 1) % Tileset!.TileCount;
        public void NextTile() => TileId = (TileId + Tileset!.TileCount + 1) % Tileset!.TileCount;

        private void RefreshTile()
        {
            using var imageStream = new MemoryStream(Tileset!.TileData[TileId]);
            Tile = Bitmap.DecodeToWidth(imageStream, 160, BitmapInterpolationMode.Default);
        }

        public async void LoadTileset()
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime app) return;

            var dialog = new OpenFileDialog { Filters = { new FileDialogFilter { Extensions = { "tile" }, Name = "PMU Tileset Files" } }};
            var filePath = (await dialog.ShowAsync(app.MainWindow))?[0];
            if (filePath == null) return;

            await using var file = File.Open(filePath, FileMode.Open);
            Tileset = new Tileset(file);
        }

        public async void ReplaceTile()
        {
            if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime app) return;

            var dialog = new OpenFileDialog { Filters = { new FileDialogFilter { Extensions = { "png" }, Name = "PNG image files" } }};
            var filePath = (await dialog.ShowAsync(app.MainWindow))?[0];
            if (filePath == null) return;

            var tile = await File.ReadAllBytesAsync(filePath);
            Tileset!.TileData[TileId] = tile;
            RefreshTile();
        }
    }
}