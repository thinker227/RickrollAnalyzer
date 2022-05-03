using Microsoft.CodeAnalysis;

namespace RickrollAnalyzer.Analyzer;

public static class Diagnostics {

	public static DiagnosticDescriptor RICK0001 { get; } = new(
		"RICK0001",
		"Do not include links to Rickroll",
		"Possible rickroll",
		"Code Quality",
		DiagnosticSeverity.Warning,
		true,
		"Do not include direct links to Rickroll in order to not cause accidental rickrolls"
	);

}