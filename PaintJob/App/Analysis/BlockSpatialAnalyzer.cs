using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class BlockSpatialAnalyzer
    {
        public class BlockSpatialInfo
        {
            public MySlimBlock Block { get; set; }
            public Vector3I Min { get; set; }
            public Vector3I Max { get; set; }
            public HashSet<Vector3I> OccupiedPositions { get; set; }
            public LayerType PrimaryLayer { get; set; }
            public Dictionary<Vector3I, LayerType> PositionLayers { get; set; }
            public bool IsMultiBlock => OccupiedPositions.Count > 1;
            public bool SpansMultipleLayers { get; set; }
        }

        public enum LayerType
        {
            Exterior,      // Exposed to space
            Interior,      // Accessible internal space
            Guts,          // Hidden between walls
            Transitional   // Spans multiple layers
        }

        public Dictionary<MySlimBlock, BlockSpatialInfo> AnalyzeBlockSpatialData(MyCubeGrid grid)
        {
            var result = new Dictionary<MySlimBlock, BlockSpatialInfo>();
            var blocks = grid.GetBlocks();
            
            // First pass: gather all occupied positions
            var allOccupiedPositions = new HashSet<Vector3I>();
            var blockInfos = new Dictionary<MySlimBlock, BlockSpatialInfo>();
            
            foreach (var block in blocks)
            {
                var info = new BlockSpatialInfo
                {
                    Block = block,
                    Min = block.Min,
                    Max = block.Max,
                    OccupiedPositions = new HashSet<Vector3I>(),
                    PositionLayers = new Dictionary<Vector3I, LayerType>()
                };
                
                // Get all positions this block occupies
                for (int x = block.Min.X; x <= block.Max.X; x++)
                {
                    for (int y = block.Min.Y; y <= block.Max.Y; y++)
                    {
                        for (int z = block.Min.Z; z <= block.Max.Z; z++)
                        {
                            var pos = new Vector3I(x, y, z);
                            info.OccupiedPositions.Add(pos);
                            allOccupiedPositions.Add(pos);
                        }
                    }
                }
                
                blockInfos[block] = info;
            }
            
            // Second pass: determine layers for each position
            foreach (var kvp in blockInfos)
            {
                var info = kvp.Value;
                var layerTypes = new HashSet<LayerType>();
                
                foreach (var pos in info.OccupiedPositions)
                {
                    var layer = DeterminePositionLayer(grid, pos, allOccupiedPositions);
                    info.PositionLayers[pos] = layer;
                    layerTypes.Add(layer);
                }
                
                // Determine primary layer and if block spans multiple layers
                info.SpansMultipleLayers = layerTypes.Count > 1;
                
                if (info.SpansMultipleLayers)
                {
                    info.PrimaryLayer = LayerType.Transitional;
                }
                else
                {
                    info.PrimaryLayer = layerTypes.First();
                }
                
                result[kvp.Key] = info;
            }
            
            return result;
        }

        private LayerType DeterminePositionLayer(MyCubeGrid grid, Vector3I position, HashSet<Vector3I> allOccupiedPositions)
        {
            var directions = new[]
            {
                Vector3I.Up, Vector3I.Down, Vector3I.Left,
                Vector3I.Right, Vector3I.Forward, Vector3I.Backward
            };
            
            var exposedFaces = 0;
            var emptyNeighbors = 0;
            var hasInteriorAccess = false;
            
            foreach (var dir in directions)
            {
                var neighborPos = position + dir;
                
                if (!allOccupiedPositions.Contains(neighborPos))
                {
                    emptyNeighbors++;
                    
                    // Check if this empty space leads to exterior
                    if (IsPathToExterior(grid, neighborPos, allOccupiedPositions))
                    {
                        exposedFaces++;
                    }
                    else
                    {
                        // Empty space but enclosed - likely interior
                        hasInteriorAccess = true;
                    }
                }
            }
            
            // Determine layer based on exposure
            if (exposedFaces >= 1)
            {
                return LayerType.Exterior;
            }
            else if (hasInteriorAccess && emptyNeighbors >= 2)
            {
                return LayerType.Interior;
            }
            else
            {
                return LayerType.Guts;
            }
        }

        private bool IsPathToExterior(MyCubeGrid grid, Vector3I startPos, HashSet<Vector3I> occupiedPositions)
        {
            // Simple check: if position is outside grid bounds, it's exterior
            var gridMin = grid.Min;
            var gridMax = grid.Max;
            
            if (startPos.X < gridMin.X - 1 || startPos.X > gridMax.X + 1 ||
                startPos.Y < gridMin.Y - 1 || startPos.Y > gridMax.Y + 1 ||
                startPos.Z < gridMin.Z - 1 || startPos.Z > gridMax.Z + 1)
            {
                return true;
            }
            
            // For positions inside grid bounds, we'd need more complex pathfinding
            // For now, use a simplified heuristic
            var maxDistance = 10; // Check within 10 blocks
            var visited = new HashSet<Vector3I>();
            var queue = new Queue<(Vector3I pos, int distance)>();
            queue.Enqueue((startPos, 0));
            
            while (queue.Count > 0)
            {
                var (pos, distance) = queue.Dequeue();
                
                if (distance > maxDistance)
                    continue;
                    
                if (visited.Contains(pos))
                    continue;
                    
                visited.Add(pos);
                
                // Check if we've reached exterior
                if (pos.X <= gridMin.X - 2 || pos.X >= gridMax.X + 2 ||
                    pos.Y <= gridMin.Y - 2 || pos.Y >= gridMax.Y + 2 ||
                    pos.Z <= gridMin.Z - 2 || pos.Z >= gridMax.Z + 2)
                {
                    return true;
                }
                
                // Check neighbors
                var directions = new[]
                {
                    Vector3I.Up, Vector3I.Down, Vector3I.Left,
                    Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                };
                
                foreach (var dir in directions)
                {
                    var neighborPos = pos + dir;
                    if (!occupiedPositions.Contains(neighborPos) && !visited.Contains(neighborPos))
                    {
                        queue.Enqueue((neighborPos, distance + 1));
                    }
                }
            }
            
            return false;
        }

        public HashSet<Vector3I> GetBlocksByLayer(Dictionary<MySlimBlock, BlockSpatialInfo> spatialData, LayerType layer)
        {
            var positions = new HashSet<Vector3I>();
            
            foreach (var info in spatialData.Values)
            {
                foreach (var kvp in info.PositionLayers)
                {
                    if (kvp.Value == layer)
                    {
                        positions.Add(kvp.Key);
                    }
                }
            }
            
            return positions;
        }

        public Dictionary<MySlimBlock, float> CalculateLayerComposition(Dictionary<MySlimBlock, BlockSpatialInfo> spatialData)
        {
            var result = new Dictionary<MySlimBlock, float>();
            
            foreach (var kvp in spatialData)
            {
                var info = kvp.Value;
                if (info.SpansMultipleLayers)
                {
                    // Calculate what percentage is exterior vs interior vs guts
                    var exteriorCount = info.PositionLayers.Count(p => p.Value == LayerType.Exterior);
                    var ratio = (float)exteriorCount / info.OccupiedPositions.Count;
                    result[kvp.Key] = ratio;
                }
            }
            
            return result;
        }
    }
}