﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Eocron.Algorithms.Streams
{
    public static class BinaryStreamExtensions
    {
        private static MemoryPool<byte> DefaultMemoryPool => MemoryPool<byte>.Shared;
        private const int DefaultBufferSize = 8 * 1024;

        /// <summary>
        /// Produce single call enumerable
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pool"></param>
        /// <param name="leaveOpen"></param>
        /// <returns></returns>
        public static IEnumerable<Memory<byte>> AsEnumerable(this Stream stream, MemoryPool<byte> pool = null, bool leaveOpen = false)
        {
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));
            return new BinaryReadOnlyStreamWrapper(() => leaveOpen ? new NonDisposableStream(stream) : stream, pool ?? DefaultMemoryPool, DefaultBufferSize);
        }

        /// <summary>
        /// Produce single call enumerable
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pool"></param>
        /// <param name="leaveOpen"></param>
        /// <returns></returns>
        public static IAsyncEnumerable<Memory<byte>> AsAsyncEnumerable(this Stream stream, MemoryPool<byte> pool = null, bool leaveOpen = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return new BinaryReadOnlyStreamWrapper(() => leaveOpen ? new NonDisposableStream(stream) : stream, pool ?? DefaultMemoryPool, DefaultBufferSize);
        }

        public static byte[] ToByteArray(this IEnumerable<Memory<byte>> stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            using var ms = new MemoryStream();
            foreach (var memory in stream)
            {
                ms.Write(memory.Span);
            }
            return ms.ToArray();
        }

        public static async Task<byte[]> ToByteArrayAsync(this IAsyncEnumerable<Memory<byte>> stream,
            CancellationToken ct = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            using var ms = new MemoryStream();
            await foreach (var memory in stream.WithCancellation(ct).ConfigureAwait(false))
            {
                ms.Write(memory.Span);
            }
            return ms.ToArray();
        }

        public static IEnumerable<Memory<byte>> GZip(this IEnumerable<Memory<byte>> stream, CompressionMode mode)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return new BinaryReadOnlyStreamWrapper(
                () => mode == CompressionMode.Decompress
                    ? (Stream)new GZipStream(new EnumerableStream(stream), mode, false)
                    : (Stream)new WriteToReadStream<GZipStream>(
                        x => new GZipStream(x, mode, false),
                        () => new EnumerableStream(stream),
                        (x, ct) => x.FlushAsync(ct),
                        x => x.Flush()),
                DefaultMemoryPool, DefaultBufferSize);
        }

        public static IAsyncEnumerable<Memory<byte>> GZip(this IAsyncEnumerable<Memory<byte>> stream, CompressionMode mode)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return new BinaryReadOnlyStreamWrapper(
                () => mode == CompressionMode.Decompress
                    ? (Stream)new GZipStream(new EnumerableStream(stream), mode, false)
                    : (Stream)new WriteToReadStream<GZipStream>(
                        x => new GZipStream(x, mode, false),
                        () => new EnumerableStream(stream),
                        (x, ct) => x.FlushAsync(ct),
                        x => x.Flush()),
                DefaultMemoryPool, DefaultBufferSize);
        }

        public static IEnumerable<Memory<byte>> CryptoTransform(this IEnumerable<Memory<byte>> stream,
            ICryptoTransform transform, CryptoStreamMode mode)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            return new BinaryReadOnlyStreamWrapper(
                () => mode == CryptoStreamMode.Read
                    ? (Stream)new CryptoStream(new EnumerableStream(stream), transform, mode, false)
                    : (Stream)new WriteToReadStream<CryptoStream>(
                        x => new CryptoStream(x, transform, mode, false),
                        () => new EnumerableStream(stream),
                        async (x, ct) => x.FlushFinalBlock(),
                        x => x.FlushFinalBlock()),
                DefaultMemoryPool, DefaultBufferSize);
        }

        public static IAsyncEnumerable<Memory<byte>> CryptoTransform(this IAsyncEnumerable<Memory<byte>> stream,
            ICryptoTransform transform, CryptoStreamMode mode)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            return new BinaryReadOnlyStreamWrapper(
                () => mode == CryptoStreamMode.Read
                    ? (Stream)new CryptoStream(new EnumerableStream(stream), transform, mode, false)
                    : (Stream)new WriteToReadStream<CryptoStream>(
                        x => new CryptoStream(x, transform, mode, false),
                        () => new EnumerableStream(stream),
                        async (x, ct) => x.FlushFinalBlock(),
                        x => x.FlushFinalBlock()),
                DefaultMemoryPool, DefaultBufferSize);
        }
    }
}