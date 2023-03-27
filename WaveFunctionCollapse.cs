using System;
using System.Collections.Generic;
using System.Linq;

public class WaveFunctionCollapse
{
    public long[,] Image;
    public int Width { get; set; }
    public int Height { get; set; }
    public TileSet TileSet { get; set; }

    public WaveFunctionCollapse(TileSet tileSet, int width, int height)
    {
        Width = width;
        Height = height;
        TileSet = tileSet;
        var fullEncoding = tileSet.FullEncoding;
        Image = new long[width, height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Image[j, i] = fullEncoding;
            }
        }
    }

    private Random rand;

    public int CollapseTimes = 0;
    public int PropagateTimes = 0;

    public void Collapse()
    {
        rand = new Random();
        while (!IsObserved(out var x, out var y))
        {
            (x, y) = GetMinEntropyPos();
            CollapseTimes++;
            var formal = Image[x, y];
            var randomIndex = formal.RandomIndex(rand);
            Image[x, y] = 1L << randomIndex;
            Propagate(x, y);

            // 测试
            // if (CollapseTimes > 100000)
            //     return;
        }
    }

    public (int x, int y) GetMinEntropyPos()
    {
        var minX = 0; 
        var minY = 0;
        var minCount = 64;
        
        for (var y = 0; y < Height; ++y)
        for (var x = 0; x < Width; ++x)
        {
            var countOnes = Image[x, y].CountOnes();
            if (countOnes == 2)
                return (x, y);
            if (countOnes > 1 && countOnes < minCount)
            {
                (minX, minY) = (x, y);
                minCount = countOnes;
            }
        }

        return (minX, minY);
    }

    public bool IsObserved(out int x, out int y)
    {
        y = x = 0;
        for (y = 0; y < Height; y++)
        {
            for (x = 0; x < Width; x++)
            {
                var bitSet = Image[x, y];
                if (bitSet != 0 && !bitSet.IsOnlyOneBit())
                    return false;
            }
        }

        return true;
    }

    public void Propagate(int x, int y, int depth = 0)
    {
        if (depth > 1) return;
        PropagateTimes++;
        var i = Image[x, y];
        var allIndex = i.GetAllIndex().ToList();

        long GetNeighborByDir(int dir)
            => allIndex.Aggregate(0L, (bit, index) => bit | TileSet.IndexToNeighbors[index][dir]);

        List<(int X, int Y)> toPropagate = new();

        void PropagateAt(int newX1, int newY1, long neighbors1)
        {
            var formal = Image[newX1, newY1];
            var @new = formal & neighbors1;
            if (formal != @new)
            {
                toPropagate.Add((newX: newX1, newY: newY1));
                Image[newX1, newY1] = @new;
                TileSet.Validate(@new);
            }
        }

        if (x > 0)
        {
            // 左侧
            var neighbors = GetNeighborByDir(2);
            var (newX, newY) = (x - 1, y);
            PropagateAt(newX, newY, neighbors);
        }

        if (x < Width - 1)
        {
            // 右侧
            var neighbors = GetNeighborByDir(0);
            var (newX, newY) = (x + 1, y);
            PropagateAt(newX, newY, neighbors);
        }

        if (y > 0)
        {
            // 上册
            var neighbors = GetNeighborByDir(1);
            var (newX, newY) = (x, y - 1);
            PropagateAt(newX, newY, neighbors);
        }

        if (y < Height - 1)
        {
            // 下
            var neighbors = GetNeighborByDir(3);
            var (newX, newY) = (x, y + 1);
            PropagateAt(newX, newY, neighbors);
        }

        if (toPropagate.Any())
        {
            foreach (var (x1, y1) in toPropagate)
            {
                Propagate(x1, y1, depth + 1);
            }
        }
    }
}