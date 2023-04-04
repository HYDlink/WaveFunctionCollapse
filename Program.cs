using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

// namespace WaveFunctionCollapse;


static TileSet Load(string name2)
{
    (string rawName, int rotate) ParseTileName(string name1)
    {
        var strings = name1.Split(' ', 2);
        var (rawName1, i) = strings switch
        {
            [{ } s1, { } s2] => (s1, int.Parse(s2)),
            [{ } s3] => (s3, 0),
            _ => throw new Exception("Invalid Name"),
        };
        return (rawName1, i);
    }

    var fileName = $"tilesets/{name2}.xml";
    // var xml = File.ReadAllText(fileName);
    var xroot = XDocument.Load(fileName).Root;
    var tileSet = new TileSet(
        xroot.Element("tiles").Descendants()
            .Select(tile =>
            {
                var name = tile.Attribute("name").Value;
                var symmetry = Symmetry.TryParse<Symmetry>(tile.Attribute("symmetry").Value, out var s) ? s : default;
                return new Tile(name, symmetry);
            }).ToList(),
        xroot.Element("neighbors").Descendants()
            .Select(neighbor =>
            {
                var (leftName, leftRotate) = ParseTileName(neighbor.Attribute("left").Value);
                var (rightName, rightRotate) = ParseTileName(neighbor.Attribute("right").Value);
                return new Neighbor(leftName, leftRotate, rightName, rightRotate);
            }).ToList(),
        null
        // xroot.Element("subsets").Descendants()
        //     .Select(s => new Subset(
        //         s.Attribute("name").Value,
        //         s.Descendants().Select(d => d.Attribute("name").Value).ToList())).ToList()
    ) { Name = name2 };
    tileSet.CalcNeighbors();
    return tileSet;
}


void CollapseTile(string name)
{
    var set = Load(name);

    string TileFile(string name, string tileName) => $"tilesets/{name}/{tileName}.png";

    void ClearOutputDir()
    {
        var folder = Directory.CreateDirectory("output");
        foreach (var file in folder.GetFiles()) file.Delete();
    }

    void SaveImage(Image<Bgra32> outputImg, string name = "corner")
    {
        var outputCornerX4Png = $"output\\{name}.png";
        outputImg.Save(outputCornerX4Png, new PngEncoder());
        // Process.Start(new ProcessStartInfo(outputCornerX4Png) { UseShellExecute = true });
    }

    var cache = new ImageCache(set);

    void DrawOneTile(int tileIndex, int rotation, string s1)
    {
        var map = new long [3, 3];
        var index = (tileIndex * 4 + rotation);
        var bitSet = 1 << index;
        var neighbor = set.IndexToNeighbors[index];
        map[1, 1] = bitSet;
        map[0, 1] = neighbor[2];
        map[2, 1] = neighbor[0];
        map[1, 0] = neighbor[1];
        map[1, 2] = neighbor[3];
        var image = cache.Draw(map);
        SaveImage(image, $"{s1}_{index}");
    }

    void TestNeighbors()
    {
        var tc = set.Tiles.Count;
        var map = new long[4 * 3, tc * 3];
        for (int tileIndex = 0; tileIndex < tc; tileIndex++)
        {
            var (s, symmetry) = set.Tiles[tileIndex];
            for (int rotation = 0; rotation < symmetry.RotationCount(); rotation++)
            {
                // DrawOneTile(tileIndex, rotation, s);
                var index = (tileIndex * 4 + rotation);
                var bitSet = 1L << index;
                var neighbor = set.IndexToNeighbors[index];
                // set.Validate(bitSet);
                // set.Validate(neighbor[2]);
                // set.Validate(neighbor[0]);
                // set.Validate(neighbor[1]);
                // set.Validate(neighbor[3]);
                map[rotation * 3 + 1, tileIndex * 3 + 1] = bitSet;
                map[rotation * 3 + 0, tileIndex * 3 + 1] = neighbor[2];
                map[rotation * 3 + 2, tileIndex * 3 + 1] = neighbor[0];
                map[rotation * 3 + 1, tileIndex * 3 + 0] = neighbor[1];
                map[rotation * 3 + 1, tileIndex * 3 + 2] = neighbor[3];
            }
        }

        var image = cache.Draw(map);
        SaveImage(image, $"Neighbors_{name}");
    }

    TestNeighbors();
    // var testCache = cache.TestImageBufferByBitSet();
    // SaveImage(testCache, $"{name}_enlarged_cache");
    // return;
    var waveFunctionCollapse = set.WaveFunctionCollapse(24, 24);
    var draw = cache.Draw(waveFunctionCollapse);
    SaveImage(draw, $"Collapse_{name}");
}

// CollapseTile("Castle");
// return;
foreach (var name in new[] { "Rooms", "Castle", "Circles", "Knots", "Circuit", "FloorPlan", /*"Summer"*/ })
{
    try
    {
        CollapseTile(name);
    }
    catch (InvalidOperationException e)
    {
        Console.WriteLine($"{name} Collapse failed");
        Console.WriteLine(e);
    }
}