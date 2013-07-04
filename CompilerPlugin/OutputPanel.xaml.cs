
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EditorPlugin;

namespace CompilerPlugin
{
	/// <summary>
	/// Interaction logic for OutputPanel.xaml
	/// </summary>
	public partial class OutputPanel : UserControl
	{
		protected Editor editorPlugin;
		
		public OutputPanel()
		{
			InitializeComponent();
		}
		
		public OutputPanel(Editor editorPlugin)
		{
			InitializeComponent();
			this.editorPlugin = editorPlugin;
		}
		
		void ErrorListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try 
			{
				var lineNumber = int.Parse((((ListViewItem) sender).Content as ErrorData).Line);
				var activeEditor = editorPlugin.ActiveEditor;
				if (activeEditor != null) activeEditor.ScrollToLine(lineNumber);
			}
			catch(Exception) {}
		}
	}
}