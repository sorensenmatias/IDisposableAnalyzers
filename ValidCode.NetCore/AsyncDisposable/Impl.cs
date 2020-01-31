﻿namespace ValidCode.NetCore.AsyncDisposable
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class Impl : IAsyncDisposable
    {
        private readonly IAsyncDisposable disposable = File.OpenRead(string.Empty);

        public async ValueTask DisposeAsync()
        {
            await this.disposable.DisposeAsync();
        }
    }
}
