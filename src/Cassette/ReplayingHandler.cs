using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Cassette
{
    public class ReplayingHandler : DelegatingHandler
    {
        private readonly IDistributedCache _cache;

        public ReplayingHandler(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            string key = await request.GetCassetteKey();
            var cassetteEntry = await _cache.GetAsync(key, ct);
            if (cassetteEntry is null)
            {
                var response = await base.SendAsync(request, ct);
                if (response.IsSuccessStatusCode)
                {
                    var cassette = await Cassette.Record(request, response);
                    await _cache.SetAsync(key, cassette.ToByteArray(), ct);
                }

                return response;
            }
            else
            {
                return cassetteEntry.Replay();
            }
        }
    }
}