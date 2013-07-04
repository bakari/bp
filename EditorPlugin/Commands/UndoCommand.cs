
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using MainWindowPlugin;
using ICSharpCode.ILSpy;

namespace EditorPlugin
{
	/// <summary>
	/// UndoCommand.
	/// </summary>
	[ExportMainMenuCommand(Menu = "_Edit", MenuIcon = "Images/Undo.png", Header = "_Undo", MenuCategory = "1", MenuOrder = 2.1, InputGestureText = "Ctrl+Z", CommandHotKey = System.Windows.Input.Key.Z, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Undo", ToolbarIcon = "Images/Undo.png", ToolbarCategory = "3", ToolbarOrder = 3.1, HotKey = System.Windows.Input.Key.Z, ModifierKey = ModifierKeys.Control)]
	public class UndoCommand : CommandWrapper
	{
		public UndoCommand() : base(ApplicationCommands.Undo)
		{
		}
	}
}