using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackerNews.UnitTests.Helpers
{
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handlerAsync;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handlerAsync = req => Task.FromResult(handler(req));
        }
        public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handlerAsync)
        {
            _handlerAsync = handlerAsync;
        }
        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler, CancellationToken cancellationToken)
        {
            _handlerAsync = req => Task.FromResult(handler(req));
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handlerAsync(request);
    }
}
