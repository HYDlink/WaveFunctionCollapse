using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WFC.Core;

namespace WFC.GUI;

public class ImageCacheWpf
{
    public ImageSource[] Images;
    public ObservableCollection<ImageSource> ImagesObservable => new(Images);

    public int ImageWidth;
    public int ImageHeight;
    public TileSet TileSet;

    public static Rotation ToRotateMode(int rotate) => rotate switch
    {
        0 => Rotation.Rotate0,
        1 => Rotation.Rotate270,
        2 => Rotation.Rotate180,
        3 => Rotation.Rotate90,
        _ => throw new ArgumentOutOfRangeException(nameof(rotate), rotate, null)
    };

    public ImageCacheWpf(TileSet tileSet)
    {
        TileSet = tileSet;
        Images = new ImageSource[tileSet.Tiles.Count * 4];

        int ti = 0;
        var converter = new ImageSourceConverter();
        foreach (var (name, symmetry) in tileSet.Tiles)
        {
            var imgFilename = tileSet.GetTileFilePath(name);
            var uri = new Uri(imgFilename);
            //var imgSource = converter.ConvertFromString(imgFilename) as ImageSource;
            var imgSource = new BitmapImage(uri);
            if (imgSource == null)
                throw new FileNotFoundException($"ImageSource not found for {imgFilename}");
            Images[ti * 4] = imgSource;


            for (int i = 1; i < symmetry.RotationCount(); i++)
            {
                var index = ti * 4 + i;
                var rotateImage = new BitmapImage();
                rotateImage.BeginInit();
                rotateImage.UriSource = tileSet.IsUnique
                    ? new Uri(tileSet.GetTileFilePath(name, i))
                    : uri;
                rotateImage.Rotation = ToRotateMode(i);
                rotateImage.EndInit();

                Images[index] = rotateImage;
            }

            ti++;
        }
    }
}