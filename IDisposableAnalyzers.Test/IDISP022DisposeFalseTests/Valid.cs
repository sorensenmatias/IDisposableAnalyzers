﻿namespace IDisposableAnalyzers.Test.IDISP022DisposeFalseTests
{
    using Gu.Roslyn.Asserts;
    using NUnit.Framework;

    public static class Valid
    {
        private static readonly FinalizerAnalyzer Analyzer = new();

        [Test]
        public static void SealedWithFinalizer()
        {
            var code = @"
namespace N
{
    using System;

    public sealed class C : IDisposable
    {
        private bool isDisposed = false;

        ~C()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }
    }
}";
            RoslynAssert.Valid(Analyzer, code);
        }
    }
}
