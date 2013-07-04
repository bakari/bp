using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using ApplicationCore;
using AvalonDock;

namespace MainWindowPlugin
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	[Export(typeof(Window))]
	[Export(typeof(MainWindow))]
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			
			// Window preferences
			var winPrefs = new WindowPreferences();
			Height = winPrefs.windowHeight;
			Width = winPrefs.windowWidth;
			Top = winPrefs.windowTop;
			Left = winPrefs.windowLeft;
			WindowState = winPrefs.windowState;
			
			var duration = new Duration(TimeSpan.FromSeconds(0));
			var doubleAnimation = new DoubleAnimation(200.0, duration);
			progressBar.BeginAnimation(ProgressBar.ValueProperty, doubleAnimation);
		}
		
		void Window_Closing(object sender, CancelEventArgs e)
		{
			var winPrefs = new WindowPreferences();
			winPrefs.windowTop = Top;
			winPrefs.windowLeft = Left;
			winPrefs.windowWidth = Width;
			winPrefs.windowHeight = Height;
			winPrefs.windowState = WindowState;
			winPrefs.Save();
		}
		
		void DockManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (dockManager.Content != null)
			{
				ShowProgressBar(false);
				SetStatus("Ready");
			}
		}

		public void SetStatus(string s)
		{
			if (!string.IsNullOrEmpty(s))
				statusLabel.Text = s;
		}
		
		public void ShowProgressBar(bool b)
		{
			if (b)
				progressBar.Visibility = Visibility.Visible;
			else
				progressBar.Visibility = Visibility.Collapsed;
		}
		
		public ManagedContent ActiveTabContent
		{
			get 
			{
				return tabPane.SelectedItem as ManagedContent;
			}
		}
		
		public DockingManager DockManager
		{
			get
			{
				return dockManager;
			}
		}
		
		public Menu Menu
		{
			get 
			{
				return mainMenu;
			}
		}
		
		public DocumentPane TabPane
		{
			get
			{
				return tabPane;
			}
		}
		
		public StatusBar StatusBar
		{
			get	
			{
				return statusBar;
			}
		}
    }
}