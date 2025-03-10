﻿namespace IDisposableAnalyzers.Test.Helpers.AssignedValueWalkerTests
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;

    using NUnit.Framework;

    public static partial class AssignedValueWalkerTests
    {
        public static class SideEffect
        {
            [TestCase("var temp1 = this.value;", "")]
            [TestCase("var temp2 = this.value;", "1")]
            [TestCase("var temp3 = this.value;", "1, 2")]
            [TestCase("var temp4 = this.value;", "1, 2, arg")]
            public static void MethodInjected(string statement, string expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    internal class C
    {
        private int value;

        internal C()
        {
            var temp1 = this.value;
            this.Update(1);
            var temp2 = this.value;
            this.Update(2);
            var temp3 = this.value;
        }

        internal void M()
        {
            var temp4 = this.value;
        }

        internal void Update(int arg)
        {
            this.value = arg;
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindEqualsValueClause(statement).Value;
                using var walker = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None);
                var actual = string.Join(", ", walker.Values);
                Assert.AreEqual(expected, actual);
            }

            [TestCase("var temp1 = this.value;", "")]
            [TestCase("var temp2 = this.value;", "1")]
            [TestCase("var temp3 = this.value;", "1, 2")]
            [TestCase("var temp4 = this.value;", "1, 2, arg")]
            public static void MethodInjectedWithOptional(string statement, string expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    internal class C
    {
        private int value;

        internal C()
        {
            var temp1 = this.value;
            this.Update(1);
            var temp2 = this.value;
            this.Update(2, ""abc"");
            var temp3 = this.value;
        }

        internal void M()
        {
            var temp4 = this.value;
        }

        internal void Update(int arg, string text = null)
        {
            this.value = arg;
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindEqualsValueClause(statement).Value;
                using var walker = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None);
                var actual = string.Join(", ", walker.Values);
                Assert.AreEqual(expected, actual);
            }

            [TestCase("var temp1 = this.text;", "")]
            [TestCase("var temp2 = this.text;", "null")]
            [TestCase("var temp3 = this.text;", "null, \"abc\"")]
            [TestCase("var temp4 = this.text;", "null, \"abc\", textArg")]
            public static void MethodInjectedWithOptionalAssigningOptional(string statement, string expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace N
{
    internal class C
    {
        private string text;

        internal C()
        {
            var temp1 = this.text;
            this.Update(1);
            var temp2 = this.text;
            this.Update(2, ""abc"");
            var temp3 = this.text;
        }

        internal void M()
        {
            var temp4 = this.text;
        }

        internal void Update(int arg, string textArg = null)
        {
            this.text = textArg;
        }
    }
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, Settings.Default.MetadataReferences);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindEqualsValueClause(statement).Value;
                using var walker = AssignedValueWalker.Borrow(value, semanticModel, CancellationToken.None);
                var actual = string.Join(", ", walker.Values);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
