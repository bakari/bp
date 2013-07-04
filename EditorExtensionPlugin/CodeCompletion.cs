
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using ApplicationCore;
using AvalonDock;
using CSharpEditor;
using EditorPlugin;
using Framework.Core;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using MainWindowPlugin;

namespace EditorExtensionPlugin
{
	/// <summary>
	/// CodeCompletionPlugin.
	/// </summary>
	[Export(typeof(IPluginBase))]
	public class CodeCompletionPlugin : IPluginBase
	{
		ParseInformation parseInformation;
		DefaultProjectContent myProjectContent;
		ProjectContentRegistry pcRegistry;
		ICompilationUnit lastCompilationUnit;
		Thread parserThread;
		CompositionContainer container;
		
		[Import(typeof(MainWindow))]
		MainWindow mainWindow = null;
		
		[Import(typeof(Editor))]
		Editor editorPlugin = null;
		
		public CodeCompletionPlugin()
		{
			pcRegistry = new ProjectContentRegistry();
			pcRegistry.ActivatePersistence(Path.Combine(Path.GetTempPath(), "CSharpCodeCompletion"));
			
			myProjectContent = new DefaultProjectContent();
			myProjectContent.Language = LanguageProperties.CSharp;
			parseInformation = new ParseInformation(new DefaultCompilationUnit(myProjectContent));
		}
		
		public void Load(CompositionContainer container)
		{
			this.container = container;
			if (editorPlugin != null || mainWindow != null)
			{
				editorPlugin.CreationActions.Add(InstallCodeCompletion);
				
				if (mainWindow != null)
				{
					mainWindow.Closing += new CancelEventHandler(MainApp_WindowClosing);
				
					var tabPane = mainWindow.TabPane;
					if (tabPane != null)
					{
						foreach (var content in tabPane.Items)
						{
							var editor = (content as DocumentContent).Content as TextEditor;
							if (editor != null) 
							{
								InstallCodeCompletion(editor);
							}
						}
						parserThread = new Thread(ParserThread);
						parserThread.IsBackground = true;		
						parserThread.Start();
					}
				}
			}
			else
			{
				MessageBox.Show("Required plugin not found: \r\n\r\nEditor plugin", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		
		#region Code completion
		bool canParse = true;
		
		void ParserThread()
		{
			myProjectContent.AddReferencedContent(pcRegistry.Mscorlib);
			
			// do one initial parser step to enable code-completion while other
			// references are loading
			ParseStep();
			
			string[] referencedAssemblies = {
				"System", "System.Data", "System.Drawing", "System.Xml", "System.Windows.Forms", "Microsoft.VisualBasic"
			};
			foreach (var assemblyName in referencedAssemblies) 
			{
				var assemblyNameCopy = assemblyName; // copy for anonymous method
				var referenceProjectContent = pcRegistry.GetProjectContentForReference(assemblyName, assemblyName);
				myProjectContent.AddReferencedContent(referenceProjectContent);
				if (referenceProjectContent is ReflectionProjectContent) 
					(referenceProjectContent as ReflectionProjectContent).InitializeReferences();
			}
			
			// Parse the current file every 2 seconds
			while (canParse) 
			{
				ParseStep();
				Thread.Sleep(2000);
			}
		}
		
		void ParseStep()
		{
			string code = null;
			Action action = () => code = editorPlugin.ActiveEditor != null ? editorPlugin.ActiveEditor.Text : "";
			try {App.Current.Dispatcher.Invoke(DispatcherPriority.Render, action);} catch(Exception){}
			var textReader = new StringReader(code);
			ICompilationUnit newCompilationUnit;
	
			using (IParser p = ParserFactory.CreateParser(SupportedLanguage.CSharp, textReader)) 
			{
				p.ParseMethodBodies = true;
				p.Parse();
				newCompilationUnit = ConvertCompilationUnit(p.CompilationUnit);
			}
			// edited
			myProjectContent.UpdateCompilationUnit(lastCompilationUnit, newCompilationUnit, "edited.cs");
			lastCompilationUnit = newCompilationUnit;
			parseInformation = new ParseInformation(newCompilationUnit);
		}
		
		ICompilationUnit ConvertCompilationUnit(CompilationUnit cu)
		{
			var converter = new NRefactoryASTConvertVisitor(myProjectContent, SupportedLanguage.CSharp);
			cu.AcceptVisitor(converter, null);
			return converter.Cu;
		}
		
		ExpressionResult FindExpression(ICSharpCode.AvalonEdit.Editing.TextArea textArea)
		{
			var finder = new CSharpExpressionFinder(parseInformation);
			var expression = finder.FindExpression(textArea.Document.Text, textArea.Caret.Offset - 1);
			if (expression.Region.IsEmpty) 
				expression.Region = new DomRegion(textArea.Caret.Line + 1, textArea.Caret.Column + 1);
			return expression;
		}
		
		void AddCompletionData(IList<ICompletionData> resultList, IEnumerable<ICompletionEntry> completionData)
		{
			// stores overloads
			var nameDictionary = new Dictionary<string, CodeCompletionData>();
			
			// edited cycle
			foreach (object obj in completionData) 
			{
				if (obj is NamespaceEntry) 
				{
					resultList.Add(new CodeCompletionData(obj.ToString(), "namespace " + obj));
				} 
				else if (obj is ICSharpCode.SharpDevelop.Dom.NRefactoryResolver.KeywordEntry)
				{
					var k = (KeywordEntry) obj;
					resultList.Add(new CodeCompletionData(k.Name));
				}
				else if (obj is IClass) 
				{
					var c = (IClass) obj;
					resultList.Add(new CodeCompletionData(c));
				} 
				else if (obj is IMember) 
				{
					var m = (IMember) obj;
					if (m is IMethod && ((m as IMethod).IsConstructor)) 
					{
						// skip constructors
						continue;
					}
					CodeCompletionData data;
					if (nameDictionary.TryGetValue(m.Name, out data)) 
					{
						data.AddOverload();
					}
					else 
					{
						nameDictionary[m.Name] = data = new CodeCompletionData(m);
						resultList.Add(data);
					}
				}
			}
		}
		#endregion
		
		#region Code completion event handlers	
		void TextArea_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				e.Handled = true;
				TextArea_TextEntering(null, null);
			}
		}
		
		CompletionWindow completionWindow;
		
		void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (completionWindow != null && completionWindow.CompletionList.ListBox.Items.Count == 0)
				completionWindow.Close();
			var activeEditor = editorPlugin.ActiveEditor;
			if (activeEditor != null) 
			{
				// code completion
				if (e.Text == ".") 
				{
					completionWindow = new CompletionWindow(activeEditor.TextArea);
					var resultList = completionWindow.CompletionList.CompletionData;
	
					var resolver = new NRefactoryResolver(myProjectContent.Language);
					var expression = FindExpression(activeEditor.TextArea);
					var rr = resolver.Resolve(expression, parseInformation, activeEditor.Text);
					if (rr != null) 
					{
						var completionData = rr.GetCompletionData(myProjectContent, true);
						if (completionData != null) 
							AddCompletionData(resultList, completionData);
					}			                 
					if (resultList.IsEmpty()) 
					{
						completionWindow = null;
						return;
					}
					completionWindow.Show();
					completionWindow.Closed += delegate { completionWindow = null; };
				}
				// brace completion
				else if (e.Text == "\n" || e.Text == "\r")
				{
					if (Regex.Matches(activeEditor.Text, "{").Count - Regex.Matches(activeEditor.Text, "}").Count != 0)
					{
						var offset = activeEditor.CaretOffset-1;
						while (activeEditor.Text.At(offset).AnyOf("\n\r\t ".ToCharArray()))
						{
							offset -= 1;
						}
						if (offset != -1 && activeEditor.Document.GetCharAt(offset) == '{')
						{
							var caret = activeEditor.CaretOffset;
							var line = activeEditor.Text.Sub(activeEditor.Document.GetLineByOffset(offset).Offset, offset-activeEditor.Document.GetLineByOffset(offset).Offset);
							var c = line.Length - line.Trim('\t').Length;
							activeEditor.Document.Insert(caret,"\r\n" +line.Sub(0,c) +"}");
							activeEditor.TextArea.Caret.Offset = caret;
						}
					}
				}
				// brace indentation
				else if (e.Text == "{")
				{
					var line = activeEditor.Document.GetLineByOffset(activeEditor.CaretOffset).PreviousLine;
					var c = 0;
					if (line != null)
					{
						var t1 = activeEditor.Text.Sub(line.Offset, line.Length);
						var m = Regex.Match(t1, @"([^\t]+)\t*");
						var match = m.Success ? m.Value : "";
						var t2 = activeEditor.Text.Sub(line.NextLine.Offset, activeEditor.CaretOffset-1);
						if (t2.Sub(0,t2.IndexOf('{')).Trim().Length == 0) 
							c = t2.Length - t2.Trim('\t').Length - t1.Sub(0,t1.IndexOf(match)).Length;
					}
					if (c > 0) 
						activeEditor.Document.Remove(activeEditor.Document.GetLineByOffset(activeEditor.CaretOffset).Offset, c);
					else
					{
						while (c!=0) 
						{
							activeEditor.Document.Insert(activeEditor.Document.GetLineByOffset(activeEditor.CaretOffset).Offset, "\t");
							c++;
						}
					}
				}
			}
		}
		
		void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			if ((sender == null || e.Text.Length > 0 && char.IsLetterOrDigit(e.Text[0])) && completionWindow == null)
			{
				var activeEditor = editorPlugin.ActiveEditor;
				if (activeEditor == null || char.IsLetterOrDigit(activeEditor.Text.At(activeEditor.CaretOffset-1))) return;
				
				completionWindow = new CompletionWindow(activeEditor.TextArea);
				var resultList = completionWindow.CompletionList.CompletionData;
				var resolver = new NRefactoryResolver(myProjectContent.Language);
				
				var finder = new CSharpExpressionFinder(parseInformation);
				var expression = finder.FindFullExpression(activeEditor.Text, activeEditor.CaretOffset);
				var rr = resolver.CtrlSpace(activeEditor.TextArea.Caret.Line, activeEditor.TextArea.Caret.Column, parseInformation, activeEditor.Text, expression.Context, true);
				if (rr != null) 
				{
					var completionData = rr;
					if (completionData != null) 
					{
						AddCompletionData(resultList, completionData);
					}
				}
				
				if (resultList.IsEmpty()) 
				{
					completionWindow = null;
					return;
				}
				completionWindow.Show();
				completionWindow.Closed += delegate { completionWindow = null; };
			}
		}
		
		public void MainApp_WindowClosing(object sender, CancelEventArgs e)
		{
			canParse = false;
		}
		#endregion
		
		public void InstallCodeCompletion(TextEditor editor)
		{
			editor.TextArea.TextEntered += new TextCompositionEventHandler(TextArea_TextEntered);
			editor.TextArea.TextEntering += new TextCompositionEventHandler(TextArea_TextEntering);
			editor.TextArea.KeyDown += new KeyEventHandler(TextArea_KeyDown);
		}
	}
}
