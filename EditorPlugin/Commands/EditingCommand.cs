
using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using EditorPlugin;
using MainWindowPlugin;
using ICSharpCode.ILSpy;

namespace EditorPlugin
{
	/// <summary>
	/// Description of EditingCommand.
	/// </summary>
	[ExportMainMenuCommand(Menu = "_Edit", MenuIcon = "Images/Copy.png", Header = "_Copy", MenuCategory = "2", MenuOrder = 2.3, InputGestureText = "Ctrl+C", CommandHotKey = System.Windows.Input.Key.C, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Copy", ToolbarIcon = "Images/Copy.png", ToolbarCategory = "2", ToolbarOrder = 1.4, HotKey = System.Windows.Input.Key.C, ModifierKey = ModifierKeys.Control)]
	public class CopyCommand : CommandWrapper
	{
		public CopyCommand() : base(ApplicationCommands.Copy)
		{
		}
	}
	}
	
	[ExportMainMenuCommand(Menu = "_Edit", MenuIcon = "Images/Cut.png", Header = "_Cut", MenuCategory = "2", MenuOrder = 2.5, InputGestureText = "Ctrl+X", CommandHotKey = System.Windows.Input.Key.X, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Cut", ToolbarIcon = "Images/Cut.png", ToolbarCategory = "2", ToolbarOrder = 1.3, HotKey = System.Windows.Input.Key.X, ModifierKey = ModifierKeys.Control)]
	public class CutCommand : CommandWrapper
	{
		public CutCommand() : base(ApplicationCommands.Cut)
		{
		}
	}
	
	[ExportMainMenuCommand(Menu = "_Edit", MenuIcon = "Images/Paste.png", Header = "_Paste", MenuCategory = "2", MenuOrder = 2.4, InputGestureText = "Ctrl+V", CommandHotKey = System.Windows.Input.Key.V, CommandModifierKey = ModifierKeys.Control)]
	[ExportToolbarCommand(ToolTip = "Paste", ToolbarIcon = "Images/Paste.png", ToolbarCategory = "2", ToolbarOrder = 1.5, HotKey = System.Windows.Input.Key.V, ModifierKey = ModifierKeys.Control)]
	public class PasteCommand : CommandWrapper
	{
		public PasteCommand() : base(ApplicationCommands.Paste)
		{
		}
			
	[ExportMainMenuCommand(Menu = "_Edit", MenuIcon = "Images/Delete.png", Header = "_Delete", MenuCategory = "2", MenuOrder = 2.6, CommandHotKey = System.Windows.Input.Key.Delete)]
	[ExportToolbarCommand(ToolTip = "Delete", ToolbarIcon = "Images/Delete.png", ToolbarCategory = "2", ToolbarOrder = 1.6, HotKey = System.Windows.Input.Key.Delete)]
	public class DeleteCommand : CommandWrapper
	{
		public DeleteCommand() : base(ApplicationCommands.Delete)
		{
		}
	}
}
