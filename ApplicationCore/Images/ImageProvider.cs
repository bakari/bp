using System;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApplicationCore
{
	/// <summary>
	/// Image provider
	/// </summary>
	public class ImageProvider
	{
		public static Uri Uri_error = new Uri("pack://application:,,,/Images/Error.png");
		public static ImageSource Image_error = new BitmapImage(new Uri(("pack://application:,,,/Images/Error.png")));
		public static ImageSource Image_errors = new BitmapImage(new Uri(("pack://application:,,,/Images/Errors.png")));
		public static ImageSource Image_output = new BitmapImage(new Uri(("pack://application:,,,/Images/Output.png")));
		public static ImageSource Image_search = new BitmapImage(new Uri(("pack://application:,,,/Images/Iconsearch.png")));
		public static ImageSource Image_folder = new BitmapImage(new Uri(("pack://application:,,,/Images/Folder.png")));
		public static ImageSource Image_diskdrive = new BitmapImage(new Uri(("pack://application:,,,/Images/Diskdrive.png")));
		public static ImageSource Image_file = new BitmapImage(new Uri(("pack://application:,,,/Images/File.png")));
		public static ImageSource Image_namespace = new BitmapImage(new Uri(("pack://application:,,,/Images/Namespace.png")));
		public static ImageSource Image_method = new BitmapImage(new Uri(("pack://application:,,,/Images/Method.png")));
		public static ImageSource Image_property = new BitmapImage(new Uri(("pack://application:,,,/Images/Property.png")));
		public static ImageSource Image_field = new BitmapImage(new Uri(("pack://application:,,,/Images/Field.png")));
		public static ImageSource Image_enum = new BitmapImage(new Uri(("pack://application:,,,/Images/Enum.png")));
		public static ImageSource Image_class = new BitmapImage(new Uri(("pack://application:,,,/Images/Class.png")));
		public static ImageSource Image_event = new BitmapImage(new Uri(("pack://application:,,,/Images/Event.png")));
		public static ImageSource Image_keyword = new BitmapImage(new Uri(("pack://application:,,,/Images/Keyword.png")));
	}
}
