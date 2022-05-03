using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace RickrollAnalyzer.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer {

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(Diagnostics.RICK0001);

	public override void Initialize(AnalysisContext context) {
#if DEBUG
		if (!Debugger.IsAttached) Debugger.Launch();
#else
		context.EnableConcurrentExecution();
#endif
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

		context.RegisterOperationAction(AnalyzeOperation, OperationKind.Literal);
	}

	private void AnalyzeOperation(OperationAnalysisContext context) {
		var literal = (ILiteralOperation)context.Operation;
		var node = literal.Syntax;
		var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);

		if (
			SymbolEqualityComparer.Default.Equals(literal.Type, stringType) &&
			literal.ConstantValue.HasValue &&
			literal.ConstantValue.Value is string str
		) {
			var reportDiagnostic = context.ReportDiagnostic;
			TryReportRickroll(str, node, "https://www.youtube.com/watch?v=dQw4w9WgXcQ", reportDiagnostic);
			TryReportRickroll(str, node, "https://youtu.be/dQw4w9WgXcQ", reportDiagnostic);
		}
	}

	private static void TryReportRickroll(string str, SyntaxNode node, string pattern, Action<Diagnostic> reportDiagnostic) {
		var nodeSpan = node.GetLocation().SourceSpan;
		foreach (var stringSpan in GetLocations(str, pattern)) {
			TextSpan textSpan = new(nodeSpan.Start + stringSpan.Start + 1, stringSpan.Length);
			var location = Location.Create(node.SyntaxTree, textSpan);

			var diagnostic = Diagnostic.Create(
				Diagnostics.RICK0001,
				location
			);
			reportDiagnostic(diagnostic);
		}
	}

	private static IEnumerable<TextSpan> GetLocations(string str, string pattern) {
		int index = str.IndexOf(pattern);
		if (index == -1) yield break;

		yield return new TextSpan(index, pattern.Length);
		
		string sub = str.Substring(index + pattern.Length);
		foreach (var s in GetLocations(sub, pattern)) yield return s;
	}

}
