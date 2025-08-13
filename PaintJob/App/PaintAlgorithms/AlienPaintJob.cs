using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class AlienPaintJob : PaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "organic";
        private Random _random;

        public AlienPaintJob()
        {
            _colorResults = new Dictionary<Vector3I, int>();
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "organic";
        }

        public override void Clean()
        {
            _colorResults.Clear();
            _colorPalette = null;
        }

        protected override void Apply(MyCubeGrid grid)
        {
            try
            {
                var blocks = grid.GetBlocks();
                _random = new Random(unchecked((int)grid.EntityId));
                
                // First pass: Create dramatic base pattern
                ApplyOrganicBase(blocks);
                
                // Create large-scale bio structures
                ApplyBioStructures(blocks);
                
                // Apply bio-luminescent veins with better connectivity
                ApplyBioLuminescentVeins(blocks);
                
                // Apply pulsating energy nodes
                ApplyEnergyNodes(blocks);
                
                // Apply membrane-like patterns on surfaces
                ApplyMembranePatterns(blocks);
                
                // Apply exotic functional colors
                ApplyExoticFunctionalColors(blocks);
                
                // Create crystalline growth patterns
                if (_variant == "crystalline" || _variant == "ancient")
                {
                    ApplyCrystallineGrowth(blocks);
                }
                
                // Apply final colors to grid
                foreach (var block in blocks)
                {
                    if (_colorResults.TryGetValue(block.Position, out var colorIndex))
                    {
                        if (colorIndex >= 0 && colorIndex < _colorPalette.Length)
                        {
                            grid.ColorBlocks(block.Min, block.Max, _colorPalette[colorIndex], false);
                        }
                    }
                }
                
                LogInfo($"Applied alien paint job variant '{_variant}' to grid");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply alien paint job: {ex.Message}");
                throw;
            }
        }

        private void ApplyOrganicBase(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            var center = new Vector3(
                (bounds.Min.X + bounds.Max.X) / 2f,
                (bounds.Min.Y + bounds.Max.Y) / 2f,
                (bounds.Min.Z + bounds.Max.Z) / 2f
            );
            
            foreach (var block in blocks)
            {
                var pos = new Vector3(block.Position);
                var distFromCenter = Vector3.Distance(pos, center);
                var maxDist = Vector3.Distance(bounds.Min, bounds.Max);
                
                // Create organic gradient from center
                var normalizedDist = distFromCenter / (maxDist * 0.5f);
                
                // Add wave pattern for organic feel
                var wave = Math.Sin(normalizedDist * Math.PI * 3) * 0.5f + 0.5f;
                var spiral = Math.Sin(Math.Atan2(pos.Y - center.Y, pos.X - center.X) * 4 + distFromCenter * 0.2f);
                
                var combined = (wave + spiral * 0.3f) / 1.3f;
                
                if (combined < 0.3f)
                {
                    _colorResults[block.Position] = 0; // Deep organic base
                }
                else if (combined < 0.6f)
                {
                    _colorResults[block.Position] = 1; // Mid-tone organic
                }
                else
                {
                    _colorResults[block.Position] = 2; // Light organic tissue
                }
            }
        }

        private void ApplyBioStructures(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            
            // Create large organic "organs" or bio-chambers
            var organCount = _random.Next(2, 5);
            
            for (int i = 0; i < organCount; i++)
            {
                // Pick a random center point
                var centerX = _random.Next((int)bounds.Min.X, (int)bounds.Max.X + 1);
                var centerY = _random.Next((int)bounds.Min.Y, (int)bounds.Max.Y + 1);
                var centerZ = _random.Next((int)bounds.Min.Z, (int)bounds.Max.Z + 1);
                var organCenter = new Vector3I(centerX, centerY, centerZ);
                
                // Create organic shape
                var organRadius = _random.Next(3, 7);
                var organColor = _random.Next(3, 6); // Bio-luminescent colors
                
                foreach (var block in blocks)
                {
                    var dist = Vector3.Distance(new Vector3(block.Position), new Vector3(organCenter));
                    
                    // Create irregular organic shape using noise
                    var angle = Math.Atan2(block.Position.Y - organCenter.Y, block.Position.X - organCenter.X);
                    var radiusVariation = Math.Sin(angle * 5) * 0.3f + Math.Cos(angle * 3) * 0.2f + 1f;
                    var effectiveRadius = organRadius * radiusVariation;
                    
                    if (dist < effectiveRadius)
                    {
                        // Gradient from center to edge
                        var normalizedDist = dist / effectiveRadius;
                        
                        if (normalizedDist < 0.3f)
                        {
                            _colorResults[block.Position] = organColor; // Bright core
                        }
                        else if (normalizedDist < 0.7f && _random.NextDouble() > 0.3)
                        {
                            _colorResults[block.Position] = organColor + 1; // Mid-tone
                        }
                        else if (normalizedDist < 0.9f && _random.NextDouble() > 0.6)
                        {
                            _colorResults[block.Position] = organColor + 2; // Edge fade
                        }
                    }
                }
            }
        }

        private void ApplyBioLuminescentVeins(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            
            // Create major vein pathways along ship axes
            var veinPaths = new List<List<Vector3I>>();
            
            // Create main arteries along each axis
            // X-axis veins
            for (int y = (int)bounds.Min.Y; y <= bounds.Max.Y; y += _random.Next(3, 6))
            {
                for (int z = (int)bounds.Min.Z; z <= bounds.Max.Z; z += _random.Next(3, 6))
                {
                    var veinColor = _random.Next(3, 6);
                    foreach (var block in blocks)
                    {
                        if (Math.Abs(block.Position.Y - y) < 2 && Math.Abs(block.Position.Z - z) < 2)
                        {
                            // Main vein
                            if (Math.Abs(block.Position.Y - y) + Math.Abs(block.Position.Z - z) < 2)
                            {
                                _colorResults[block.Position] = veinColor;
                            }
                            // Vein glow
                            else if (_random.NextDouble() > 0.5)
                            {
                                _colorResults[block.Position] = veinColor + 1;
                            }
                        }
                    }
                }
            }
            
            // Y-axis veins (vertical)
            for (int x = (int)bounds.Min.X; x <= bounds.Max.X; x += _random.Next(4, 7))
            {
                for (int z = (int)bounds.Min.Z; z <= bounds.Max.Z; z += _random.Next(4, 7))
                {
                    var veinColor = _random.Next(4, 7);
                    foreach (var block in blocks)
                    {
                        if (Math.Abs(block.Position.X - x) < 2 && Math.Abs(block.Position.Z - z) < 2)
                        {
                            if (!_colorResults.ContainsKey(block.Position) || _random.NextDouble() > 0.3)
                            {
                                _colorResults[block.Position] = veinColor;
                            }
                        }
                    }
                }
            }
            
            // Create branching veins
            var branchCount = _random.Next(8, 15);
            for (int i = 0; i < branchCount; i++)
            {
                if (blocks.Count == 0) break;
                
                var startBlock = blocks.ElementAt(_random.Next(blocks.Count));
                var veinLength = _random.Next(5, 15);
                var veinColor = _random.Next(3, 8);
                var currentPos = startBlock.Position;
                
                for (int j = 0; j < veinLength; j++)
                {
                    // Apply vein color
                    _colorResults[currentPos] = veinColor;
                    
                    // Find next position with some randomness
                    var neighbors = GetNeighborPositions(currentPos, blocks);
                    if (neighbors.Count == 0) break;
                    
                    // Prefer to continue in similar direction with some variance
                    currentPos = neighbors[_random.Next(neighbors.Count)];
                    
                    // Add glow around vein
                    foreach (var block in blocks)
                    {
                        if (Vector3I.DistanceManhattan(block.Position, currentPos) == 1 && _random.NextDouble() > 0.5)
                        {
                            if (!_colorResults.ContainsKey(block.Position))
                            {
                                _colorResults[block.Position] = veinColor + 1;
                            }
                        }
                    }
                }
            }
        }

        private void ApplyEnergyNodes(HashSet<MySlimBlock> blocks)
        {
            // Create pulsating energy node patterns
            var nodeCount = _random.Next(3, 8);
            
            for (int i = 0; i < nodeCount; i++)
            {
                if (blocks.Count == 0) break;
                
                var nodeCenter = blocks.ElementAt(_random.Next(blocks.Count)).Position;
                var nodeRadius = _random.Next(2, 4);
                var nodeColor = _random.Next(6, 9); // Bright energy colors
                
                foreach (var block in blocks)
                {
                    var distance = Vector3I.DistanceManhattan(block.Position, nodeCenter);
                    
                    if (distance == 0)
                    {
                        _colorResults[block.Position] = nodeColor; // Bright center
                    }
                    else if (distance <= nodeRadius)
                    {
                        // Gradient effect
                        if (_random.NextDouble() > distance / (float)nodeRadius)
                        {
                            _colorResults[block.Position] = nodeColor + 1; // Slightly dimmer
                        }
                    }
                }
            }
        }

        private void ApplyMembranePatterns(HashSet<MySlimBlock> blocks)
        {
            // Initialize pattern painter for more complex patterns
            var patternPainter = new Common.Painters.PatternPainter(unchecked((int)_random.Next()));
            var gradientPainter = new Common.Painters.GradientPainter(unchecked((int)_random.Next()));
            
            // Apply translucent membrane-like patterns on larger surfaces
            var surfaceGroups = GroupBlocksBySurface(blocks);
            
            foreach (var surface in surfaceGroups.Where(s => s.Count > 15))
            {
                var surfaceSet = surface.ToHashSet();
                
                // Apply honeycomb pattern for organic feel
                if (_variant == "organic" || _variant == "hive")
                {
                    patternPainter.ApplyPattern(
                        surfaceSet,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Honeycomb,
                        9, // Membrane highlight
                        10, // Membrane mid-tone
                        scale: 1.2f
                    );
                }
                // Apply scale pattern for techno-organic
                else if (_variant == "techno_organic")
                {
                    patternPainter.ApplyPattern(
                        surfaceSet,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Scales,
                        9, // Tech scales primary
                        10, // Tech scales secondary
                        scale: 1.5f
                    );
                }
                // Apply circuit pattern for ancient tech
                else if (_variant == "ancient")
                {
                    patternPainter.ApplyPattern(
                        surfaceSet,
                        _colorResults,
                        Common.Painters.PatternPainter.PatternType.Circuit,
                        11, // Circuit traces
                        12, // Circuit nodes
                        scale: 1f
                    );
                }
                
                // Add gradient overlay for depth
                gradientPainter.ApplyGradient(
                    surfaceSet,
                    _colorResults,
                    Common.Painters.GradientPainter.GradientType.Spherical,
                    9, // Start color
                    10, // End color
                    customCenter: new Vector3(CalculateCenter(surface))
                );
            }
        }

        private void ApplyExoticFunctionalColors(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                // Thrusters get plasma-like colors
                if (blockDef.Contains("Thrust"))
                {
                    _colorResults[block.Position] = 11; // Plasma exhaust
                }
                // Weapons get energy discharge colors
                else if (blockDef.Contains("Gatling") || blockDef.Contains("Missile") || 
                         blockDef.Contains("Rocket"))
                {
                    _colorResults[block.Position] = 12; // Energy weapon
                }
                // Power systems get crystalline colors
                else if (blockDef.Contains("Reactor") || blockDef.Contains("Battery"))
                {
                    _colorResults[block.Position] = 13; // Crystal energy
                }
                // Sensors get psychic/telepathic colors
                else if (blockDef.Contains("Sensor") || blockDef.Contains("Antenna"))
                {
                    _colorResults[block.Position] = 14; // Psychic purple
                }
            }
        }

        private void ApplyCrystallineGrowth(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            
            // Create crystalline growth centers
            var crystalCount = _random.Next(3, 7);
            
            for (int i = 0; i < crystalCount; i++)
            {
                // Pick growth center
                var centerX = _random.Next((int)bounds.Min.X, (int)bounds.Max.X + 1);
                var centerY = _random.Next((int)bounds.Min.Y, (int)bounds.Max.Y + 1);
                var centerZ = _random.Next((int)bounds.Min.Z, (int)bounds.Max.Z + 1);
                var crystalCenter = new Vector3I(centerX, centerY, centerZ);
                
                // Crystal properties
                var maxRadius = _random.Next(4, 8);
                var crystalColor = _random.Next(11, 15); // Crystal colors
                var growthAngle = _random.NextDouble() * Math.PI * 2;
                
                foreach (var block in blocks)
                {
                    var offset = block.Position - crystalCenter;
                    var dist = offset.Length();
                    
                    if (dist <= maxRadius)
                    {
                        // Create angular crystalline structure
                        var blockAngle = Math.Atan2(offset.Y, offset.X);
                        var verticalAngle = Math.Atan2(offset.Z, Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y));
                        
                        // Create faceted crystal pattern
                        var facets = 6; // Hexagonal crystal structure
                        var facetAngle = Math.Round(blockAngle * facets / (2 * Math.PI)) * (2 * Math.PI) / facets;
                        var angleDiff = Math.Abs(blockAngle - facetAngle);
                        
                        // Layer-based growth
                        var layer = (int)(dist / 2);
                        var isEdge = angleDiff < 0.3f || dist % 2 < 0.5f;
                        
                        if (layer == 0)
                        {
                            _colorResults[block.Position] = crystalColor; // Crystal core
                        }
                        else if (isEdge && layer < 3)
                        {
                            _colorResults[block.Position] = crystalColor + 1; // Crystal edges
                        }
                        else if (layer < maxRadius - 1 && _random.NextDouble() > 0.3)
                        {
                            // Crystal body with transparency effect
                            _colorResults[block.Position] = crystalColor + ((layer % 2) == 0 ? 2 : 3);
                        }
                    }
                }
            }
            
            // Add crystal "shards" extending from main structures
            var shardCount = _random.Next(5, 10);
            for (int i = 0; i < shardCount; i++)
            {
                if (blocks.Count == 0) break;
                
                var startBlock = blocks.ElementAt(_random.Next(blocks.Count));
                var shardLength = _random.Next(3, 8);
                var shardDirection = new Vector3I(
                    _random.Next(-1, 2),
                    _random.Next(-1, 2),
                    _random.Next(-1, 2)
                );
                
                if (shardDirection == Vector3I.Zero) continue;
                
                var currentPos = startBlock.Position;
                var shardColor = _random.Next(12, 15);
                
                for (int j = 0; j < shardLength; j++)
                {
                    currentPos += shardDirection;
                    
                    // Check if position has a block
                    if (blocks.Any(b => b.Position == currentPos))
                    {
                        _colorResults[currentPos] = shardColor;
                        
                        // Add glow effect
                        foreach (var block in blocks)
                        {
                            if (Vector3I.DistanceManhattan(block.Position, currentPos) == 1)
                            {
                                if (!_colorResults.ContainsKey(block.Position) || _random.NextDouble() > 0.6)
                                {
                                    _colorResults[block.Position] = shardColor + 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        break; // Stop if we hit empty space
                    }
                }
            }
        }

        private float SimplexNoise(float x, float y, float z)
        {
            // Simplified pseudo-noise function for organic patterns
            var n = Math.Sin(x * 2.1f) * 1000 + Math.Sin(y * 3.3f) * 1000 + Math.Sin(z * 4.7f) * 1000;
            return (float)(n - Math.Floor(n)) * 2f - 1f;
        }

        private List<Vector3I> SelectVeinStartPoints(HashSet<MySlimBlock> blocks, int count)
        {
            var startPoints = new List<Vector3I>();
            var availableBlocks = blocks.ToList();
            
            for (int i = 0; i < count && availableBlocks.Count > 0; i++)
            {
                var index = _random.Next(availableBlocks.Count);
                startPoints.Add(availableBlocks[index].Position);
                availableBlocks.RemoveAt(index);
            }
            
            return startPoints;
        }

        private List<Vector3I> GetNeighborPositions(Vector3I position, HashSet<MySlimBlock> blocks)
        {
            var neighbors = new List<Vector3I>();
            var offsets = new[] {
                Vector3I.Forward, Vector3I.Backward,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Up, Vector3I.Down
            };
            
            foreach (var offset in offsets)
            {
                var neighborPos = position + offset;
                if (blocks.Any(b => b.Position == neighborPos))
                {
                    neighbors.Add(neighborPos);
                }
            }
            
            return neighbors;
        }

        private List<List<MySlimBlock>> GroupBlocksBySurface(HashSet<MySlimBlock> blocks)
        {
            var groups = new List<List<MySlimBlock>>();
            var processed = new HashSet<Vector3I>();
            
            foreach (var block in blocks)
            {
                if (!processed.Contains(block.Position))
                {
                    var group = new List<MySlimBlock>();
                    var queue = new Queue<MySlimBlock>();
                    queue.Enqueue(block);
                    
                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        if (processed.Contains(current.Position))
                            continue;
                        
                        processed.Add(current.Position);
                        group.Add(current);
                        
                        foreach (var other in blocks)
                        {
                            if (!processed.Contains(other.Position) &&
                                Vector3I.DistanceManhattan(current.Position, other.Position) == 1)
                            {
                                queue.Enqueue(other);
                            }
                        }
                    }
                    
                    groups.Add(group);
                }
            }
            
            return groups.OrderByDescending(g => g.Count).ToList();
        }

        private Vector3I CalculateCenter(List<MySlimBlock> blocks)
        {
            var sum = Vector3I.Zero;
            foreach (var block in blocks)
            {
                sum += block.Position;
            }
            return sum / blocks.Count;
        }

        private BoundingBox CalculateGridBounds(HashSet<MySlimBlock> blocks)
        {
            if (blocks.Count == 0)
                return new BoundingBox();
            
            var firstPos = blocks.First().Position;
            var min = new Vector3(firstPos);
            var max = new Vector3(firstPos);
            
            foreach (var block in blocks)
            {
                min = Vector3.Min(min, block.Position);
                max = Vector3.Max(max, block.Position);
            }
            
            return new BoundingBox(min, max);
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            var colorGenerator = new Utils.ColorSchemeGenerator(seed);
            
            // Generate alien-specific palette
            _colorPalette = colorGenerator.GenerateAlienPalette(_variant);
            
            if (_colorPalette == null || _colorPalette.Length == 0)
            {
                throw new InvalidOperationException("Failed to generate alien color palette");
            }
            
            LogInfo($"Generated alien palette with {_colorPalette.Length} colors for variant '{_variant}'");
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("AlienPaintJob", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("AlienPaintJob", $"ERROR: {message}");
        }
    }
}