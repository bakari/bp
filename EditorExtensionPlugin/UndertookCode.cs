#region Using Statements

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Xml;
using ApplicationCore;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;

#endregion

#region CodeCompletionData

/*
 * Erstellt mit SharpDevelop.
 * Benutzer: grunwald
 * Datum: 27.08.2007
 * Zeit: 14:25
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */

namespace CSharpEditor
{
	/// <summary>
	/// Represents an item in the code completion window.
	/// </summary>
	class CodeCompletionData : ICompletionData
	{
		// edited
		IEntity entity;
		private string text;
		
		// edited
		public CodeCompletionData(IEntity entity)
		{
			this.entity = entity;
			this.text = entity.Name;
		}
		
		// edited
		public CodeCompletionData(string name)
		{
			this.text = name;
		}
		
		// edited
		public CodeCompletionData(string name, string description)
		{
			this.text = name;
			this.description = description;
		}
		
		// edited
		public ImageSource Image 
		{
			get 
			{
				if (entity == null)
					if (description == null)
						return ImageProvider.Image_keyword;
					else
						return ImageProvider.Image_namespace;
				if (entity is IMethod)
					return ImageProvider.Image_method;
				if (entity is IProperty)
					return ImageProvider.Image_property;
				if (entity is IField)
					return ImageProvider.Image_field;
				if (entity is IClass)
					if ((entity as IClass).ClassType == ClassType.Enum)
						return ImageProvider.Image_enum;
					else
						return ImageProvider.Image_class;
				if (entity is IEvent)
					return ImageProvider.Image_event;
				return null;
			}
		}
		
		// edited
		public string Text 
		{ 
			get { return this.text; }
		}
		
		// edited
		public object Content {
			get
			{
				return Text;
			}
		}
		
		int overloads = 0;
		
		public void AddOverload()
		{
			overloads++;
		}
		
		string description;
		
		IAmbience ambience = new CSharpAmbience();
		
		public object Description {
			get 
			{
				if (description == null) 
				{
					if (entity is IMethod)
					{
						description = ambience.Convert(entity as IMethod);
					}
					else if (entity is IProperty)
					{
						description = ambience.Convert(entity as IProperty);
					}
					else if (entity is IEvent)
					{
						description = ambience.Convert(entity as IEvent);
					}
					else if (entity is IField)
					{
						description = ambience.Convert(entity as IField);
					}
					else if (entity is IClass)
					{
						description = ambience.Convert(entity as IClass);
					}
					else if (entity != null)
					{
						description = entity.ToString();
					}
					if (overloads > 1) {
						description += " (+" + overloads + " overloads)";
					}
					if (entity != null && !string.IsNullOrEmpty(XmlDocumentationToText(entity.Documentation)))
					{
						description += Environment.NewLine + XmlDocumentationToText(entity.Documentation);
					}
				}
				return description;
			}
		}
	
		// edited
		public string XmlDocumentationToText(string xmlDoc)
		{
			if (xmlDoc != null)
			{
				var match = Regex.Match(xmlDoc, @"<summary>(.*)</summary>");
				var s = match.Success ? match.Groups[1].Value.Replace("<see cref=\"","").Replace("\" />","").Replace("T:","").Replace("P:","").Replace("F:","") : "";
				match = Regex.Match(xmlDoc, @"<returns>(.*)</returns>");
				s = s +(match.Success ?  (Environment.NewLine +"Returns: " +match.Groups[1].Value.Replace("<see cref=\"","").Replace("\" />","").Replace("T:","").Replace("P:","").Replace("F:","")) : "");
				return s;
			}
			return xmlDoc;
		}
	
		public double Priority { get { return 0; } }
		
		public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
		{
			textArea.Document.Replace(completionSegment, Text);
		}
	}
}

#endregion

#region CodeCompletion

// CSharp Editor Example with Code Completion
// Copyright (c) 2006, Daniel Grunwald
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// - Redistributions of source code must retain the above copyright notice, this list
//   of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright notice, this list
//   of conditions and the following disclaimer in the documentation and/or other materials
//   provided with the distribution.
// 
// - Neither the name of the ICSharpCode team nor the names of its contributors may be used to
//   endorse or promote products derived from this software without specific prior written
//   permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace CSharpEditor
{
/*
 * following methods in CodeCompletion.cs
 * 
 * void ParserThread()
 * 
 * void ParseStep()
 * 
 * ICompilationUnit ConvertCompilationUnit(CompilationUnit cu)
 * 
 * ExpressionResult FindExpression(ICSharpCode.AvalonEdit.Editing.TextArea textArea)
 * 
 * void AddCompletionData(IList<ICompletionData> resultList, IEnumerable<ICompletionEntry> completionData)
 * 
 */
}

#endregion

#region BraceFoldingStrategy

// Copyright (c) 2009 Daniel Grunwald
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace AvalonEdit.Sample
{
	/// <summary>
	/// Allows producing foldings from a document based on braces.
	/// </summary>
	public class BraceFoldingStrategy : AbstractFoldingStrategy
	{
		/// <summary>
		/// Gets/Sets the opening brace. The default value is '{'.
		/// </summary>
		public char OpeningBrace { get; set; }
		
		/// <summary>
		/// Gets/Sets the closing brace. The default value is '}'.
		/// </summary>
		public char ClosingBrace { get; set; }
		
		/// <summary>
		/// Creates a new BraceFoldingStrategy.
		/// </summary>
		public BraceFoldingStrategy() 
		{
			this.OpeningBrace = '{';
			this.ClosingBrace = '}';
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
		{
			firstErrorOffset = -1;
			return CreateNewFoldings(document);
		}
		
		/// <summary>
		/// Create <see cref="NewFolding"/>s for the specified document.
		/// </summary>
		public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document)
		{
			var newFoldings = new List<NewFolding>();
			var startOffsets = new Stack<int>();
			var lastNewLineOffset = 0;
			var openingBrace = this.OpeningBrace;
			var closingBrace = this.ClosingBrace;
			for (int i = 0; i < document.TextLength; i++) {
				var c = document.GetCharAt(i);
				if (c == openingBrace) {
					// edited
					var n = 0;
					for ( ; n < i; n++)
					{
						if (char.IsLetterOrDigit(document.GetCharAt(i-n)))
						{
							n -= 1;
							break;
						}
					}
					startOffsets.Push(i-n);
				} else if (c == closingBrace && startOffsets.Count > 0) {
					int startOffset = startOffsets.Pop();
					// don't fold if opening and closing brace are on the same line
					if (startOffset < lastNewLineOffset) {
						newFoldings.Add(new NewFolding(startOffset, i + 1));
					}
				} else if (c == '\n' || c == '\r') {
					lastNewLineOffset = i + 1;
				}
			}
			newFoldings.Sort((a,b) => a.StartOffset.CompareTo(b.StartOffset));
			return newFoldings;
		}
	}
}

#endregion
