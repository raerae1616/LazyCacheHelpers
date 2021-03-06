﻿using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LazyCacheHelpers
{
    /// <summary>
    /// BBernard
    /// Original Source (MIT License): https://github.com/cajuncoding/LazyCacheHelpers
    /// 
    /// This class provides simple way to get started with teh LazyCacheHandler by implementing an easy to use 
    /// Default instance of LazyCacheHandler that supports generic caching of any data with any CacheKey.
    /// 
    /// Static implementation follows similar patterns for applications to quickly consume the default cache implementation which
    /// uses the .Net Memory Cache (via LazyDotNetMemoryCacheRepository) for underlying cache storage with all of the benefits
    /// of self-populating cache and lazy initialization as implemented by the LazyCacheHandler class.
    /// 
    /// </summary>
    public static class DefaultLazyCache
    {
        //Added methods to CacheHelper to work with MemoryCache more easily.
        //NOTE: .Net MemoryCache supports this does NOT support Garbage Collection and Resource Reclaiming so it should
        //      be used whenever caching dynamic runtime data.
        private static readonly LazyCacheHandler<object> _lazyCache = new LazyCacheHandler<object>();

        /// <summary>
        /// BBernard
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnValueFactory"></param>
        /// <param name="cachePolicyFactory"></param>
        /// <returns></returns>
        public static TValue GetOrAddFromCache<TKey, TValue>(TKey key, Func<TValue> fnValueFactory, ILazyCachePolicy cachePolicyFactory) 
            where TValue: class
        {
            TValue result = GetOrAddFromCache(key, fnValueFactory, cachePolicyFactory.GeneratePolicy());
            return result;
        }

        /// <summary>
        /// BBernard
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnValueFactory"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        public static TValue GetOrAddFromCache<TKey, TValue>(TKey key, Func<TValue> fnValueFactory, CacheItemPolicy cacheItemPolicy)
            where TValue : class
        {
            TValue result = LazyCachePolicy.IsPolicyEnabled(cacheItemPolicy)
                                ? (TValue) _lazyCache.GetOrAddFromCache(key, fnValueFactory, cacheItemPolicy)
                                : fnValueFactory();
            return result;
        }

        /// <summary>
        /// BBernard
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized Asynchronously from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnAsyncValueFactory"></param>
        /// <param name="cachePolicyFactory"></param>
        /// <returns></returns>
        public static async Task<TValue> GetOrAddFromCacheAsync<TKey, TValue>(TKey key, Func<Task<TValue>> fnAsyncValueFactory, ILazyCachePolicy cachePolicyFactory)
            where TValue : class
        {
            TValue result = await GetOrAddFromCacheAsync<TKey, TValue>(key, fnAsyncValueFactory, cachePolicyFactory.GeneratePolicy());
            return result;
        }

        /// <summary>
        /// BBernard
        /// Add or update the cache with the specified cache key and item that will be Lazy Initialized Asynchronously from Lambda function/logic.
        /// This method ensures that the item is initialized with full thread safety and that only one thread ever executes the work
        /// to initialize the item to be cached -- significantly improving server utilization and performance.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="fnAsyncValueFactory"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <returns></returns>
        public static async Task<TValue> GetOrAddFromCacheAsync<TKey, TValue>(TKey key, Func<Task<TValue>> fnAsyncValueFactory, CacheItemPolicy cacheItemPolicy) 
            where TValue : class
        {
            //Because the underlying cache is set up to store any object and the async coercion isn't as easy as the synchronous,
            //  we must wrap the original generics typed async factory into a new Func<> that matches the required type.
            var wrappedFnValueFactory = new Func<Task<object>>(async () => await fnAsyncValueFactory());

            TValue result = LazyCachePolicy.IsPolicyEnabled(cacheItemPolicy)
                                ? (TValue) await _lazyCache.GetOrAddFromCacheAsync(key, wrappedFnValueFactory, cacheItemPolicy)
                                : await fnAsyncValueFactory();

            return result;
        }

        /// <summary>
        /// BBernard
        /// Remove the item/data corresponding to the specified cache key from the Cache.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        public static void RemoveFromCache<TKey>(TKey key)
        {
            _lazyCache.RemoveFromCache(key);
        }
    }
}
