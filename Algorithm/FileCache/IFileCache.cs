﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Algorithm.FileCache
{
    /// <summary>
    /// Expiration policy of file. Allows one to invalidate object by custom behavior and constant updates.
    /// </summary>
    public interface ICacheExpirationPolicy
    {
        bool IsExpired(DateTime now);

        void LogAccess(DateTime now);

        bool TryMerge(ICacheExpirationPolicy toMerge);
    }

    /// <summary>
    /// Represents file system cache of files.
    /// Cache is working over some directory, and should be cleaned manually.
    /// To do so: invoke GarbageCollect method to give cache chance to cleanup.
    /// If you don't do this - cache will just grow over time.
    /// It will add 22 characters to base path. Be aware of your file paths.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IFileCache<TKey>
    {
        /// <summary>
        /// Invalidates entire cache. It will created from scratch.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task InvalidateAsync(CancellationToken token);

        /// <summary>
        /// Invalidates single key in cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task InvalidateAsync(TKey key, CancellationToken token);

        /// <summary>
        /// Performs garbage collection. It can skip files which currently readed.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task GarbageCollect(CancellationToken token);

        Task<Stream> GetStreamOrAddStreamAsync(TKey key, Func<TKey, Task<Stream>> provider, CancellationToken token, ICacheExpirationPolicy policy);
        Task<Stream> GetStreamAddFileAsync(TKey key, Func<TKey, Task<string>> provider, CancellationToken token, ICacheExpirationPolicy policy);
        Task AddOrUpdateStreamAsync(TKey key, Stream stream, CancellationToken token, ICacheExpirationPolicy policy);

        Task AddOrUpdateFileAsync(TKey key, string sourceFilePath, CancellationToken token, ICacheExpirationPolicy policy);
        Task GetFileOrAddStreamAsync(TKey key, Func<TKey, Task<Stream>> provider, CancellationToken token, string targetFilePath, ICacheExpirationPolicy policy);
        Task GetFileOrAddFileAsync(TKey key, Func<TKey, Task<string>> provider, CancellationToken token, string targetFilePath, ICacheExpirationPolicy policy);

        Task<bool> TryGetFile(TKey key, CancellationToken token, string targetFilePath);

        Task<Stream> TryGetStream(TKey key, CancellationToken token);
    }
}