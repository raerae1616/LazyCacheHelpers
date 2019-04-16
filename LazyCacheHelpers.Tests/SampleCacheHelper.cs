﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LazyCacheHelpers;

namespace LazyCacheHelpersTests
{
    public class DemoCacheHelper
    {
        public static string GetCachedData(string cacheKeyVariable, Func<string> fnValueFactory, int secondsTTL = 60)
        {
            //Compute/Load the TTL from Configuration or from static class values, etc.
            //NOTE: During high load the cache timings could be Distributed to prevent multiple misses at one time.
            var timeSpanTTL = TimeSpan.FromSeconds(secondsTTL);
            //var timeSpanTTL = LazyCachePolicy.RandomizeCacheTTLDistribution(TimeSpan.FromSeconds(secondsTTL), 60);
            //var timeSpanTTL = LazyCacheConfig.GetCacheTTLFromConfig("Cache.SampleAppTTL");

            var result = DefaultLazyCache.GetOrAddFromCache(
                new DemoCacheKey(cacheKeyVariable),
                fnValueFactory,
                LazyCachePolicy.NewAbsoluteExpirationPolicy(timeSpanTTL)
            );

            return result;
        }

        public static async Task<string> GetCachedDataAsync(string cacheKeyVariable, Func<Task<string>> fnValueFactory, int secondsTTL = 60)
        {
            //Compute/Load the TTL from Configuration or from static class values, etc.
            //NOTE: During high load the cache timings could be Distributed to prevent multiple misses at one time.
            var timeSpanTTL = TimeSpan.FromSeconds(secondsTTL);
            //var timeSpanTTL = LazyCachePolicy.RandomizeCacheTTLDistribution(TimeSpan.FromSeconds(secondsTTL), 60);
            //var timeSpanTTL = LazyCacheConfig.GetCacheTTLFromConfig("Cache.SampleAppTTL");

            var result = await DefaultLazyCache.GetOrAddFromCacheAsync<ILazyCacheKey, string>(
                new DemoCacheKey(cacheKeyVariable),
                //NOTE: We wrap the value factory Func in a new Lambda to allow enable it to be 
                //      dynamically down cast to Func<Task<object>> of the underlying generic cache!
                async () => {
                    var factoryResult = await fnValueFactory();
                    return (object)factoryResult;
                },
                LazyCachePolicy.NewAbsoluteExpirationPolicy(timeSpanTTL)
            );

            return result;
        }

        public static void RemoveCachedData(string cacheKeyVariable)
        {
            var cacheKey = new DemoCacheKey(cacheKeyVariable);
            DefaultLazyCache.RemoveFromCache(cacheKey);
        }
    }

    public class DemoCacheKey : ILazyCacheKey
    {
        public DemoCacheKey(String keyNameVariable)
        {
            this.Variable = keyNameVariable;
        }

        public string Variable { get; private set; }

        public string GenerateKey()
        {
            return $"{nameof(DemoCacheKey)}::{this.Variable}";
        }
    }
}
