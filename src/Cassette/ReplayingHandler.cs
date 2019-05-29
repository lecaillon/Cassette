using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Cassette
{
    public class ReplayingHandler : DelegatingHandler
    {
        private readonly IDistributedCache _cache;
        private readonly CassetteOptions _options;

        public ReplayingHandler(IDistributedCache cache, IOptions<CassetteOptions> options)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            string key = await request.GetCassetteKey(_options);
            var cassetteEntry = await _cache.GetAsync(key, ct);
            if (cassetteEntry is null)
            {
                var response = await base.SendAsync(request, ct);
                if (response.IsSuccessStatusCode)
                {
                    var cassette = await Cassette.Record(request, response);
                    await _cache.SetAsync(key, cassette.ToByteArray(), _options.CacheEntryOption, ct);
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