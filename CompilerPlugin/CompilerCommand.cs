
using System;
using System.CodeDom.Compiler;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MainWindowPlugin;
using ApplicationCore;
using AvalonDock;
using EditorPlugin;
using Framework.Core;
using ICSharpCode.ILSpy;
using ICSharpCode.AvalonEdit.Rendering;

namespace CompilerPlugin
{
	/// <summary>
	/// Description of CompilerCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_Build", MenuIcon = "Images/Compile.png", Header = "_Run", MenuCategory = "1", MenuOrder = 4.1, CommandHotKey = System.Windows.Input.Key.F5)]
	[ExportToolbarCommand(ToolTip = "Run compiled exe", ToolbarIcon = "Images/Compile.png", ToolbarCategory = "4", ToolbarOrder = 4.1)]
	public class CompilerCommand : SimpleCommand
	{
		protected bool includeDebugInformation;
		bool flag;
		
		[Import(typeof(CompilerPlugin))]
		CompilerPlugin compilerPlugin = null;
		
		public CompilerCommand()
		{
			includeDebugInformation = true;
			flag = true;
		}
		
		public override void Execute(object parameter)
		{
			flag = false;
			
			if (editorPlugin == null || mainWindow == null || compilerPlugin == null || editorPlugin.ActiveEditor == null)
				return;
			
			var editor = editorPlugin.ActiveEditor;
			// set status, clear underlines, clear output
			mainWindow.SetStatus("Building...");
			foreach (var renderer in (from IBackgroundRenderer r in editor.TextArea.TextView.BackgroundRenderers where r is UnderlineBackgroundRenderer select r).ToList())
			{
				editor.TextArea.TextView.BackgroundRenderers.Remove(renderer);
			}
			compilerPlugin.ClearErrors();
			compilerPlugin.ClearErrors();
			
			// save file
			var file = editorPlugin.SaveFile();
					
			if (string.IsNullOrEmpty(file) || !File.Exists(file))
			{
				return;
			}
			
			// compile
			var results = ExeBuilder.CompileCSharpFromFile(file + (includeDebugInformation ? ".debug.exe" : ".exe"), includeDebugInformation, file);
			
			var ubr = new UnderlineBackgroundRenderer(editor);
			editor.TextArea.TextView.BackgroundRenderers.Add(ubr);
			
			// print errors
	        if (results.Errors.HasErrors)
	        {
	        	foreach (CompilerError error in results.Errors)
	            {
					var errorData = new ErrorData(error.ToString());
					ubr.Positions.Add(errorData.Position);
					compilerPlugin.AddError(errorData);
					flag = true;
					mainWindow.SetStatus("Build failed. " +results.Errors.Count.ToString() +" error(s)");
				}
	        }
	        else
	        {
	        	// run proccess
	        	ThreadPool.QueueUserWorkItem(delegate{
	        	    try {
	        	                             		
		        	    var proc = new Process();
			        	var dispatcher = Application.Current.Dispatcher;
				        proc.StartInfo.UseShellExecute = false;
				        proc.StartInfo.CreateNoWindow = true;
				        proc.StartInfo.FileName = file +(includeDebugInformation ? ".debug.exe" : ".exe");
				        proc.StartInfo.RedirectStandardOutput = true;
				        proc.StartInfo.RedirectStandardError = true;
				        proc.EnableRaisingEvents = true;
				        proc.Exited += new EventHandler(proc_Exited);
				        proc.Start();
				        // print output
				        foreach (var line in proc.StandardOutput.ReadLines())
				        {
				        	dispatcher.Invoke((Action)(() => compilerPlugin.AddOutput(line)));
				        }
				        // print errors
				        foreach (var line in proc.StandardError.ReadLines())
				        {
				        	var errorData = new ErrorData(line);
							ubr.Positions.Add(errorData.Position);
							dispatcher.Invoke((Action)(() => compilerPlugin.AddError(errorData)), null);
				        }
				        dispatcher.Invoke((Action)(() => mainWindow.SetStatus("Build finished successfully.")));
				        
                 	} catch(Exception) {}
				});
	        }
		}
		
		public override bool CanExecute(object parameter)
		{
			if (editorPlugin == null)
				return false;
			var editor = editorPlugin.ActiveEditor;
			if (editor != null && editor.Document.TextLength > 0 && flag)
				return true;
			else 
				return false;
		}
		
		public void proc_Exited(object sender, EventArgs e)
		{
			flag = true;
		}
	}
}
