using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using ApplicationCore;
using ICSharpCode.ILSpy;

namespace EditorPlugin
{
	/// <summary>
	/// Description of MenuPlugins.
	/// </summary>
	[ExportMainMenuCommand(Menu = "_File", Header = "_Recent files", ElementName = "menuItem_recentFiles",  MenuCategory = "3",  MenuOrder = 1.6)]
	public class RecentFiles : SimpleCommand
	{
	}
}
