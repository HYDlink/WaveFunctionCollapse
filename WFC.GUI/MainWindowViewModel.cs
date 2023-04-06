using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WaveFunctionCollapse;

namespace WFC.GUI;

public class MainWindowViewModel
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

    public ObservableCollection<long> ImageBitset { get; set; }

    public ImageCacheWpf ImageCacheWpf { get; set; }
    public TileSet TileSet { get; set; }

    public MainWindowViewModel()
    {
        TestSource();
    }

    public void TestSource()
    {
        var name = "Knots";
        //var file_name = $"Resources/tilesets/{name}.xml";
        var file_name = @"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\Knots.xml";
        TileSet = TileSetLoader.LoadFromFile(name, file_name);
        var image_source_converter = new ImageSourceConverter();
        var image_sources = TileSet.Tiles.Select(t => image_source_converter.ConvertFromString(TileSet.GetTileFilePath(t.Name))).OfType<ImageSource>();
        TileImages = new(image_sources);
        ImageCacheWpf = new ImageCacheWpf(TileSet);
        GenerateRandomBitSet(16, 16);
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

        var randBits = Enumerable.Repeat(0, width * height).Select(_ => RandBit());
        ImageBitset = new(randBits);
    }
}