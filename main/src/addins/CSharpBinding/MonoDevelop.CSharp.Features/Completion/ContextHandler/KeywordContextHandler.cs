//
// KeywordContextHandler.cs
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory6.CSharp.Completion.KeywordRecommenders;
using Microsoft.CodeAnalysis.Text;
using MonoDevelop.Ide.CodeCompletion;

namespace ICSharpCode.NRefactory6.CSharp.Completion
{
	internal sealed class RecommendedKeyword
	{
		public string Keyword { get; private set; }
		public bool IsIntrinsic { get; private set; }
		public bool ShouldFormatOnCommit { get; private set; }

		public RecommendedKeyword (string keyword, bool isIntrinsic = false, bool shouldFormatOnCommit = false)
		{
			this.Keyword = keyword;
			this.IsIntrinsic = isIntrinsic;
			this.ShouldFormatOnCommit = shouldFormatOnCommit;
		}
		
	}

	internal interface IKeywordRecommender<TContext>
	{
		IEnumerable<RecommendedKeyword> RecommendKeywords(int position, TContext context, CancellationToken cancellationToken);
	}

	class KeywordContextHandler : CompletionContextHandler
	{
		static readonly IKeywordRecommender<CSharpSyntaxContext>[] recommender = {
			new AbstractKeywordRecommender(),
			new AddKeywordRecommender(),
			new AliasKeywordRecommender(),
			new AscendingKeywordRecommender(),
			new AsKeywordRecommender(),
			new AssemblyKeywordRecommender(),
			new AsyncKeywordRecommender(),
			new AwaitKeywordRecommender(),
			new BaseKeywordRecommender(),
			new BoolKeywordRecommender(),
			new BreakKeywordRecommender(),
			new ByKeywordRecommender(),
			new ByteKeywordRecommender(),
			new CaseKeywordRecommender(),
			new CatchKeywordRecommender(),
			new CharKeywordRecommender(),
			new CheckedKeywordRecommender(),
			new ChecksumKeywordRecommender(),
			new ClassKeywordRecommender(),
			new ConstKeywordRecommender(),
			new ContinueKeywordRecommender(),
			new DecimalKeywordRecommender(),
			new DefaultKeywordRecommender(),
			new DefineKeywordRecommender(),
			new DelegateKeywordRecommender(),
			new DescendingKeywordRecommender(),
			new DisableKeywordRecommender(),
			new DoKeywordRecommender(),
			new DoubleKeywordRecommender(),
			new DynamicKeywordRecommender(),
			new ElifKeywordRecommender(),
			new ElseKeywordRecommender(),
			new EndIfKeywordRecommender(),
			new EndRegionKeywordRecommender(),
			new EnumKeywordRecommender(),
			new EqualsKeywordRecommender(),
			new ErrorKeywordRecommender(),
			new EventKeywordRecommender(),
			new ExplicitKeywordRecommender(),
			new ExternKeywordRecommender(),
			new FalseKeywordRecommender(),
			new FieldKeywordRecommender(),
			new FinallyKeywordRecommender(),
			new FixedKeywordRecommender(),
			new FloatKeywordRecommender(),
			new ForEachKeywordRecommender(),
			new ForKeywordRecommender(),
			new FromKeywordRecommender(),
			new GetKeywordRecommender(),
			new GlobalKeywordRecommender(),
			new GotoKeywordRecommender(),
			new GroupKeywordRecommender(),
			new HiddenKeywordRecommender(),
			new IfKeywordRecommender(),
			new ImplicitKeywordRecommender(),
			new InKeywordRecommender(),
			new InterfaceKeywordRecommender(),
			new InternalKeywordRecommender(),
			new IntKeywordRecommender(),
			new IntoKeywordRecommender(),
			new IsKeywordRecommender(),
			new JoinKeywordRecommender(),
			new LetKeywordRecommender(),
			new LineKeywordRecommender(),
			new LockKeywordRecommender(),
			new LongKeywordRecommender(),
			new MethodKeywordRecommender(),
			new ModuleKeywordRecommender(),
			new NameOfKeywordRecommender(),
			new NamespaceKeywordRecommender(),
			new NewKeywordRecommender(),
			new NullKeywordRecommender(),
			new ObjectKeywordRecommender(),
			new OnKeywordRecommender(),
			new OperatorKeywordRecommender(),
			new OrderByKeywordRecommender(),
			new OutKeywordRecommender(),
			new OverrideKeywordRecommender(),
			new ParamKeywordRecommender(),
			new ParamsKeywordRecommender(),
			new PartialKeywordRecommender(),
			new PragmaKeywordRecommender(),
			new PrivateKeywordRecommender(),
			new PropertyKeywordRecommender(),
			new ProtectedKeywordRecommender(),
			new PublicKeywordRecommender(),
			new ReadOnlyKeywordRecommender(),
			new ReferenceKeywordRecommender(),
			new RefKeywordRecommender(),
			new RegionKeywordRecommender(),
			new RemoveKeywordRecommender(),
			new RestoreKeywordRecommender(),
			new ReturnKeywordRecommender(),
			new SByteKeywordRecommender(),
			new SealedKeywordRecommender(),
			new SelectKeywordRecommender(),
			new SetKeywordRecommender(),
			new ShortKeywordRecommender(),
			new SizeOfKeywordRecommender(),
			new StackAllocKeywordRecommender(),
			new StaticKeywordRecommender(),
			new StringKeywordRecommender(),
			new StructKeywordRecommender(),
			new SwitchKeywordRecommender(),
			new ThisKeywordRecommender(),
			new ThrowKeywordRecommender(),
			new TrueKeywordRecommender(),
			new TryKeywordRecommender(),
			new TypeKeywordRecommender(),
			new TypeOfKeywordRecommender(),
			new TypeVarKeywordRecommender(),
			new UIntKeywordRecommender(),
			new ULongKeywordRecommender(),
			new UncheckedKeywordRecommender(),
			new UndefKeywordRecommender(),
			new UnsafeKeywordRecommender(),
			new UShortKeywordRecommender(),
			new UsingKeywordRecommender(),
			new VarKeywordRecommender(),
			new VirtualKeywordRecommender(),
			new VoidKeywordRecommender(),
			new VolatileKeywordRecommender(),
			new WarningKeywordRecommender(),
//			new WhenKeywordRecommender(),
			new WhereKeywordRecommender(),
			new WhileKeywordRecommender(),
			new YieldKeywordRecommender()
		};

		public override bool IsTriggerCharacter (Microsoft.CodeAnalysis.Text.SourceText text, int position)
		{
			var ch = text [position];
			return ch == '#' || 
				ch == ' ' && position >= 1 && !char.IsWhiteSpace (text [position - 1]) ||
				IsStartingNewWord (text, position);
		}

		protected async override Task<IEnumerable<CompletionData>> GetItemsWorkerAsync (CompletionResult completionResult, CompletionEngine engine, CompletionContext completionContext, CompletionTriggerInfo info, SyntaxContext ctx, CancellationToken cancellationToken)
		{
			var model = ctx.SemanticModel;
			if (ctx.CSharpSyntaxContext.IsInNonUserCode) {
				return Enumerable.Empty<CompletionData> ();
			}

			if (ctx.TargetToken.IsKind (SyntaxKind.OverrideKeyword))
				return Enumerable.Empty<CompletionData> ();

			if (info.CompletionTriggerReason == CompletionTriggerReason.CharTyped && info.TriggerCharacter == ' ') {
				if (!ctx.CSharpSyntaxContext.IsEnumBaseListContext && !ctx.LeftToken.IsKind (SyntaxKind.EqualsToken) && !ctx.LeftToken.IsKind (SyntaxKind.EqualsEqualsToken))
					return Enumerable.Empty<CompletionData> ();
//				completionResult.AutoCompleteEmptyMatch = false;
			}

			var result = new List<CompletionData> ();

			foreach (var r in recommender) {
				var recommended = r.RecommendKeywords (completionContext.Position, ctx.CSharpSyntaxContext, cancellationToken);
				if (recommended == null)
					continue;
				foreach (var kw in recommended) {
					result.Add (engine.Factory.CreateGenericData (this, kw.Keyword, GenericDataType.Keyword));
				}
			}
		
//			if (ctx.IsPreProcessorKeywordContext) {
//				foreach (var kw in preprocessorKeywords)
//					result.Add(factory.CreateGenericData (this, kw, GenericDataType.PreprocessorKeyword));
//			}
//			

//			if (parent.IsKind(SyntaxKind.TypeParameterConstraintClause)) {
//				result.Add(factory.CreateGenericData (this, "new()", GenericDataType.PreprocessorKeyword));
//			}
			return result;
		} 

	}
}
