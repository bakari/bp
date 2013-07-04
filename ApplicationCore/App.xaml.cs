using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

namespace ApplicationCore
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		[Import(typeof(Window), AllowDefault = true)]
		Lazy<Window> MW = null;
		
		[ImportMany(typeof(IPluginBase), AllowRecomposition=true)]
		Lazy<IPluginBase>[] plugins = null;
		
		void Application_Startup(object sender, StartupEventArgs e)
		{
			if (Compose())
			{
				foreach (var plugin in plugins)
		      	{
					plugin.Value.Load(container);
				}
			}
			else
			{
				Shutdown();
			}
		}
		
		public CompositionContainer container;
		
		bool Compose()
		{
			var catalog = new AggregateCatalog();
			catalog.Catalogs.Add(new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly()));
			catalog.Catalogs.Add(new DirectoryCatalog(".", "*.Plugin.dll"));
				
			container = new CompositionContainer(catalog);
			
			container.ComposeParts(this);
			MainWindow = MW.Value;
			MainWindow.Show();
			
			return true;
		}
	}
}