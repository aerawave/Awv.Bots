using Microsoft.Extensions.Logging;
using System;

namespace Awv.Bots.Logging
{
    public class AwvLogFactory : ILoggerFactory
    {
        private ILoggerFactory internalFactory;

        public AwvLogFactory()
            : this("logs/log-{Date}-{Time}.json")
        {
        }

        public AwvLogFactory(string pathFormat)
        {
            internalFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                ;
            }).AddFile(pathFormat.Replace("{Time}", DateTime.Now.ToString("HHmmss")), isJson: true);
        }

        public void AddProvider(ILoggerProvider provider) => internalFactory.AddProvider(provider);
        public ILogger CreateLogger(string categoryName) => internalFactory.CreateLogger(categoryName);
        public void Dispose() => internalFactory.Dispose();
    }
}
