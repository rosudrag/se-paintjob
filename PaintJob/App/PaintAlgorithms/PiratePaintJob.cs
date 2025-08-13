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
    public class PiratePaintJob : PaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "skull";
        private Random _random;

        public PiratePaintJob()
        {
            _colorResults = new Dictionary<Vector3I, int>();
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "skull";
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
                
                // Apply base weathered appearance
                ApplyWeatheredBase(blocks);
                
                // Apply battle damage simulation
                ApplyBattleDamage(blocks);
                
                // Apply pirate markings (skull patterns on larger surfaces)
                ApplyPirateMarkings(blocks);
                
                // Apply rust and wear to edges
                ApplyRustEffects(blocks);
                
                // Apply colors to weapons with special "menacing" colors
                ApplyWeaponColors(blocks);
                
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
                
                LogInfo($"Applied pirate paint job variant '{_variant}' to grid");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply pirate paint job: {ex.Message}");
                throw;
            }
        }

        private void ApplyWeatheredBase(HashSet<MySlimBlock> blocks)
        {
            // Use gradient for more realistic weathering
            var gradientPainter = new Common.Painters.GradientPainter(unchecked((int)_random.Next()));
            var patternPainter = new Common.Painters.PatternPainter(unchecked((int)_random.Next()));
            
            // Apply base weathered gradient
            gradientPainter.ApplyGradient(
                blocks,
                _colorResults,
                Common.Painters.GradientPainter.GradientType.Wave,
                0, // Dark base
                1, // Weathered metal
                direction: Vector3.Up // Weather runs down
            );
            
            // Add camo pattern for extra weathering texture
            patternPainter.ApplyPattern(
                blocks,
                _colorResults,
                Common.Painters.PatternPainter.PatternType.Camo,
                0, // Dark patches
                2, // Rust patches
                scale: 2f
            );
            
            // Original random weathering on top
            foreach (var block in blocks)
            {
                // Random weathering - mix between base colors
                var weatherLevel = _random.NextDouble();
                if (weatherLevel < 0.2 && !_colorResults.ContainsKey(block.Position))
                {
                    _colorResults[block.Position] = 0; // Dark base
                }
                else if (weatherLevel < 0.35 && _random.NextDouble() > 0.5)
                {
                    _colorResults[block.Position] = 1; // Weathered metal
                }
                else if (weatherLevel < 0.45 && _random.NextDouble() > 0.7)
                {
                    _colorResults[block.Position] = 2; // Rust/wear
                }
            }
        }

        private void ApplyBattleDamage(HashSet<MySlimBlock> blocks)
        {
            // Simulate battle damage with random scorch marks
            var damageSpots = _random.Next(3, 8);
            
            for (int i = 0; i < damageSpots; i++)
            {
                if (blocks.Count == 0) break;
                
                var centerBlock = blocks.ElementAt(_random.Next(blocks.Count));
                var damageRadius = _random.Next(2, 5);
                
                foreach (var block in blocks)
                {
                    var distance = Vector3I.DistanceManhattan(block.Position, centerBlock.Position);
                    if (distance <= damageRadius)
                    {
                        // Apply scorch/damage color
                        _colorResults[block.Position] = 3; // Scorch marks
                    }
                }
            }
        }

        private void ApplyPirateMarkings(HashSet<MySlimBlock> blocks)
        {
            // Find large flat surfaces for pirate emblems
            var surfaceGroups = GroupBlocksBySurface(blocks);
            
            foreach (var surface in surfaceGroups.Take(2)) // Apply to 2 largest surfaces
            {
                if (surface.Count > 20)
                {
                    // Create a skull pattern or crossbones effect
                    var center = CalculateCenter(surface);
                    
                    foreach (var block in surface)
                    {
                        var dist = Vector3I.DistanceManhattan(block.Position, center);
                        
                        // Create circular "skull" pattern
                        if (dist < 3)
                        {
                            _colorResults[block.Position] = 4; // Bone white
                        }
                        else if (dist < 5 && _random.NextDouble() > 0.5)
                        {
                            _colorResults[block.Position] = 5; // Dark accent
                        }
                    }
                }
            }
        }

        private void ApplyRustEffects(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                // Check if block is on an edge
                var neighborCount = CountNeighbors(block, blocks);
                
                if (neighborCount < 4) // Edge block
                {
                    if (_random.NextDouble() > 0.4)
                    {
                        _colorResults[block.Position] = 6; // Rust color
                    }
                }
            }
        }

        private void ApplyWeaponColors(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                if (blockDef.Contains("Gatling") || blockDef.Contains("Missile") || 
                    blockDef.Contains("Rocket") || blockDef.Contains("Cannon"))
                {
                    _colorResults[block.Position] = 7; // Menacing weapon color
                }
                else if (blockDef.Contains("Thrust"))
                {
                    _colorResults[block.Position] = 8; // Dark exhaust
                }
            }
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
                        
                        // Add connected blocks in same plane
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

        private int CountNeighbors(MySlimBlock block, HashSet<MySlimBlock> allBlocks)
        {
            var count = 0;
            var offsets = new[] {
                Vector3I.Forward, Vector3I.Backward,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Up, Vector3I.Down
            };
            
            foreach (var offset in offsets)
            {
                var neighborPos = block.Position + offset;
                if (allBlocks.Any(b => b.Position == neighborPos))
                {
                    count++;
                }
            }
            
            return count;
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            var colorGenerator = new Utils.ColorSchemeGenerator(seed);
            
            // Generate pirate-specific palette
            _colorPalette = colorGenerator.GeneratePiratePalette(_variant);
            
            if (_colorPalette == null || _colorPalette.Length == 0)
            {
                throw new InvalidOperationException("Failed to generate pirate color palette");
            }
            
            LogInfo($"Generated pirate palette with {_colorPalette.Length} colors for variant '{_variant}'");
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("PiratePaintJob", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("PiratePaintJob", $"ERROR: {message}");
        }
    }
}