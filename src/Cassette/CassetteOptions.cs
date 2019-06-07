using Microsoft.Extensions.Caching.Distributed;

namespace Cassette
{
    /// <summary>
    ///     An options class for configuring Cassette.
    /// </summary>
    public class CassetteOptions
    {
        /// <summary>
        ///     Name of the HTTP request header used by Cassette to prevent caching of the HTTP response.
        /// </summary>
        public const string NoRecord = "Cassette-No-Record";
        /// <summary>
        ///     Name of the HTTP request header used by Cassette to remove the request body from the record.
        ///     Use this HTTP header when the content of the request body is always different between calls 
        ///     (when it contains an auto-generated identifier for example) and causes cache misses.
        /// </summary>
        public const string ExcludeRequestBody = "Cassette-Exclude-Request-Body";
        /// <summary>
        ///     Name of the HTTP request header used by Cassette to remove the last segment of the URI from the record.
        ///     Use this HTTP header when the value of last segment of the URI is always different between calls and causes cache misses.
        /// </summary>
        public const string ExcludeLastUriSegment = "Cassette-Exclude-Last-Uri-Segment";

        /// <summary>
        ///     Gets or sets the prefix used in the name of a key in the cache.
        ///     The default value of this property is: Cassette
        /// </summary>
        public string KeyPrefix { get; set; } = "Cassette";

        /// <summary>
        ///     Gets or sets the separator used in the name of a key in the cache.
        ///     The default value of this property is the colon, following the Redis key naming convention.
        /// </summary>
        public string KeySeparator { get; set; } = ":";

        /// <summary>
        ///     Provides the cache options for an entry in the <see cref="IDistributedCache"/>.
        /// </summary>
        public DistributedCacheEntryOptions CacheEntryOption { get; } = new DistributedCacheEntryOptions();

        /// <summary>
        ///     List all HTTP request headers used by Cassette to the REFIT format.
        /// </summary>
        public static class Refit
        {
            /// <summary>
            ///     Name of the HTTP request header used by Cassette to prevent caching of the HTTP response.
            /// </summary>
            public const string NoRecord = CassetteOptions.NoRecord + ":";
            /// <summary>
            ///     Name of the HTTP request header used by Cassette to remove the request body from the record.
            ///     Use this HTTP header when the content of the request body is always different between calls 
            ///     (when it contains a auto-generated identifier for example) and causes cache misses.
            /// </summary>
            public const string ExcludeRequestBody = CassetteOptions.ExcludeRequestBody + ":";
            /// <summary>
            ///     Name of the HTTP request header used by Cassette to remove the last segment of the URI from the record.
            ///     Use this HTTP header when the value of last segment of the URI is always different between calls and causes cache misses.
            /// </summary>
            public const string ExcludeLastUriSegment = CassetteOptions.ExcludeLastUriSegment + ":";
        }
    }
}
