namespace IDisposableAnalyzers.NetCoreTests.Helpers
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    internal static partial class DisposableTests
    {
        internal static class IsPotentiallyAssignableTo
        {
            [TestCase("new string(' ', 1)", false)]
            [TestCase("new System.Text.StringBuilder()", false)]
            [TestCase("new System.IO.MemoryStream()", true)]
            [TestCase("(Microsoft.Extensions.Logging.ILoggerFactory)o", true)]
            public static void Expression(string code, bool expected)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class Foo
    {
        internal Foo(object o)
        {
            var value = PLACEHOLDER;
        }
    }
}".AssertReplace("PLACEHOLDER", code);
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindEqualsValueClause(code).Value;
                Assert.AreEqual(expected, Disposable.IsPotentiallyAssignableFrom(value, semanticModel, CancellationToken.None));
            }
        }
    }
}
