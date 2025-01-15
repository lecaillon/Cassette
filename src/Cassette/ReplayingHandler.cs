using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cassette;

/// <summary>
///     An HTTP Client handler that caches successful responses and reuse them 
///     for speeding up your test suite and improving connectivity resilience.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="ReplayingHandler"/>.
/// </remarks>
/// <param name="cache"> The distributed cache used by Cassette to store successful HTTP response messages that went through this handler. </param>
/// <param name="options"> The options class for configuring Cassette. </param>
/// <param name="logger"> A dedicated logger. </param>
public class ReplayingHandler(IDistributedCache cache, IOptions<CassetteOptions> options, ILogger<ReplayingHandler> logger) : DelegatingHandler
{
    private readonly IDistributedCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly CassetteOptions _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        string key = await request.GetCassetteKey(_options);
        if (key is null)
        {
            _logger.LogDebug("HTTP {Method} {Uri} skipped by Cassette.", request.Method, request.RequestUri);
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
                _logger.LogInformation("HTTP {Method} {Uri} recorded by Cassette at {Key}.", request.Method, request.RequestUri, key);
            }

            return response;
        }
        else
        {
            var response = cassetteEntry.Replay();
            _logger.LogDebug("HTTP {Method} {Uri} replayed by Cassette using {Key}.", request.Method, request.RequestUri, key);

            return response;
        }
    }
}
