namespace IDisposableAnalyzers.Test.IDISP002DisposeMemberTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NUnit.Framework;

    public static partial class CodeFix
    {
        public static class Rx
        {
            private static readonly DiagnosticAnalyzer Analyzer = new FieldAndPropertyDeclarationAnalyzer();
            private static readonly CodeFixProvider Fix = new DisposeMemberFix();
            private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("IDISP002");

            [Test]
            public static void SerialDisposable()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;
    using System.Reactive.Disposables;

    public sealed class C : IDisposable
    {
        ↓private readonly SerialDisposable disposable = new SerialDisposable();

        public void Update()
        {
            this.disposable.Disposable = File.OpenRead(string.Empty);
        }

        public void Dispose()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;
    using System.IO;
    using System.Reactive.Disposables;

    public sealed class C : IDisposable
    {
        private readonly SerialDisposable disposable = new SerialDisposable();

        public void Update()
        {
            this.disposable.Disposable = File.OpenRead(string.Empty);
        }

        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }

            [Test]
            public static void ObservableSubscribe()
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class C : IDisposable
    {
        ↓private readonly IDisposable disposable;

        public C(IObservable<object> observable)
        {
            this.disposable = observable.Subscribe(_ => { });
        }

        public void Dispose()
        {
        }
    }
}";

                var fixedCode = @"
namespace RoslynSandbox
{
    using System;

    public sealed class C : IDisposable
    {
        private readonly IDisposable disposable;

        public C(IObservable<object> observable)
        {
            this.disposable = observable.Subscribe(_ => { });
        }

        public void Dispose()
        {
            this.disposable?.Dispose();
        }
    }
}";
                RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
                RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            }
        }
    }
}
