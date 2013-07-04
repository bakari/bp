using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ApplicationCore;
using AvalonDock;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using MainWindowPlugin;
using Microsoft.Win32;

namespace EditorPlugin
{
	/// <summary>
	/// Description of AvalonEditPlugin.
	/// </summary>
	[Export(typeof(Editor))]
	[Export(typeof(IPluginBase))]
	public class Editor : IPluginBase
	{
		MRU mRU;
		[Import(typeof(MainWindow))]
		public MainWindow MainWindow = null;
		
		[Import(typeof(MainMenuPlugin))]
		MainMenuPlugin mainMenuPlugin = null;
		
		public void Load(CompositionContainer container)
		{
			if (MainWindow == null)
				return;
			
			mRU = new MRU(this);
			
			if (!mainMenuPlugin.Loaded)
			{
				mainMenuPlugin.Load(container);
			}
			
			mRU.LoadOpenedTabs();
			if (MainWindow.ActiveTabContent == null) 
				CreateNewTab("Untitled", null);
			MainWindow.Closing += new CancelEventHandler(MainApp_MainWindow_Closing);
			
			mRU.UpdateMRUMenu();
		}
		
		#region Editor
		
		public TextEditor ActiveEditor 
		{
			get 
			{
				if (MainWindow.ActiveTabContent != null)
				{
					return MainWindow.ActiveTabContent.Content as TextEditor;
				}
				return null;
			}
		}
		
		public void CreateNewTab(string title, string filename)
		{
			var documentContent = new DocumentContent();
			documentContent.Title = title;
			
			var editor = new TextEditor();
			editor.Tag = title;
			if (filename != null) 
			{
				try
				{
					editor.Load(filename);
					mRU.AddToMRU(filename);	
				} 
				catch (FileNotFoundException)
				{
					MessageBox.Show("Unable to locate files opened during previous program session.\r\n\r\n" +filename, "Warning", MessageBoxButton.OK, MessageBoxImage.Question);
					return;
				}
				catch (Exception)
				{
					MessageBox.Show("Couldn't open file:\r\n\r\n" +filename, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}
			}
			var b = new Binding("Tag");
			b.Source = documentContent;
			b.Mode = BindingMode.TwoWay;
			documentContent.SetBinding(DocumentContent.InfoTipProperty, b);
			editor.TextChanged += new EventHandler(DocumentContent_Changed);
			editor.FontFamily = new FontFamily("Consolas");
			editor.FontSize = 13;
			editor.ShowLineNumbers = true;
			documentContent.Closing += new EventHandler<CancelEventArgs>(DocumentContent_Closing);
			documentContent.Content = editor;
			documentContent.Tag = filename;
			documentContent.Show(MainWindow.DockManager);
			documentContent.Activate();
			
			foreach (var action in CreationActions)
			{
				action(editor);
			}
		}
		
		
		public string SaveFile()
		{
			var currentFileName = "";
			if (ActiveEditor != null)
			{
				var documentContent = ActiveEditor.Parent as DocumentContent;
			
				if (documentContent.Tag == null) 
				{
					SaveFileDialog dlg = new SaveFileDialog();
					dlg.Filter = "C#Script files (.csx)|*.csx|Text documents (.txt)|*.txt";
					dlg.FileName = (documentContent.Title as string).Trim('*');
					var result = dlg.ShowDialog();
					if (result == true)
					{
						currentFileName = dlg.FileName;
					}
					else
					{
						return null;
					}
				} else
				{
					currentFileName = documentContent.Tag.ToString();
				}
				ActiveEditor.Save(currentFileName);
				ActiveEditor.Tag = currentFileName.Substring(currentFileName.LastIndexOf('\\')+1);
				documentContent.Title =	ActiveEditor.Tag.ToString();
				documentContent.Tag = currentFileName;
				mRU.AddToMRU(currentFileName);
			}
			return currentFileName;
		}
		
		#endregion
		
		#region Event handlers
		
		void DocumentContent_Changed(object sender, EventArgs e)
		{
			var documentContent = (sender as TextEditor).Parent as DocumentContent;
			if (documentContent != null && !documentContent.Title.EndsWith("*"))
			{
				documentContent.Title = documentContent.Title + "*";
			}
		}
		
		void DocumentContent_Closing(object sender, CancelEventArgs e)
		{
			var item = sender as DocumentContent;
			if (item.Title.EndsWith("*"))
			{
				var dResult = MessageBox.Show(string.Concat("Save changes made to file?", 
					Environment.NewLine, 
				    item.Tag != null ? item.Tag : item.Title.Trim('*')),
					"Confirmation",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question);
				if (dResult == MessageBoxResult.Yes)
				{
					SaveFile();
				}
				if (dResult == MessageBoxResult.Cancel)
				{
					e.Cancel = true;	
					return;
				}
			}
		}
		
		void MainApp_MainWindow_Closing(object sender, CancelEventArgs e)
		{
			if (MainWindow != null && MainWindow.TabPane != null)
			{
				foreach (DocumentContent item in MainWindow.TabPane.Items)
				{
					DocumentContent_Closing(item, e);
					if (e.Cancel) break;
				}
				mRU.SaveOpenedTabs();
				
				var contents = MainWindow.DockManager.DockableContents;
				foreach(var content in contents.ToArray())
				{
					content.Close();
				}
			}
		}
		
		// you can add actions made during creation of new editor tab
		List<Action<TextEditor>> actions;
		public List<Action<TextEditor>> CreationActions 
		{
			get
			{
				if (actions == null)
				{
					actions = new List<Action<TextEditor>>();
				}
				return actions;
			}
		}
		#endregion
	}
}