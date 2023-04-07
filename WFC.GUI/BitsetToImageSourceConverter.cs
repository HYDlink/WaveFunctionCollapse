using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WaveFunctionCollapse;
using YamlDotNet.Core.Tokens;

namespace WFC.GUI;

public class ImageList : List<ImageSource> { }

public class BitsetToImageSourceConverter: IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [long bitset and > 0, ImageCacheWpf cache] )
        {
            if (bitset.IsOnlyOneBit())
            {
                return cache.Images[bitset.ToIndex()];
            }
            else
            {
                var imageList = new ImageList();
                var imageSources = bitset.GetAllIndex().Select(i => cache.Images[i]);
                imageList.AddRange(imageSources);
                return imageList;
            }
        }

        return Binding.DoNothing;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}