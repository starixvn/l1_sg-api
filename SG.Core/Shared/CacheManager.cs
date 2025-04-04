using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace SG.Core.Shared
{
    public interface ICacheManager
    {
        void SetCache<T>(string key, T value, byte? expirationMinute = null);

        T GetCache<T>(string key);
    }

    public class CacheManager : ICacheManager
    {
        private readonly IMemoryCache cache;
        private readonly byte expirationInMinute;
        const byte defaultExpirationMinute = 5;

        public CacheManager(IMemoryCache cache, IConfiguration configuration)
        {
            this.cache = cache;
            expirationInMinute = configuration.GetValue("CacheExpirationInMinute", defaultExpirationMinute);
        }

        public void SetCache<T>(string key, T value, byte? expirationMinute = null)
        {
            cache.Set(key, value, TimeSpan.FromMinutes(expirationMinute ?? expirationInMinute));
        }

        public T GetCache<T>(string key)
        {
            T value;

            if (cache.TryGetValue(key, out value))
            {
                return value;
            }

            return default;
        }
    }
}
