using System;
using System.Collections.Generic;
using System.Linq;


public record Tile(string Name, Symmetry Symmetry);

public record Neighbor(string Left, int LeftRotate, string Right, int RightRotate);

public record Subset(string Name, List<string> Tiles);

// Tiles 没有强制 DistinctBy Name
public record TileSet(List<Tile> Tiles, List<Neighbor> Neighbors, List<Subset> Subsets)
{
    public string Name { get; set; }
    public Tile GetTile(string name) => Tiles.FirstOrDefault(t => t.Name == name);
    public string TileFile(string tileName) => $"tilesets/{Name}/{tileName}.png";

    public long FullEncoding = Tiles
        .Select((t, i) => (t.Symmetry,i))
        .Aggregate(0l, (bit, sym) => bit | (sym.Symmetry.Encoding() << (4 * sym.i)));

    public bool isValidBitSet(long bitset) => (bitset & (~FullEncoding)) == 0;

    public void Validate(long bitset)
    {
        if (!isValidBitSet(bitset)) //|| bitset == 0)
            throw new InvalidOperationException("Invalid bitset");
    }

    public void ValidateIndex(int index) => Validate(1l << index);

    public (Tile Tile, int Rotate) FromIndex(int index)
        => (Tiles[index / 4], index % 4);

    public int ToIndex(string tileName, int rotate)
        => Tiles.FindIndex(t => t.Name == tileName) * 4 + rotate;

    public IEnumerable<(Tile Tile, int Rotate)> FromEncoding(long encoding)
    {
        for (int i = 0; i < Tiles.Count * 4; i++)
        {
            if ((encoding & 1 << i) != 0)
                yield return FromIndex(i);
        }
    }

    // Remarks 貌似抽象层次会让方法以 2^n 上涨，
    // 比如 (string tileName, int Rotate) 到 int encoding 的转换，
    // 这其中还能将第一个 string tileName 转换成 Tile tile
    public Dictionary<int, long[]> IndexToNeighbors = new();

    private void InitNeighborsDict()
    {
        var i = 0;
        foreach (var (name, symmetry) in Tiles)
        {
            var encoding = symmetry.Encoding();
            for (int j = 0; (1 << j & encoding) != 0; j++)
            {
                IndexToNeighbors[i * 4 + j] = new long[4];
            }

            i++;
        }
    }

    public void CalcNeighbors()
    {
        InitNeighborsDict();

        foreach (var name in Tiles.Select(t => t.Name))
        {
            CalcRightNeighbors(name);
        }
    }

    private void AddNeighbor(string centerName, int rotate, int direction, string neighborName, int neighborRotate)
    {
        var index = ToIndex(centerName, rotate);
        ValidateIndex(index);
        if (!IndexToNeighbors.ContainsKey(index)) throw new InvalidOperationException("Index not exists");
        var neighbors4d = IndexToNeighbors[index];
        neighbors4d[direction] |= 1L << ToIndex(neighborName, neighborRotate);
        Validate(neighbors4d[direction]);
    }


    public void CalcRightNeighbors(string name)
    {
        var neighbors = Neighbors.Where(n => n.Left == name);

        (_, var left_sym) = GetTile(name);

        // int[4] neighborsBits, right, up, left, down
        // index to int[4]


        void AddNeighborDirectly(int leftRotate, int direction, string neighborName, int neighborRotate)
            => this.AddNeighbor(name, leftRotate, direction, neighborName, neighborRotate);

        void AddNeighbor(int leftRotate, string rightName, int rightRotate)
        {
            var right_sym = GetTile(rightName).Symmetry;

            void AddWithRotationAndRelative(int lRotate, int rRotate)
            {
                AddNeighborDirectly(lRotate, 0, rightName, rRotate);
                this.AddNeighbor(rightName, rRotate, 2, name, lRotate);

                // 同时加上左侧不同旋转以后，旋转方向上的邻居
                for (int i = 1; i < 4; i++)
                {
                    var cur_left_rotate = left_sym.GetOriginalRotate((lRotate + i) % 4);
                    var cur_right_rotate = right_sym.GetOriginalRotate((rRotate + i) % 4);
                    AddNeighborDirectly(cur_left_rotate, i, rightName, cur_right_rotate);
                    this.AddNeighbor(rightName, cur_right_rotate, (i + 2) % 4, name, cur_left_rotate);
                }
            }
            
            AddWithRotationAndRelative(leftRotate, rightRotate);

            var rotateWithSameRightEdge = left_sym.GetRotateWithSameRightEdge(leftRotate);
            if (rotateWithSameRightEdge != -1)
            {
                AddWithRotationAndRelative(rotateWithSameRightEdge, rightRotate);
            }

            // 求自身旋转以后右侧边缘依然相同的性质
            if (left_sym.IsRightEdgeVerticalSymmetry(leftRotate))
            {
                var v_flip_rotate = right_sym.GetRotateByVerticalFlip(rightRotate);
                if (v_flip_rotate != -1 && v_flip_rotate != leftRotate)
                {
                    AddWithRotationAndRelative(leftRotate, v_flip_rotate);
                }
            }
        }

        // 求不同的旋转角下右侧的邻居
        foreach (var (_, leftRotate, rightName, rightRotate) in neighbors)
        {
            AddNeighbor(leftRotate, rightName, rightRotate);
        }

        // 根据自身的对称性来添加上下左右侧的邻居


        // 将右侧的邻居应用到不同旋转的情况上，在应用的过程中，同时为邻居的邻居数据添加上自己
    }

    public List<(Tile Tile, int Rotate)> GetNeighbors(string curTileName, int rotate = 0, int neighborDir = 0)
    {
        var index = ToIndex(curTileName, rotate);
        if (!IndexToNeighbors.TryGetValue(index, out var list))
            return new();
        var encoding = list[neighborDir];
        return FromEncoding(encoding).ToList();
    }

    public long[,] WaveFunctionCollapse(int width, int height)
    {
        var waveFunctionCollapse = new WaveFunctionCollapse(this, width, height);
        waveFunctionCollapse.Collapse();
        return waveFunctionCollapse.Image;
    }
};