using System;
using System.Text.RegularExpressions;

namespace CompilerPlugin
{
	/// <summary>
	/// Description of ErrorData.
	/// </summary>
	public class ErrorData
	{
		public string Position { get; set; }
		public string Description { get; set; }
		public string File { get; set; }
		public string Line { get; set; }
		public string ToolTip { get; set; }
		
		/// <summary>
		/// CTOR for error in form: "c:\Users\Me\Desktop\File.csx(line,column) : error CS0116: Description..."
		/// </summary>
		public ErrorData(string s)
		{
			var sMatch = Regex.Match(s, @"\\([^\\\(]*)?\(");
			File = sMatch.Success ? sMatch.Groups[1].Value : "?";
			
			sMatch = Regex.Match(s, @"\(([^\:]*)\)");
			Position = sMatch.Success ? sMatch.Groups[1].Value : "?";
			
			sMatch = Regex.Match(s, @"error\s(.*)?");
			Description = sMatch.Success ? sMatch.Groups[1].Value : "?";
			
			sMatch = Regex.Match(s, @"\((.*),.*\)");
			Line = sMatch.Success ? sMatch.Groups[1].Value : "?";
			
			ToolTip = s;
		}
		
		public ErrorData(string file, string description, string line, string column, string tooltip)
		{
			File = file;
			Position = string.Concat(line,",",column);
			Line = line;
			ToolTip = tooltip;
		}
	}
}
