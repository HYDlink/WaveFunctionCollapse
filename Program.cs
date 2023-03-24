using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

// namespace WaveFunctionCollapse;


var name = "Knots";
var fileName = $"tilesets/{name}.xml";
// var xml = File.ReadAllText(fileName);
var xroot = XDocument.Load(fileName).Root;
var set = new TileSet(
    xroot.Element("tiles").Descendants()
        .Select(tile =>
        {
            var name = tile.Attribute("name").Value;
            var strings = name.Split(' ', 2);
            var (rawName, rotate) = strings switch
            {
                [{ } s1, { } s2] => (s1, s2),
                [{ } s3] => (s3, "0"),
                _ => throw new Exception("Invalid Name"),
            };
            var symmetry = Symmetry.TryParse<Symmetry>(tile.Attribute("symmetry").Value, out var s) ? s : default;
            return new Tile(rawName, symmetry, rotate);
        }).ToList(),
    xroot.Element("neighbors").Descendants()
        .Select(neighbor => new Neighbor(neighbor.Attribute("left").Value, neighbor.Attribute("right").Value)).ToList(),
    // null
    xroot.Element("subsets").Descendants()
        .Select(s => new Subset(
            s.Attribute("name").Value,
            s.Descendants().Select(d => d.Attribute("name").Value).ToList())).ToList()
);

string TileFile(string name, string tileName) => $"tilesets/{name}/{tileName}.png";

using var image = Image.Load<Bgra32>(TileFile(name, "corner"));
var tile_width = image.Width;
var tile_height = image.Height;
image.Mutate(m => m.Resize(tile_width * 4, image.Height * 4, new NearestNeighborResampler()));


var curTile = set.Tiles.First();
var rightTiles = set.Tiles;
using var output_img = new Image<Bgra32>(tile_width * 8, tile_height * 4, Color.White);
output_img.Mutate(ctx => ctx.DrawImage(image, new Point(0, 0), 1f));
foreach (var (i, rightTile) in rightTiles.Select((t,i) => (i, t.Name)))
{
    var x = tile_width * 4 + tile_width * (i % 4);
    var y = tile_height * (i / 4);
    using var neighbor = Image.Load<Bgra32>(TileFile(name, rightTile));
    output_img.Mutate(ctx => ctx.DrawImage(neighbor, new Point(x, y), 1f));
}
Console.WriteLine(set.Tiles.Count);

var folder = Directory.CreateDirectory("output");
foreach (var file in folder.GetFiles()) file.Delete();
var outputCornerX4Png = $"output\\corner_x4.png";
output_img.Save(outputCornerX4Png, new PngEncoder());
Process.Start(new ProcessStartInfo(outputCornerX4Png) {UseShellExecute = true});

public enum Symmetry
{
    L,
    I,
    X,
    T,
    [Description("\\")] Slash,
}

public record Tile(string Name, Symmetry Symmetry, string Rotate);

public record Neighbor(string Left, string Right);

public record Subset(string Name, List<string> Tiles);

public record TileSet(List<Tile> Tiles, List<Neighbor> Neighbors, List<Subset> Subsets);