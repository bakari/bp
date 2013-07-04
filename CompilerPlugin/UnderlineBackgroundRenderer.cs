using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace CompilerPlugin
{
	/// <summary>
	/// Renders squiggly lines
	/// </summary>
	public class UnderlineBackgroundRenderer : IBackgroundRenderer
	{
		TextEditor editor;
		List<string> positions;
		
		public UnderlineBackgroundRenderer(TextEditor e)
		{
			editor = e;
			positions = new List<string>();
		}
		
		public KnownLayer Layer {
			get { return KnownLayer.Caret; }
		}
		
		public List<string> Positions { get { return positions; } }
		
		public void Draw(TextView textView, System.Windows.Media.DrawingContext drawingContext)
		{	
			foreach (var s in positions)
			{
				try
				{
					textView.EnsureVisualLines();
					var sMatch = Regex.Match(s, @"([0-9]*),[0-9]*");
					var lineNumber = int.Parse(sMatch.Success ? sMatch.Groups[1].Value : "1");
					sMatch = Regex.Match(s, @"[0-9]*,([0-9]*)");
					var column = int.Parse(sMatch.Success ? sMatch.Groups[1].Value : "1");
					
					var currentLine = editor.Document.GetLineByNumber(lineNumber);
					
					var pen = new Pen(new SolidColorBrush(Colors.Red),2);
					var offset = editor.TextArea.TextView.GetVisualPosition(new TextViewPosition(lineNumber, column), VisualYPosition.LineBottom).X;
					foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
					{
						var steps = (rect.BottomRight.X-offset-rect.BottomLeft.X) / 4;
						var startPoint = rect.BottomLeft;
						startPoint.X += offset;
						// Squiggly Line
						for (int i = 0; i < steps; i++) 
						{
							drawingContext.DrawLine(pen, new Point(startPoint.X+(i * 4), startPoint.Y+0), new Point(startPoint.X+(i * 4 + 3), startPoint.Y+3));
						}
					}
				}
				catch(Exception) {}
			}
		}
	}
}