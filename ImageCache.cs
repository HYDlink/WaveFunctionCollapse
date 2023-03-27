using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

public class ImageCache
{
    public Image<Bgra32>[] OriginalImage;
    public Image<Bgra32>[] EnlargedImage;


    public static RotateMode ToRotateMode(int rotate) => rotate switch
    {
        0 => RotateMode.None,
        1 => RotateMode.Rotate270,
        2 => RotateMode.Rotate180,
        3 => RotateMode.Rotate90,
        _ => throw new ArgumentOutOfRangeException(nameof(rotate), rotate, null)
    };

    public int ImageWidth;
    public int ImageHeight;

    public ImageCache(TileSet tileSet)
    {
        OriginalImage = new Image<Bgra32>[tileSet.Tiles.Count * 4];
        EnlargedImage = new Image<Bgra32>[tileSet.Tiles.Count * 4];

        int ti = 0;
        foreach (var (name, symmetry) in tileSet.Tiles)
        {
            var imgFilename = tileSet.TileFile(name);
            var image = Image.Load<Bgra32>(imgFilename);
            (ImageWidth, ImageHeight) = (image.Width, image.Height);
            var (lw, lh) = (ImageWidth * 4, ImageHeight * 4);

            var enlarged = image.Clone();
            enlarged.Mutate(c => c.Resize(lw, lh, new NearestNeighborResampler()));

            OriginalImage[ti * 4] = image;
            EnlargedImage[ti * 4] = enlarged;


            for (int i = 1; i < symmetry.RotationCount(); i++)
            {
                var index = ti * 4 + i;
                var newI = image.Clone();
                var rotate = ToRotateMode(i);
                newI.Mutate(x => x.Rotate(rotate));
                var rEnlarged = newI.Clone();
                rEnlarged.Mutate(c => c.Resize(lw, lh, new NearestNeighborResampler()));
                OriginalImage[index] = newI;
                EnlargedImage[index] = rEnlarged;
            }

            ti++;
        }
    }

    public Image<Bgra32> Draw(long[,] collapsedData)
    {
        var width = collapsedData.GetLength(0);
        var height = collapsedData.GetLength(1);

        var FullImage = ImageWidth * 4 * width;
        var image = new Image<Bgra32>(ImageWidth * 4 * width, ImageHeight * 4 * height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var ic = collapsedData[x, y];
                if (ic.IsOnlyOneBit())
                {
                    var point = new Point(ImageWidth * 4 * x, ImageHeight * 4 * y);
                    image.Mutate(n =>
                        n.DrawImage(EnlargedImage[ic.ToIndex()], point, 1f));
                }
                else
                {
                    var image_count = 0;
                    foreach (var index in ic.GetAllIndex())
                    {
                        if (image_count >= 15) break;
                        var px = ImageWidth * 4 * x + ImageWidth * (image_count % 4);
                        var py = ImageHeight * 4 * y + ImageHeight * (image_count / 4);
                        image_count++;
                        image.Mutate(ctx => ctx.DrawImage(OriginalImage[index], new Point(px, py), 1f));
                    }
                }
            }
        }

        return image;
    }
}