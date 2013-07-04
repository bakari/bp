
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using ApplicationCore;
using AvalonDock;
using AvalonEdit.Sample;
using EditorPlugin;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using MainWindowPlugin;

namespace EditorExtensionPlugin
{
	/// <summary>
	/// Folding strategy, indentation and syntax highlighting plugin
	/// </summary>
	[Export(typeof(IPluginBase))]
	public class FoldingStrategy : IPluginBase
	{
		FoldingManager foldingManager;
		AbstractFoldingStrategy foldingStrategy;
		
		[Import(typeof(Editor))]
		Editor editorPlugin = null;
		
		[Import(typeof(MainWindow))]
		MainWindow mainWindow = null;
		
		public void Load(CompositionContainer container)
		{
			if (editorPlugin == null || mainWindow == null)
				return;
			
			editorPlugin.CreationActions.Add(InstallFolding);
			editorPlugin.CreationActions.Add(InstallIndentation);
			editorPlugin.CreationActions.Add(HighlightSyntax);
			foldingStrategy = new BraceFoldingStrategy();
			var foldingUpdateTimer = new DispatcherTimer();
			foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
			foldingUpdateTimer.Tick += FoldingUpdateTimer_Tick;
			foldingUpdateTimer.Start();
			
			var tabPane = mainWindow.TabPane;
			if (tabPane != null)
			{
				foreach (var content in tabPane.Items)
				{
					var editor = (content as DocumentContent).Content as TextEditor;
					if (editor != null) 
					{
						InstallIndentation(editor);
						InstallFolding(editor);
						HighlightSyntax(editor);
					}
				}
			}
		}
		
		void FoldingUpdateTimer_Tick(object sender, EventArgs e)
		{
			if (foldingManager != null && foldingStrategy != null && editorPlugin.ActiveEditor != null) {
				foldingStrategy.UpdateFoldings(foldingManager, editorPlugin.ActiveEditor.Document);
			}
		}
		
		public void HighlightSyntax(TextEditor editor)
		{
			editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
		}
		
		public void InstallFolding(TextEditor editor)
		{
			foldingManager = FoldingManager.Install(editor.TextArea);
			foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
		}
		
		public void InstallIndentation(TextEditor editor)
		{
			editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(editor.Options);
		}
	}
}
