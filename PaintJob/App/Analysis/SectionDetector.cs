using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class SectionDetector
    {
        public class ShipSection
        {
            public int Id { get; set; }
            public HashSet<MySlimBlock> Blocks { get; set; } = new HashSet<MySlimBlock>();
            public Vector3 Center { get; set; }
            public BoundingBox Bounds { get; set; }
            public SectionType Type { get; set; }
            public List<int> ConnectedSections { get; set; } = new List<int>();
            public float Volume { get; set; }
            public bool IsMainHull { get; set; }
        }

        public class NeckConnection
        {
            public int Section1Id { get; set; }
            public int Section2Id { get; set; }
            public Vector3 Position { get; set; }
            public float CrossSectionArea { get; set; }
            public HashSet<MySlimBlock> ConnectingBlocks { get; set; } = new HashSet<MySlimBlock>();
        }

        public enum SectionType
        {
            MainHull,
            Wing,
            Nacelle,
            Tower,
            Protrusion,
            ThinConnector
        }

        public class SectionAnalysis
        {
            public List<ShipSection> Sections { get; set; } = new List<ShipSection>();
            public List<NeckConnection> Necks { get; set; } = new List<NeckConnection>();
            public ShipSection MainSection { get; set; }
        }

        private const float NECK_THRESHOLD = 0.3f; // Connection is a neck if cross-section < 30% of smaller section

        public SectionAnalysis AnalyzeGrid(MyCubeGrid grid)
        {
            var analysis = new SectionAnalysis();
            var blocks = grid.GetBlocks();
            
            if (!blocks.Any())
                return analysis;

            // Find connected components (sections)
            var sections = FindConnectedSections(grid, blocks);
            
            // Analyze each section
            foreach (var section in sections)
            {
                AnalyzeSection(grid, section);
            }
            
            // Find the main hull (largest section)
            var mainSection = sections.OrderByDescending(s => s.Blocks.Count).FirstOrDefault();
            if (mainSection != null)
            {
                mainSection.IsMainHull = true;
                mainSection.Type = SectionType.MainHull;
                analysis.MainSection = mainSection;
            }
            
            // Detect connections and necks between sections
            FindSectionConnections(grid, sections, analysis);
            
            // Classify sections based on their properties and connections
            ClassifySections(sections, analysis);
            
            analysis.Sections = sections;
            return analysis;
        }

        private List<ShipSection> FindConnectedSections(MyCubeGrid grid, HashSet<MySlimBlock> blocks)
        {
            var sections = new List<ShipSection>();
            var visited = new HashSet<MySlimBlock>();
            var sectionId = 0;
            
            foreach (var block in blocks)
            {
                if (visited.Contains(block))
                    continue;
                
                // Start a new section
                var section = new ShipSection { Id = sectionId++ };
                var queue = new Queue<MySlimBlock>();
                queue.Enqueue(block);
                visited.Add(block);
                
                // Flood fill to find all connected blocks
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    section.Blocks.Add(current);
                    
                    // Check all 6 directions
                    var directions = new[]
                    {
                        Vector3I.Up, Vector3I.Down, Vector3I.Left,
                        Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                    };
                    
                    foreach (var dir in directions)
                    {
                        var neighborPos = current.Position + dir;
                        var neighbor = grid.GetCubeBlock(neighborPos);
                        
                        if (neighbor != null && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
                
                if (section.Blocks.Count > 0)
                {
                    sections.Add(section);
                }
            }
            
            return sections;
        }

        private void AnalyzeSection(MyCubeGrid grid, ShipSection section)
        {
            if (!section.Blocks.Any())
                return;
            
            // Calculate bounds
            var min = Vector3.MaxValue;
            var max = Vector3.MinValue;
            var centerSum = Vector3.Zero;
            
            foreach (var block in section.Blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                min = Vector3.Min(min, worldPos);
                max = Vector3.Max(max, worldPos);
                centerSum += worldPos;
            }
            
            section.Bounds = new BoundingBox(min, max);
            section.Center = centerSum / section.Blocks.Count;
            section.Volume = section.Blocks.Count * grid.GridSize * grid.GridSize * grid.GridSize;
        }

        private void FindSectionConnections(MyCubeGrid grid, List<ShipSection> sections, SectionAnalysis analysis)
        {
            // Build a map of block positions to sections
            var blockToSection = new Dictionary<Vector3I, int>();
            foreach (var section in sections)
            {
                foreach (var block in section.Blocks)
                {
                    blockToSection[block.Position] = section.Id;
                }
            }
            
            // Find connections between sections
            var connections = new Dictionary<(int, int), HashSet<MySlimBlock>>();
            
            foreach (var section in sections)
            {
                foreach (var block in section.Blocks)
                {
                    // Check all neighbors
                    var directions = new[]
                    {
                        Vector3I.Up, Vector3I.Down, Vector3I.Left,
                        Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                    };
                    
                    foreach (var dir in directions)
                    {
                        var neighborPos = block.Position + dir;
                        if (blockToSection.TryGetValue(neighborPos, out var neighborSectionId) &&
                            neighborSectionId != section.Id)
                        {
                            var key = section.Id < neighborSectionId 
                                ? (section.Id, neighborSectionId) 
                                : (neighborSectionId, section.Id);
                            
                            if (!connections.ContainsKey(key))
                                connections[key] = new HashSet<MySlimBlock>();
                            
                            connections[key].Add(block);
                            
                            // Update connected sections lists
                            if (!section.ConnectedSections.Contains(neighborSectionId))
                                section.ConnectedSections.Add(neighborSectionId);
                            
                            var neighborSection = sections.First(s => s.Id == neighborSectionId);
                            if (!neighborSection.ConnectedSections.Contains(section.Id))
                                neighborSection.ConnectedSections.Add(section.Id);
                        }
                    }
                }
            }
            
            // Analyze connections to find necks
            foreach (var conn in connections)
            {
                var section1 = sections.First(s => s.Id == conn.Key.Item1);
                var section2 = sections.First(s => s.Id == conn.Key.Item2);
                var connectingBlocks = conn.Value;
                
                var connectionArea = connectingBlocks.Count * grid.GridSize * grid.GridSize;
                var smallerSectionArea = Math.Min(
                    GetCrossSectionArea(section1),
                    GetCrossSectionArea(section2)
                );
                
                // Check if this is a neck (narrow connection)
                if (connectionArea < smallerSectionArea * NECK_THRESHOLD)
                {
                    var neck = new NeckConnection
                    {
                        Section1Id = conn.Key.Item1,
                        Section2Id = conn.Key.Item2,
                        ConnectingBlocks = connectingBlocks,
                        CrossSectionArea = connectionArea,
                        Position = CalculateAveragePosition(grid, connectingBlocks)
                    };
                    
                    analysis.Necks.Add(neck);
                }
            }
        }

        private float GetCrossSectionArea(ShipSection section)
        {
            var size = section.Bounds.Size;
            // Use the two smallest dimensions as approximate cross-section
            var dimensions = new[] { size.X, size.Y, size.Z }.OrderBy(d => d).ToArray();
            return dimensions[0] * dimensions[1];
        }

        private Vector3 CalculateAveragePosition(MyCubeGrid grid, HashSet<MySlimBlock> blocks)
        {
            var sum = Vector3.Zero;
            foreach (var block in blocks)
            {
                sum += grid.GridIntegerToWorld(block.Position);
            }
            return sum / blocks.Count;
        }

        private void ClassifySections(List<ShipSection> sections, SectionAnalysis analysis)
        {
            foreach (var section in sections)
            {
                if (section.IsMainHull)
                    continue;
                
                var bounds = section.Bounds;
                var size = bounds.Size;
                var aspectRatios = CalculateAspectRatios(size);
                
                // Wing detection: thin in one dimension, elongated in another
                if (aspectRatios.thinness > 3.0f && aspectRatios.elongation > 2.0f)
                {
                    section.Type = SectionType.Wing;
                }
                // Nacelle detection: elongated cylinder-like sections
                else if (aspectRatios.elongation > 2.5f && aspectRatios.roundness > 0.7f)
                {
                    section.Type = SectionType.Nacelle;
                }
                // Tower detection: tall and narrow
                else if (size.Y > size.X * 2 && size.Y > size.Z * 2)
                {
                    section.Type = SectionType.Tower;
                }
                // Thin connector: very small volume relative to main hull
                else if (section.Volume < analysis.MainSection?.Volume * 0.05f &&
                         section.ConnectedSections.Count >= 2)
                {
                    section.Type = SectionType.ThinConnector;
                }
                // Default: general protrusion
                else
                {
                    section.Type = SectionType.Protrusion;
                }
            }
        }

        private (float elongation, float thinness, float roundness) CalculateAspectRatios(Vector3 size)
        {
            var dimensions = new[] { size.X, size.Y, size.Z }.OrderByDescending(d => d).ToArray();
            var elongation = dimensions[0] / dimensions[1];
            var thinness = dimensions[1] / dimensions[2];
            var roundness = dimensions[2] / dimensions[1]; // How circular the cross-section is
            
            return (elongation, thinness, roundness);
        }
    }
}