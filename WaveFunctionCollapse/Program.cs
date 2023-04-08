using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WFC.Core;

partial class Program
{
    static void ClearOutputDir()
    {
        var folder = Directory.CreateDirectory("output");
        foreach (var file in folder.GetFiles()) file.Delete();
    }

    static void CollapseTile(string name)
    {
        var file_name = $@"C:\Work\Projects\WaveFunctionCollapse\WFC.GUI\Resources\tilesets\{name}.xml";
        var set = TileSetLoader.LoadFromFile(name, file_name);


        void SaveImage(Image<Bgra32> outputImg, string name = "corner")
        {
            var outputCornerX4Png = $"output\\{name}.png";
            outputImg.Save(outputCornerX4Png, new PngEncoder());
            // Process.Start(new ProcessStartInfo(outputCornerX4Png) { UseShellExecute = true });
        }

        var cache = new ImageCache(set);

        void DrawOneTile(int tileIndex, int rotation, string s1)
        {
            var map = new long[3, 3];
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
        var waveFunctionCollapse = set.WaveFunctionCollapse(24, 24);
        var draw = cache.Draw(waveFunctionCollapse);
        SaveImage(draw, $"Collapse_{name}");
    }

    static void Main()
    {
        ClearOutputDir();
        // CollapseTile("Castle");
        // return;
        foreach (var name in new[]
                 {
                     "Rooms", "Castle", "Circles", "Knots", "Circuit", "FloorPlan", "Summer"
                 })

        {
            try
            {
                CollapseTile(name);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"{name} CollapseByRandomIndex failed");
                Console.WriteLine(e);
            }
        }
    }
}