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

var name = "Knots";
var fileName = $"tilesets/{name}.xml";
// var xml = File.ReadAllText(fileName);
var xroot = XDocument.Load(fileName).Root;
var set = new TileSet(
    xroot.Element("tiles").Descendants()
        .Select(tile =>
        {
            var name = tile.Attribute("name").Value;
            var (rawName, rotate) = ParseTileName(name);
            var symmetry = Symmetry.TryParse<Symmetry>(tile.Attribute("symmetry").Value, out var s) ? s : default;
            return new Tile(rawName, symmetry, rotate);
        }).ToList(),
    xroot.Element("neighbors").Descendants()
        .Select(neighbor =>
        {
            var (leftName, leftRotate) = ParseTileName(neighbor.Attribute("left").Value);
            var (rightName, rightRotate) = ParseTileName(neighbor.Attribute("right").Value);
            return new Neighbor(leftName, leftRotate, rightName, rightRotate);
        }).ToList(),
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
using var output_img = new Image<Bgra32>(tile_width * 8, tile_height * 4, Color.White);
output_img.Mutate(ctx =>
    ctx.DrawImage(image, new Point(0, 0), 1f));
        // .Resize(tile_width * 4, image.Height * 4, new NearestNeighborResampler()));
foreach (var (i, rightTile, rightRotate) in set.GetRightNeighbors(curTile.Name).Select((t, i) => (i, t.Name, t.Rotate)))
{
    var x = tile_width * 4 + tile_width * (i % 4);
    var y = tile_height * (i / 4);
    using var neighbor = Image.Load<Bgra32>(TileFile(name, rightTile));
    neighbor.Mutate(n => n .Rotate(ToRotateMode(rightRotate)));
    output_img.Mutate(ctx => ctx.DrawImage(neighbor, new Point(x, y), 1f)
    );
    // .Rotate(ToRotateMode(rightRotate)));
    // .Rotate((RotateMode)(rightRotate)));
}

RotateMode ToRotateMode(int rotate) => rotate switch
{
    0 => RotateMode.None,
    1 => RotateMode.Rotate270,
    2 => RotateMode.Rotate180,
    3 => RotateMode.Rotate90,
    _ => throw new ArgumentOutOfRangeException(nameof(rotate), rotate, null)
};
Console.WriteLine(set.Tiles.Count);

var folder = Directory.CreateDirectory("output");
foreach (var file in folder.GetFiles()) file.Delete();
var outputCornerX4Png = $"output\\corner_x4.png";
output_img.Save(outputCornerX4Png, new PngEncoder());
Process.Start(new ProcessStartInfo(outputCornerX4Png) { UseShellExecute = true });

public enum Symmetry
{
    L,
    I,
    X,
    T,
    [Description("\\")] Slash,
}

public record Tile(string Name, Symmetry Symmetry, int Rotate);

public record Neighbor(string Left, int LeftRotate, string Right, int RightRotate);

public record Subset(string Name, List<string> Tiles);

public record TileSet(List<Tile> Tiles, List<Neighbor> Neighbors, List<Subset> Subsets)
{
    public Tile GetTile(string name) => Tiles.FirstOrDefault(t => t.Name == name);

    public List<(string Name, int Rotate)> GetRightNeighbors(string name)
    {
        var fromLeft = Neighbors.Where(n => n.Left == name && n.LeftRotate == 0)
            .Select(n => (n.Right, n.RightRotate));
        return fromLeft.ToList();
    }
};