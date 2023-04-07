using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using WFC.Core;
using WaveFunctionCollapse = WFC.Core.WaveFunctionCollapse;

namespace WFC.GUI;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<ImageSource> TileImages { get; set; }

    public ObservableCollection<string> RoomImageNames { get; } = new()
    {
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\bridge.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\component.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\connection.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\corner.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\dskew.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\skew.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\substrate.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\t.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\track.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\transition.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\turn.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\viad.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\vias.png",
        @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Circuit\wire.png",
    };

    [ObservableProperty]
    public long[] imageBitset;

    public ImageCacheWpf ImageCacheWpf { get; set; }
    public TileSet TileSet { get; set; }
    public WaveFunctionCollapse WFC { get; set; }

    [ObservableProperty]
    public int width = 16;
    [ObservableProperty]
    public int height = 16;
    [ObservableProperty]
    public bool canChangeWidthHeight = true;

    [ObservableProperty]
    public int backStepCount = 1;
    [ObservableProperty]
    public int nextStepCount = 1;

    public MainWindowViewModel()
    {
        TestSource();
    }

    public void UpdateImageBitSetByWFC()
    {
        ImageBitset = WFC.Image.Cast<long>().ToArray();
    }

    public void TestSource()
    {
        var name = "Summer";
        //var file_name = $"Resources/tilesets/{name}.xml";
        var file_name = $@"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\{name}.xml";
        TileSet = TileSetLoader.LoadFromFile(name, file_name);
        var image_source_converter = new ImageSourceConverter();
        var image_sources = TileSet.Tiles.Select(t => image_source_converter.ConvertFromString(TileSet.GetTileFilePath(t.Name))).OfType<ImageSource>();
        TileImages = new(image_sources);
        ImageCacheWpf = new ImageCacheWpf(TileSet);

        // GenerateRandomBitSet(16, 16);
        WFC = new WaveFunctionCollapse(TileSet, width, height);
        
        imageBitset = WFC.Image.Cast<long>().ToArray();
        canChangeWidthHeight = false;
    }

    public void GenerateRandomBitSet(int width, int height)
    {
        var random = new Random();
        var allIndex = TileSet.FullEncoding.GetAllIndex().ToList();
        var max_bit_index = allIndex.Last();

        long RandBit()
        {
            return 1L << allIndex[random.Next(allIndex.Count)];
        }

        long RandBits()
        {
            return random.NextInt64(TileSet.FullEncoding) & TileSet.FullEncoding;
        }

        var randBits = Enumerable.Repeat(0, width * height).Select(_ => RandBits());
        imageBitset = randBits.ToArray();
    }

    public void Reset()
    {
        WFC.Width = width;
        WFC.Height = height;
        WFC.Reset();
        UpdateImageBitSetByWFC();
    }
}