﻿namespace IDisposableAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DisposeMethodAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(IDISP010CallBaseDispose.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleDisposeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void HandleDisposeMethod(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.ContainingSymbol is IMethodSymbol method &&
                method.IsOverride &&
                method.Name == "Dispose")
            {
                var overridden = method.OverriddenMethod;
                if (overridden == null)
                {
                    return;
                }

                using (var invocations = InvocationWalker.Borrow(context.Node))
                {
                    foreach (var invocation in invocations)
                    {
                        if (invocation.TryGetInvokedMethodName(out var name) &&
                            name != overridden.Name)
                        {
                            continue;
                        }

                        if (SymbolComparer.Equals(context.SemanticModel.GetSymbolSafe(invocation, context.CancellationToken), overridden))
                        {
                            return;
                        }
                    }
                }

                if (overridden.DeclaringSyntaxReferences.Length == 0)
                {
                    context.ReportDiagnostic(Diagnostic.Create(IDISP010CallBaseDispose.Descriptor, context.Node.GetLocation()));
                    return;
                }

                using (var disposeWalker = Disposable.DisposeWalker.Borrow(overridden, context.SemanticModel, context.CancellationToken))
                {
                    foreach (var disposeCall in disposeWalker)
                    {
                        if (Disposable.TryGetDisposedRootMember(disposeCall, context.SemanticModel, context.CancellationToken, out var disposed))
                        {
                            var member = context.SemanticModel.GetSymbolSafe(disposed, context.CancellationToken);
                            if (!Disposable.IsMemberDisposed(member, method, context.SemanticModel, context.CancellationToken))
                            {
                                context.ReportDiagnostic(Diagnostic.Create(IDISP010CallBaseDispose.Descriptor, context.Node.GetLocation()));
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}