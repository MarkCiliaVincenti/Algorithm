﻿using System;
using System.Linq;
using System.Threading;
using Eocron.Algorithms.Graphs;
using NTests.Core;
using NUnit.Framework;
using QuikGraph;

namespace NTests
{
    [TestFixture, Category("Performance"), Explicit]
    public sealed class DijkstraPerformanceTests
    {
        private AdjacencyGraph<int, Edge<int>> _graph;

        [SetUp]
        public void SetUp()
        {
            var rnd = new Random(42);
            _graph = DijkstraTests.ParsePathToRome(Enumerable.Range(0, 100).Select(x => rnd.Next(0, 10)).ToList());
        }

        [Test]
        public void Infinite()
        {
            var source = 0;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            Benchmark.InfiniteMeasure(ctx =>
            {
                var result = new InfiniteDijkstraAlgorithm<int, int>(
                    x => _graph.OutEdges(x).Select(y => y.Target),
                    x => 0,
                    (x, y) => x.Weight + 1,
                    count: _graph.VertexCount);
                result.Search(source);
                ctx.Increment();
            }, cts.Token);
        }
    }
}