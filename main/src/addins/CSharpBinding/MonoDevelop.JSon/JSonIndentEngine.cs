//
// JSonTextEditorExtension.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Text;
using ICSharpCode.NRefactory6.CSharp;
using ICSharpCode.NRefactory6;
using MonoDevelop.Ide.Editor;
using Microsoft.CodeAnalysis.Text;
using MonoDevelop.Ide.TypeSystem;

namespace MonoDevelop.JSon
{
	class JSonIndentEngine : IStateMachineIndentEngine
	{
		TextEditor editor;
		DocumentContext ctx;
		int offset, line, column;
		internal Indent thisLineIndent, nextLineIndent;
		StringBuilder currentIndent;
		// char previousNewline = '\0';
		char previousChar = '\0';
		bool isLineStart;
		bool isInString;

		public JSonIndentEngine (TextEditor editor, DocumentContext ctx)
		{
			if (editor == null)
				throw new ArgumentNullException ("editor");
			if (ctx == null)
				throw new ArgumentNullException ("ctx");
			this.editor = editor;
			this.ctx = ctx;
			Reset ();
		}

		#region IStateMachineIndentEngine implementation

		public IStateMachineIndentEngine Clone ()
		{
			return (IStateMachineIndentEngine)MemberwiseClone ();
		}

		public bool IsInsidePreprocessorDirective {
			get {
				return false;
			}
		}

		public bool IsInsidePreprocessorComment {
			get {
				return false;
			}
		}

		public bool IsInsideStringLiteral {
			get {
				return false;
			}
		}

		public bool IsInsideVerbatimString {
			get {
				return false;
			}
		}

		public bool IsInsideCharacter {
			get {
				return false;
			}
		}

		public bool IsInsideString {
			get {
				return isInString;
			}
		}

		public bool IsInsideLineComment {
			get {
				return false;
			}
		}

		public bool IsInsideMultiLineComment {
			get {
				return false;
			}
		}

		public bool IsInsideDocLineComment {
			get {
				return false;
			}
		}

		public bool IsInsideComment {
			get {
				return false;
			}
		}

		public bool IsInsideOrdinaryComment {
			get {
				return false;
			}
		}

		public bool IsInsideOrdinaryCommentOrString {
			get {
				return false;
			}
		}

		public bool LineBeganInsideVerbatimString {
			get {
				return false;
			}
		}

		public bool LineBeganInsideMultiLineComment {
			get {
				return false;
			}
		}

		#endregion

		#region IDocumentIndentEngine implementation

		public void Push (char ch)
		{
			var isNewLine = NewLine.IsNewLine (ch);
			if (!isNewLine) {
				if (ch == '"')
					isInString = !IsInsideString;
				if (ch == '{' || ch == '[') {
					nextLineIndent.Push (IndentType.Block);
				} else if (ch == '}' || ch == ']') {
					if (thisLineIndent.Count > 0)
						thisLineIndent.Pop ();
					if (nextLineIndent.Count > 0)
						nextLineIndent.Pop ();
				} 
			} else {
				if (ch == NewLine.LF && previousChar == NewLine.CR) {
					offset++;
					previousChar = ch;
					return;
				}
			}

			offset++;
			if (!isNewLine) {
				// previousNewline = '\0';

				isLineStart &= char.IsWhiteSpace (ch);

				if (isLineStart)
					currentIndent.Append (ch);

				if (ch == '\t') {
					var nextTabStop = (column - 1 + editor.Options.IndentationSize) / editor.Options.IndentationSize;
					column = 1 + nextTabStop * editor.Options.IndentationSize;
				} else {
					column++;
				}
			} else {
				// previousNewline = ch;
				currentIndent.Length = 0;
				isLineStart = true;
				column = 1;
				line++;
				thisLineIndent = nextLineIndent.Clone ();
			}
			previousChar = ch;
		}

		public void Reset ()
		{
			offset = 0;
			line = column = 1;
			thisLineIndent = new Indent (ctx.GetOptionSet ());
			nextLineIndent = new Indent (ctx.GetOptionSet ());
			currentIndent = new StringBuilder ();
			// previousNewline = '\0';
			previousChar = '\0';
			isLineStart = true;
			isInString = false;
		}

		public void Update (int offset)
		{
			if (Offset > offset)
				Reset ();

			while (Offset < offset) {
				Push (editor.GetCharAt (Offset));
			}
		}

		IDocumentIndentEngine IDocumentIndentEngine.Clone ()
		{
			return Clone ();
		}

		SourceText sourceText;
		public SourceText Document {
			get {
				return sourceText ?? (sourceText = new MonoDevelopSourceText (editor));
			}
		}

		public string ThisLineIndent {
			get {
				return thisLineIndent.IndentString;
			}
		}

		public string NextLineIndent {
			get {
				return nextLineIndent.IndentString;
			}
		}

		public string CurrentIndent {
			get {
				return currentIndent.ToString ();
			}
		}

		public bool NeedsReindent {
			get {
				return ThisLineIndent != CurrentIndent;
			}
		}

		public int Offset {
			get {
				return offset;
			}
		}
//
//		public TextLocation Location {
//			get {
//				return new TextLocation (line, column);
//			}
//		}

		public bool EnableCustomIndentLevels {
			get;
			set;
		}

		#endregion

		#region ICloneable implementation

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		#endregion
	}
}
