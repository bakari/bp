
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApplicationCore;
using AvalonDock;
using EditorPlugin;
using ICSharpCode.AvalonEdit;
using MainWindowPlugin;

namespace CompilerPlugin
{
	/// <summary>
	/// Description of CompilerPlugin.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[Export(typeof(CompilerPlugin))]
	public class CompilerPlugin : IPluginBase
	{
		[Import(typeof(MainWindow))]
		MainWindow mainWindow = null;
		
		[Import(typeof(Editor))]
		Editor editorPlugin = null;
		
		DockingManager dockManager;
		TextBox textBox;
		OutputPanel errorPanel;
		DockableContent errorsContent;
		DockableContent outputContent;
		
		public void Load(CompositionContainer container)
		{
			if (mainWindow == null || editorPlugin == null)
				return;
			dockManager = mainWindow.DockManager;
			if (dockManager != null) 
				InitOutputPane();
		}
		
		public void InitOutputPane()
		{
			// Output DockablePane
			var pane = new DockablePane();
			
			// Errors DockableContent
			errorsContent = new DockableContent();
			errorsContent.Name = "errorsContent";
			errorsContent.Title = "Errors";
			errorsContent.Icon = ApplicationCore.ImageProvider.Image_errors;
			errorPanel = new OutputPanel(editorPlugin);
			errorsContent.Content = errorPanel;
			
			// Output DockableContent
			outputContent = new DockableContent();
			outputContent.Name = "outputContent";
			outputContent.Title = "Output";
			outputContent.Icon = ApplicationCore.ImageProvider.Image_output;
			var sv = new ScrollViewer();
			textBox = new TextBox();
			textBox.AcceptsReturn = true;
			textBox.IsReadOnly = true;
			textBox.TextWrapping = TextWrapping.Wrap;
			textBox.Name = "outputTextBox";
			sv.Content = textBox;
			outputContent.Content = sv;
			
			ResizingPanel.SetResizeHeight(pane, new GridLength(150));
			pane.Items.Add(errorsContent);
			pane.Items.Add(outputContent);
			
			var centerResizingPanel = mainWindow.TabPane.Parent as ResizingPanel;
			if (centerResizingPanel != null) 
			{
				centerResizingPanel.Children.Add(pane);
			}
			
			errorsContent.Hide();
			outputContent.Hide();
		}
		
		public void ClearOutput()
		{
			if (textBox != null) textBox.Clear();
		}
		
		public void ClearErrors()
		{
			if (errorPanel != null) errorPanel.errorListView.Items.Clear();
		}
		
		public void AddOutput(string text)
		{
			if (!string.IsNullOrEmpty(text) && textBox != null)
			{
				textBox.AppendText(text +Environment.NewLine);
				ShowErrorPane();
				ShowOutputPane();
				if (editorPlugin.ActiveEditor != null) 
					editorPlugin.ActiveEditor.Focus();
			}
		}
		
		public void AddError(ErrorData e)
		{
			if (errorPanel == null)
				return;
			var listView = errorPanel.errorListView;
			if (listView != null) 
			{
				listView.Items.Add(e);
				ShowOutputPane();
				ShowErrorPane();
				if (editorPlugin.ActiveEditor != null) 
					editorPlugin.ActiveEditor.Focus();
			}
		}
		
		public void ShowOutputPane()
		{
			if (outputContent != null)
				outputContent.Show(dockManager);
		}
		
		public void ShowErrorPane()
		{
			if (errorsContent != null)
				errorsContent.Show(dockManager);
		}
	}
}
