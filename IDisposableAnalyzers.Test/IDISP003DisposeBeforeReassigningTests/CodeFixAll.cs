namespace IDisposableAnalyzers.Test.IDISP003DisposeBeforeReassigningTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class CodeFixAll
    {
        private static readonly AssignmentAnalyzer Analyzer = new AssignmentAnalyzer();
        private static readonly ExpectedDiagnostic ExpectedDiagnostic = ExpectedDiagnostic.Create("IDISP003");
        private static readonly DisposeBeforeAssignFix Fix = new DisposeBeforeAssignFix();

        [Test]
        public static void NotDisposingVariable()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public class C
    {
        public void Meh()
        {
            var stream = File.OpenRead(string.Empty);
            ↓stream = File.OpenRead(string.Empty);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public class C
    {
        public void Meh()
        {
            var stream = File.OpenRead(string.Empty);
            stream?.Dispose();
            stream = File.OpenRead(string.Empty);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }

        [Test]
        public static void NotDisposingVariables()
        {
            var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public class C
    {
        public void Meh()
        {
            var stream1 = File.OpenRead(string.Empty);
            var stream2 = File.OpenRead(string.Empty);
            ↓stream1 = File.OpenRead(string.Empty);
            ↓stream2 = File.OpenRead(string.Empty);
        }
    }
}";

            var fixedCode = @"
namespace RoslynSandbox
{
    using System.IO;

    public class C
    {
        public void Meh()
        {
            var stream1 = File.OpenRead(string.Empty);
            var stream2 = File.OpenRead(string.Empty);
            stream1?.Dispose();
            stream1 = File.OpenRead(string.Empty);
            stream2?.Dispose();
            stream2 = File.OpenRead(string.Empty);
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, Fix, ExpectedDiagnostic, testCode, fixedCode);
        }
    }
}
