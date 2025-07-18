using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PaintJob.App.PaintAlgorithms.Common.Painters;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Common
{
    /// <summary>
    /// Provides cached analysis results to improve performance for repeated operations.
    /// </summary>
    public class CachedAnalysisContext
    {
        private readonly ConcurrentDictionary<int, PaintContext> _contextCache = new ConcurrentDictionary<int, PaintContext>();
        private readonly ConcurrentDictionary<string, object> _analysisCache = new ConcurrentDictionary<string, object>();
        private readonly TimeSpan _cacheExpiration;
        private DateTime _lastCleanup = DateTime.UtcNow;

        public CachedAnalysisContext(TimeSpan? cacheExpiration = null)
        {
            _cacheExpiration = cacheExpiration ?? TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Gets or creates a cached paint context for the given grid.
        /// </summary>
        public PaintContext GetOrCreateContext(MyCubeGrid grid, Func<MyCubeGrid, PaintContext> factory)
        {
            CleanupIfNeeded();
            
            var gridHash = GetGridHash(grid);
            return _contextCache.GetOrAdd(gridHash, _ => factory(grid));
        }

        /// <summary>
        /// Gets or creates a cached analysis result.
        /// </summary>
        public T GetOrCreateAnalysis<T>(string key, Func<T> factory) where T : class
        {
            CleanupIfNeeded();
            
            var result = _analysisCache.GetOrAdd(key, _ => factory());
            return result as T;
        }

        /// <summary>
        /// Invalidates all cached data.
        /// </summary>
        public void InvalidateCache()
        {
            _contextCache.Clear();
            _analysisCache.Clear();
        }

        /// <summary>
        /// Invalidates cached data for a specific grid.
        /// </summary>
        public void InvalidateGrid(MyCubeGrid grid)
        {
            var gridHash = GetGridHash(grid);
            _contextCache.TryRemove(gridHash, out _);
        }

        private int GetGridHash(MyCubeGrid grid)
        {
            // Create a hash based on grid properties that affect painting
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + grid.EntityId.GetHashCode();
                hash = hash * 31 + grid.BlocksCount;
                hash = hash * 31 + grid.GridSizeEnum.GetHashCode();
                
                // Include modification timestamp if available
                if (grid.Physics != null)
                {
                    hash = hash * 31 + grid.Physics.LinearVelocity.GetHashCode();
                }
                
                return hash;
            }
        }

        private void CleanupIfNeeded()
        {
            var now = DateTime.UtcNow;
            if (now - _lastCleanup > _cacheExpiration)
            {
                InvalidateCache();
                _lastCleanup = now;
            }
        }
    }

    /// <summary>
    /// Performance-optimized block position iterator.
    /// </summary>
    public static class BlockPositionOptimizer
    {
        private static readonly ConcurrentDictionary<MySlimBlock, List<Vector3I>> _positionCache = 
            new ConcurrentDictionary<MySlimBlock, List<Vector3I>>();

        /// <summary>
        /// Gets all positions for a block with caching.
        /// </summary>
        public static IEnumerable<Vector3I> GetBlockPositions(MySlimBlock block)
        {
            return _positionCache.GetOrAdd(block, b =>
            {
                var positions = new List<Vector3I>();
                var min = b.Min;
                var max = b.Max;
                
                // Pre-calculate capacity for better performance
                var capacity = (max.X - min.X + 1) * (max.Y - min.Y + 1) * (max.Z - min.Z + 1);
                positions.Capacity = capacity;
                
                for (var x = min.X; x <= max.X; x++)
                {
                    for (var y = min.Y; y <= max.Y; y++)
                    {
                        for (var z = min.Z; z <= max.Z; z++)
                        {
                            positions.Add(new Vector3I(x, y, z));
                        }
                    }
                }
                
                return positions;
            });
        }

        /// <summary>
        /// Clears the position cache.
        /// </summary>
        public static void ClearCache()
        {
            _positionCache.Clear();
        }
    }

    /// <summary>
    /// Optimized color result builder using batched operations.
    /// </summary>
    public class BatchedColorResultBuilder
    {
        private readonly Dictionary<Vector3I, int> _results;
        private readonly List<ColorOperation> _pendingOperations;

        private struct ColorOperation
        {
            public Vector3I Position;
            public int ColorIndex;
            public int Priority;
        }

        public BatchedColorResultBuilder(int estimatedCapacity = 1000)
        {
            _results = new Dictionary<Vector3I, int>(estimatedCapacity);
            _pendingOperations = new List<ColorOperation>(estimatedCapacity / 10);
        }

        /// <summary>
        /// Adds a color operation to the batch.
        /// </summary>
        public void AddOperation(Vector3I position, int colorIndex, int priority)
        {
            _pendingOperations.Add(new ColorOperation
            {
                Position = position,
                ColorIndex = colorIndex,
                Priority = priority
            });
        }

        /// <summary>
        /// Adds multiple operations for a block.
        /// </summary>
        public void AddBlockOperations(MySlimBlock block, int colorIndex, int priority)
        {
            var positions = BlockPositionOptimizer.GetBlockPositions(block);
            foreach (var pos in positions)
            {
                AddOperation(pos, colorIndex, priority);
            }
        }

        /// <summary>
        /// Processes all pending operations and returns the final results.
        /// </summary>
        public Dictionary<Vector3I, int> BuildResults()
        {
            // Sort by priority (higher priority overwrites lower)
            _pendingOperations.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            // Apply operations
            foreach (var op in _pendingOperations)
            {
                _results[op.Position] = op.ColorIndex;
            }
            
            _pendingOperations.Clear();
            return _results;
        }
    }
}