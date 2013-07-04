using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using ApplicationCore;

namespace ExplorerPlugin
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance =
            new HeaderToImageConverter();

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
        	switch ((DirectoryTreeItem.TYPE) value)
        	{
        		case DirectoryTreeItem.TYPE.Info: 
        			return ImageProvider.Image_search;
        		
        		case DirectoryTreeItem.TYPE.Folder:
        			return ImageProvider.Image_folder;
        			
        		case DirectoryTreeItem.TYPE.Drive:
        			return ImageProvider.Image_diskdrive;
        		
        		case DirectoryTreeItem.TYPE.File:
        			return ImageProvider.Image_file;
        		
        		default: 
        			return ImageProvider.Image_file;
        	}
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
