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
using WaveFunctionCollapse;

namespace WFC.GUI;

public class ImageCacheWpf
{
    public Image[] Images;
    public ObservableCollection<Image> ImagesObservable => new(Images);

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
        Images = new Image[tileSet.Tiles.Count * 4];

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
            var image = new Image() { Source = imgSource, Width = 16, Height = 16 };
            Images[ti * 4] = image;


            for (int i = 1; i < symmetry.RotationCount(); i++)
            {
                var index = ti * 4 + i;
                var rotateImage = new BitmapImage();
                rotateImage.BeginInit();
                rotateImage.UriSource = uri;
                rotateImage.Rotation = ToRotateMode(i);
                rotateImage.EndInit();
                var newI = new Image()
                {
                    Source = rotateImage,
                    Width = 16,
                    Height = 16,
                };
                Images[index] = newI;
            }

            ti++;
        }
    }
}