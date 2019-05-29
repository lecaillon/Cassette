using Microsoft.Extensions.Caching.Distributed;

namespace Cassette
{
    /// <summary>
    ///     An options class for configuring Cassette.
    /// </summary>
    public class CassetteOptions
    {
        /// <summary>
        ///     Gets or sets the prefix used in the name of a key.
        /// </summary>
        public string KeyPrefix { get; set; }

        /// <summary>
        ///     Gets or sets the separator used in the name of a key. 
        ///     The default value of this property is the colon, following the Redis key naming convention.
        /// </summary>
        public string KeySeparator { get; set; } = ":";

        /// <summary>
        ///     Provides the cache options for an entry in the <see cref="IDistributedCache"/>.
        /// </summary>
        public DistributedCacheEntryOptions CacheEntryOption { get; } = new DistributedCacheEntryOptions();
    }
}
