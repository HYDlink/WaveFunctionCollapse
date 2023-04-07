using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace WFC.Core;

public static class TileSetLoader
{
    public static TileSet Load(string name) => LoadFromFile(name, $"tilesets/{name}.xml");
    public static TileSet LoadFromFile(string name, string fileName)
    {
        // var xml = File.ReadAllText(fileName);
        var xroot = XDocument.Load(fileName).Root;
        var tileSet = new TileSet(
            xroot.Element("tiles").Descendants()
                .Select(tile =>
                {
                    var tileName = tile.Attribute("name").Value;
                    var symmetry = Symmetry.TryParse<Symmetry>(tile.Attribute("symmetry").Value, out var s) ? s : default;
                    return new Tile(tileName, symmetry);
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
        ) { Name = name };
        tileSet.TileSetDirectory = Path.GetDirectoryName(fileName);
        tileSet.CalcNeighbors();
        return tileSet;
    }

    private static (string rawName, int rotate) ParseTileName(string name1)
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
}