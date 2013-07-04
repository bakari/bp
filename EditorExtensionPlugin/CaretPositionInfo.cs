
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using ApplicationCore;
using AvalonDock;
using EditorPlugin;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using MainWindowPlugin;

namespace EditorExtensionPlugin
{
	/// <summary>
	/// CaretPositionInfo plugin
	/// </summary>
	[Export(typeof(IPluginBase))]
	public class CaretPositionInfo : IPluginBase
	{
		[Import(typeof(MainWindow))]
		MainWindow mainWindow = null;
		
		[Import(typeof(Editor))]
		Editor editorPlugin= null;
		
		TextBlock textblock;
		
		public void Load(CompositionContainer container)
		{
			if (mainWindow == null || editorPlugin == null)
				return;
			
			var statusItem = new StatusBarItem();
			textblock = new TextBlock();
			statusItem.HorizontalAlignment = HorizontalAlignment.Right;
			statusItem.Content = textblock;
			mainWindow.StatusBar.Items.Add(statusItem);
			
			foreach (var content in mainWindow.TabPane.Items)
			{
				var editor = (content as DocumentContent).Content as TextEditor;
				if (editor != null) 
				{
					InstallCaretPosition(editor);
				}
			}
			editorPlugin.CreationActions.Add(InstallCaretPosition);
		}
		
		public void InstallCaretPosition(TextEditor editor)
		{
			editor.GotFocus += new RoutedEventHandler(editor_GotFocus);
			editor.TextArea.Caret.PositionChanged += new EventHandler(editor_TextArea_Caret_PositionChanged);
		}

		void editor_GotFocus(object sender, RoutedEventArgs e)
		{
			var editor = sender as TextEditor;
			if (editor == null) 
				return;
			editor_TextArea_Caret_PositionChanged(editor.TextArea.Caret, null);
		}

		void editor_TextArea_Caret_PositionChanged(object sender, EventArgs e)
		{
			var caret = sender as Caret;
			if (caret == null) 
				return;
			textblock.Text = string.Concat("Ln: ", caret.Line.ToString(), "  Col: ", caret.Column, "	");
		}
	}
}
