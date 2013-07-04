
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ICSharpCode.ILSpy;
using MainWindowPlugin;

namespace EditorPlugin
{
	/// <summary>
	/// Redo command
	/// </summary>
	[ExportMainMenuCommand(Menu = "_Edit", MenuIcon = "Images/Redo.png", Header = "_Redo", MenuCategory = "1", MenuOrder = 2.2, InputGestureText = "Ctrl+Y", CommandHotKey = System.Windows.Input.Key.Y, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Redo", ToolbarIcon = "Images/Redo.png", ToolbarCategory = "3", ToolbarOrder = 3.2, HotKey = System.Windows.Input.Key.Y, ModifierKey = ModifierKeys.Control)]
	public class RedoCommand : CommandWrapper
	{
		public RedoCommand() : base(ApplicationCommands.Redo)
		{
		}
	}
}
