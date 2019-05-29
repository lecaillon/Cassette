using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cassette
{
    public class ReplayingHandler : DelegatingHandler
    {
        private readonly IDistributedCache _cache;
        private readonly CassetteOptions _options;
        private readonly ILogger _logger;

        public ReplayingHandler(IDistributedCache cache, IOptions<CassetteOptions> options, ILogger<ReplayingHandler> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            string key = await request.GetCassetteKey(_options);
            if (key is null)
            {
                _logger.LogDebug($"{request.Method} {request.RequestUri} skipped by Cassette.");
                return await base.SendAsync(request, ct);
            }

            var cassetteEntry = await _cache.GetAsync(key, ct);
            if (cassetteEntry is null)
            {
                var response = await base.SendAsync(request, ct);
                if (response.IsSuccessStatusCode)
                {
                    var cassette = await Cassette.Record(request, response);
                    await _cache.SetAsync(key, cassette.ToByteArray(), _options.CacheEntryOption, ct);
                    _logger.LogInformation($"{request.Method} {request.RequestUri} recorded by Cassette at {key}.");
                }

                return response;
            }
            else
            {
                var response = cassetteEntry.Replay();
                _logger.LogInformation($"{request.Method} {request.RequestUri} replayed by Cassette using {key}.");

                return response;
            }
        }
    }
}