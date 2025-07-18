using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class SurfaceAnalyzer
    {
        public class Surface
        {
            public int Id { get; set; }
            public HashSet<Vector3I> Positions { get; set; } = new HashSet<Vector3I>();
            public Vector3 Normal { get; set; }
            public float Area { get; set; }
            public SurfaceType Type { get; set; }
            public bool IsContinuous { get; set; }
            public List<EdgeInfo> Edges { get; set; } = new List<EdgeInfo>();
        }

        public class EdgeInfo
        {
            public HashSet<Vector3I> Positions { get; set; } = new HashSet<Vector3I>();
            public EdgeType Type { get; set; }
            public float Length { get; set; }
            public Vector3 Direction { get; set; }
        }

        public enum SurfaceType
        {
            Flat,           // Completely flat surface
            Curved,         // Smooth curves (using slopes)
            Irregular,      // Mixed or rough surface
            Angled          // Flat but at an angle
        }

        public enum EdgeType
        {
            Sharp,          // 90-degree edge
            Beveled,        // Angled edge (slope blocks)
            Rounded,        // Curved edge
            Jagged          // Irregular edge
        }

        public class SurfaceAnalysis
        {
            public List<Surface> Surfaces { get; set; } = new List<Surface>();
            public List<EdgeInfo> Edges { get; set; } = new List<EdgeInfo>();
            public Dictionary<Vector3I, int> PositionToSurface { get; set; } = new Dictionary<Vector3I, int>();
        }

        private readonly BlockSpatialAnalyzer _spatialAnalyzer = new BlockSpatialAnalyzer();

        public SurfaceAnalysis AnalyzeGrid(MyCubeGrid grid)
        {
            var analysis = new SurfaceAnalysis();
            var spatialData = _spatialAnalyzer.AnalyzeBlockSpatialData(grid);
            
            // Get all exterior positions
            var exteriorPositions = _spatialAnalyzer.GetBlocksByLayer(spatialData, BlockSpatialAnalyzer.LayerType.Exterior);
            
            // Group exterior positions into continuous surfaces
            var surfaces = FindContinuousSurfaces(grid, exteriorPositions, spatialData);
            
            // Analyze each surface
            foreach (var surface in surfaces)
            {
                AnalyzeSurface(grid, surface, spatialData);
                analysis.Surfaces.Add(surface);
                
                // Map positions to surfaces
                foreach (var pos in surface.Positions)
                {
                    analysis.PositionToSurface[pos] = surface.Id;
                }
            }
            
            // Find and analyze edges
            analysis.Edges = FindAndAnalyzeEdges(grid, surfaces, spatialData);
            
            return analysis;
        }

        private List<Surface> FindContinuousSurfaces(
            MyCubeGrid grid, 
            HashSet<Vector3I> exteriorPositions,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatialData)
        {
            var surfaces = new List<Surface>();
            var visited = new HashSet<Vector3I>();
            var surfaceId = 0;
            
            foreach (var startPos in exteriorPositions)
            {
                if (visited.Contains(startPos))
                    continue;
                
                var surface = new Surface { Id = surfaceId++ };
                var queue = new Queue<Vector3I>();
                queue.Enqueue(startPos);
                visited.Add(startPos);
                
                // Get the normal for this starting position
                var startNormal = EstimateNormal(grid, startPos, exteriorPositions);
                
                // Flood fill to find connected positions with similar normals
                while (queue.Count > 0)
                {
                    var currentPos = queue.Dequeue();
                    surface.Positions.Add(currentPos);
                    
                    // Check all 6 neighbors
                    var directions = new[]
                    {
                        Vector3I.Up, Vector3I.Down, Vector3I.Left,
                        Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                    };
                    
                    foreach (var dir in directions)
                    {
                        var neighborPos = currentPos + dir;
                        
                        if (!exteriorPositions.Contains(neighborPos) || visited.Contains(neighborPos))
                            continue;
                        
                        // Check if normal is similar (part of same surface)
                        var neighborNormal = EstimateNormal(grid, neighborPos, exteriorPositions);
                        var dotProduct = Vector3.Dot(startNormal, neighborNormal);
                        
                        // If normals are similar (within ~30 degrees), consider it part of same surface
                        if (dotProduct > 0.866f) // cos(30°)
                        {
                            visited.Add(neighborPos);
                            queue.Enqueue(neighborPos);
                        }
                    }
                }
                
                if (surface.Positions.Count > 0)
                {
                    surface.Normal = startNormal;
                    surfaces.Add(surface);
                }
            }
            
            return surfaces;
        }

        private Vector3 EstimateNormal(MyCubeGrid grid, Vector3I position, HashSet<Vector3I> exteriorPositions)
        {
            var directions = new[]
            {
                Vector3I.Up, Vector3I.Down, Vector3I.Left,
                Vector3I.Right, Vector3I.Forward, Vector3I.Backward
            };
            
            var normal = Vector3.Zero;
            
            // Simple normal estimation: average of directions to empty space
            foreach (var dir in directions)
            {
                var neighborPos = position + dir;
                if (!exteriorPositions.Contains(neighborPos))
                {
                    // This direction points to empty space
                    normal += new Vector3(dir.X, dir.Y, dir.Z);
                }
            }
            
            if (normal.LengthSquared() > 0)
            {
                normal.Normalize();
            }
            else
            {
                normal = Vector3.Up; // Default if no clear normal
            }
            
            return normal;
        }

        private void AnalyzeSurface(
            MyCubeGrid grid, 
            Surface surface,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatialData)
        {
            // Calculate area
            surface.Area = surface.Positions.Count * grid.GridSize * grid.GridSize;
            
            // Determine surface type
            surface.Type = DetermineSurfaceType(grid, surface, spatialData);
            
            // Check continuity
            surface.IsContinuous = CheckSurfaceContinuity(surface);
        }

        private SurfaceType DetermineSurfaceType(
            MyCubeGrid grid,
            Surface surface,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatialData)
        {
            // Check if all blocks in surface have same orientation and type
            var blocks = new HashSet<MySlimBlock>();
            
            foreach (var pos in surface.Positions)
            {
                var block = grid.GetCubeBlock(pos);
                if (block != null)
                {
                    blocks.Add(block);
                }
            }
            
            if (!blocks.Any())
                return SurfaceType.Irregular;
            
            // Check for slope blocks
            var hasSlopes = blocks.Any(b => b.BlockDefinition.Id.SubtypeName.Contains("Slope") || 
                                           b.BlockDefinition.Id.SubtypeName.Contains("Corner"));
            
            if (hasSlopes)
            {
                // Check if all slopes form a smooth curve
                var slopeOrientations = blocks.Where(b => b.BlockDefinition.Id.SubtypeName.Contains("Slope"))
                                             .Select(b => b.Orientation)
                                             .Distinct()
                                             .Count();
                
                return slopeOrientations <= 2 ? SurfaceType.Curved : SurfaceType.Irregular;
            }
            
            // Check if surface is angled
            var avgNormal = surface.Normal;
            var isAxisAligned = Math.Abs(avgNormal.X) > 0.9f || 
                               Math.Abs(avgNormal.Y) > 0.9f || 
                               Math.Abs(avgNormal.Z) > 0.9f;
            
            return isAxisAligned ? SurfaceType.Flat : SurfaceType.Angled;
        }

        private bool CheckSurfaceContinuity(Surface surface)
        {
            if (surface.Positions.Count <= 1)
                return true;
            
            // Use flood fill to check if all positions are connected
            var visited = new HashSet<Vector3I>();
            var queue = new Queue<Vector3I>();
            var firstPos = surface.Positions.First();
            
            queue.Enqueue(firstPos);
            visited.Add(firstPos);
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                var directions = new[]
                {
                    Vector3I.Up, Vector3I.Down, Vector3I.Left,
                    Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                };
                
                foreach (var dir in directions)
                {
                    var neighbor = current + dir;
                    if (surface.Positions.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            
            return visited.Count == surface.Positions.Count;
        }

        private List<EdgeInfo> FindAndAnalyzeEdges(
            MyCubeGrid grid,
            List<Surface> surfaces,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatialData)
        {
            var edges = new List<EdgeInfo>();
            
            foreach (var surface in surfaces)
            {
                var surfaceEdges = FindSurfaceEdges(grid, surface);
                
                foreach (var edge in surfaceEdges)
                {
                    AnalyzeEdge(grid, edge, spatialData);
                    edges.Add(edge);
                    surface.Edges.Add(edge);
                }
            }
            
            return edges;
        }

        private List<EdgeInfo> FindSurfaceEdges(MyCubeGrid grid, Surface surface)
        {
            var edges = new List<EdgeInfo>();
            var edgePositions = new HashSet<Vector3I>();
            
            // Find positions on the edge of the surface
            foreach (var pos in surface.Positions)
            {
                var directions = new[]
                {
                    Vector3I.Up, Vector3I.Down, Vector3I.Left,
                    Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                };
                
                var neighboringSurfaces = 0;
                foreach (var dir in directions)
                {
                    var neighbor = pos + dir;
                    if (surface.Positions.Contains(neighbor))
                    {
                        neighboringSurfaces++;
                    }
                }
                
                // If not all neighbors are in the surface, this is an edge
                if (neighboringSurfaces < 6)
                {
                    edgePositions.Add(pos);
                }
            }
            
            // Group edge positions into continuous edges
            var visitedEdges = new HashSet<Vector3I>();
            
            foreach (var startPos in edgePositions)
            {
                if (visitedEdges.Contains(startPos))
                    continue;
                
                var edge = new EdgeInfo { Positions = new HashSet<Vector3I>() };
                var queue = new Queue<Vector3I>();
                queue.Enqueue(startPos);
                visitedEdges.Add(startPos);
                
                // Trace the edge
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    edge.Positions.Add(current);
                    
                    // Look for connected edge positions
                    var directions = new[]
                    {
                        Vector3I.Up, Vector3I.Down, Vector3I.Left,
                        Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                    };
                    
                    foreach (var dir in directions)
                    {
                        var neighbor = current + dir;
                        if (edgePositions.Contains(neighbor) && !visitedEdges.Contains(neighbor))
                        {
                            visitedEdges.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                
                if (edge.Positions.Count > 0)
                {
                    edges.Add(edge);
                }
            }
            
            return edges;
        }

        private void AnalyzeEdge(
            MyCubeGrid grid,
            EdgeInfo edge,
            Dictionary<MySlimBlock, BlockSpatialAnalyzer.BlockSpatialInfo> spatialData)
        {
            // Calculate edge length
            edge.Length = edge.Positions.Count * grid.GridSize;
            
            // Determine edge type
            var blocks = new List<MySlimBlock>();
            foreach (var pos in edge.Positions)
            {
                var block = grid.GetCubeBlock(pos);
                if (block != null)
                {
                    blocks.Add(block);
                }
            }
            
            // Check for slope blocks
            var hasSlopes = blocks.Any(b => b.BlockDefinition.Id.SubtypeName.Contains("Slope"));
            var hasCorners = blocks.Any(b => b.BlockDefinition.Id.SubtypeName.Contains("Corner"));
            
            if (hasCorners)
            {
                edge.Type = EdgeType.Rounded;
            }
            else if (hasSlopes)
            {
                edge.Type = EdgeType.Beveled;
            }
            else
            {
                // Check if edge is straight
                var positions = edge.Positions.ToList();
                if (positions.Count >= 3)
                {
                    var direction = positions[1] - positions[0];
                    var isLinear = positions.Skip(2).All(p => 
                    {
                        var testDir = p - positions[0];
                        return Vector3.Cross(new Vector3(direction.X, direction.Y, direction.Z), 
                                           new Vector3(testDir.X, testDir.Y, testDir.Z)).LengthSquared() < 0.01f;
                    });
                    
                    edge.Type = isLinear ? EdgeType.Sharp : EdgeType.Jagged;
                }
                else
                {
                    edge.Type = EdgeType.Sharp;
                }
            }
            
            // Calculate average direction
            if (edge.Positions.Count >= 2)
            {
                var posList = edge.Positions.ToList();
                var first = posList[0];
                var last = posList[posList.Count - 1];
                edge.Direction = Vector3.Normalize(new Vector3(last.X - first.X, last.Y - first.Y, last.Z - first.Z));
            }
        }
    }
}