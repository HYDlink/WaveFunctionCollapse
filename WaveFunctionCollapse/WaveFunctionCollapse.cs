using System;
using System.Collections.Generic;
using System.Linq;

namespace WFC.Core;

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

        Reset();
    }

    private Random rand;

    public int CollapseTimes = 0;
    public int PropagateTimes = 0;
    private Stack<long[,]> Stack { get; set; }

    public void Reset()
    {
        var fullEncoding = TileSet.FullEncoding;
        Image = new long[Height, Width];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Image[y, x] = fullEncoding;
            }
        }

        rand = new Random();
        Stack = new Stack<long[,]>();
    }

    public void CollapseByRandomIndex(int x, int y)
    {
        var formal = Image[y, x];
        var randomIndex = formal.RandomIndex(rand);
        Collapse(x, y, randomIndex);
    }

    public void Collapse(int x, int y, int index)
    {
        Stack.Push((long[,])Image.Clone());
        Image[y, x] = 1L << index;
        CollapseTimes++;
        if (!Propagate(x, y))
        {
            BackTrack(1);
        }
    }


    public void CollapseRandomToEnd()
    {
        var (firstX, firstY) = (rand.Next(Width), rand.Next(Height));
        CollapseByRandomIndex(firstX, firstY);

        while (!IsObserved(out var x, out var y))
        {
            CollapseRandomNext();
            // 测试
            // if (CollapseTimes > 100000)
            //     return;
        }

        Console.WriteLine($"CollapseByRandomIndex Times for \"{TileSet.Name}\": {CollapseTimes}");
    }

    public void CollapseRandomNext(int steps)
    {
        for (int i = 0; i < steps && !IsObserved(out _, out _); i++)
        {
            var (x, y) = GetMinEntropyPos();

            CollapseByRandomIndex(x, y);
        }
    }

    public void CollapseRandomNext()
    {
        var (x, y) = GetMinEntropyPos();

        CollapseByRandomIndex(x, y);
    }
    public void BackTrack(int steps)
    {
        long[,] popedItem = null;
        for (int i = 0; i < steps && Stack.Any(); i++)
        {
            popedItem = Stack.Pop();
        }
        Image = (long[,])popedItem.Clone();
    }

    public (int x, int y) GetMinEntropyPos()
    {
        var minX = 0;
        var minY = 0;
        var minCount = 64;

        for (var y = 0; y < Height; ++y)
            for (var x = 0; x < Width; ++x)
            {
                var countOnes = Image[y, x].CountOnes();
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
                var bitSet = Image[y, x];
                if (bitSet != 0 && !bitSet.IsOnlyOneBit())
                    return false;
            }
        }

        return true;
    }

    public bool Propagate(int x, int y, int depth = 0)
    {
        // if (depth > 1) return true;
        PropagateTimes++;
        var i = Image[y, x];
        var allIndex = i.GetAllIndex().ToList();

        long GetNeighborByDir(int dir)
            => allIndex.Aggregate(0L, (bit, index) => bit | TileSet.IndexToNeighbors[index][dir]);

        List<(int X, int Y)> toPropagate = new();

        bool PropagateAt(int newX1, int newY1, long neighbors1)
        {
            var formal = Image[newY1, newX1];
            var @new = formal & neighbors1;
            if (@new == 0) return false;
            if (formal != @new)
            {
                toPropagate.Add((newX: newX1, newY: newY1));
                Image[newY1, newX1] = @new;
                TileSet.Validate(@new);
            }

            return true;
        }

        if (x > 0)
        {
            // 左侧
            var neighbors = GetNeighborByDir(2);
            var (newX, newY) = (x - 1, y);
            if (!PropagateAt(newX, newY, neighbors)) return false;
        }

        if (x < Width - 1)
        {
            // 右侧
            var neighbors = GetNeighborByDir(0);
            var (newX, newY) = (x + 1, y);
            if (!PropagateAt(newX, newY, neighbors)) return false;
        }

        if (y > 0)
        {
            // 上册
            var neighbors = GetNeighborByDir(1);
            var (newX, newY) = (x, y - 1);
            if (!PropagateAt(newX, newY, neighbors)) return false;
        }

        if (y < Height - 1)
        {
            // 下
            var neighbors = GetNeighborByDir(3);
            var (newX, newY) = (x, y + 1);
            if (!PropagateAt(newX, newY, neighbors)) return false;
        }

        if (toPropagate.Any())
        {
            foreach (var (x1, y1) in toPropagate)
            {
                if (!Propagate(x1, y1, depth + 1)) return false;
            }
        }

        return true;
    }
}