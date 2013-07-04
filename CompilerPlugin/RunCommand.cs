
using System;
using System.Windows.Input;
using System.ComponentModel.Composition;
using ICSharpCode.ILSpy;
using ApplicationCore;
using MainWindowPlugin;

namespace CompilerPlugin
{
	/// <summary>
	/// Description of RunCommand.
	/// </summary>
	[Export(typeof(IPluginBase))]
	[ExportMainMenuCommand(Menu = "_Build", MenuIcon = "Images/Run.png", Header = "_Run without debugger", MenuCategory = "1", MenuOrder = 4.2, InputGestureText = "Ctrl+F5", CommandHotKey = System.Windows.Input.Key.F5, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Run without debugger", ToolbarIcon = "Images/Run.png", ToolbarCategory = "4", ToolbarOrder = 4.2)]
	public class RunCommand : CompilerCommand
	{
		
		public RunCommand()
		{
			base.includeDebugInformation = false;
		}
	}
}
