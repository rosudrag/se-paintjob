using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class PatternGenerator
    {
        public enum PatternType
        {
            Stripes,
            Gradient,
            Hexagonal,
            Circuit,
            Radial,
            Checkerboard,
            Wave,
            Noise
        }

        public class PatternParameters
        {
            public PatternType Type { get; set; }
            public Vector3 Direction { get; set; } = Vector3.Forward;
            public Vector3 Origin { get; set; } = Vector3.Zero;
            public float Scale { get; set; } = 1.0f;
            public float Offset { get; set; } = 0.0f;
            public int[] ColorIndices { get; set; } = new[] { 0, 1 };
            public float BlendFactor { get; set; } = 0.0f; // For gradients
            public float Frequency { get; set; } = 1.0f; // For waves and noise
        }

        private readonly Random _random = new Random();

        public Dictionary<Vector3I, int> GeneratePattern(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();

            switch (parameters.Type)
            {
                case PatternType.Stripes:
                    return GenerateStripes(grid, blocks, parameters);
                case PatternType.Gradient:
                    return GenerateGradient(grid, blocks, parameters);
                case PatternType.Hexagonal:
                    return GenerateHexagonal(grid, blocks, parameters);
                case PatternType.Circuit:
                    return GenerateCircuit(grid, blocks, parameters);
                case PatternType.Radial:
                    return GenerateRadial(grid, blocks, parameters);
                case PatternType.Checkerboard:
                    return GenerateCheckerboard(grid, blocks, parameters);
                case PatternType.Wave:
                    return GenerateWave(grid, blocks, parameters);
                case PatternType.Noise:
                    return GenerateNoise(grid, blocks, parameters);
                default:
                    return result;
            }
        }

        private Dictionary<Vector3I, int> GenerateStripes(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var stripeWidth = parameters.Scale * grid.GridSize;
            var colorCount = parameters.ColorIndices.Length;

            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var projectedDistance = Vector3.Dot(worldPos - parameters.Origin, parameters.Direction);
                
                // Add offset
                projectedDistance += parameters.Offset;
                
                // Determine stripe index
                var stripeIndex = (int)Math.Floor(projectedDistance / stripeWidth);
                var colorIndex = Math.Abs(stripeIndex) % colorCount;
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }

            return result;
        }

        private Dictionary<Vector3I, int> GenerateGradient(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            
            // Find bounds along gradient direction
            var minProjection = float.MaxValue;
            var maxProjection = float.MinValue;
            
            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var projection = Vector3.Dot(worldPos - parameters.Origin, parameters.Direction);
                minProjection = Math.Min(minProjection, projection);
                maxProjection = Math.Max(maxProjection, projection);
            }
            
            var range = maxProjection - minProjection;
            if (range < 0.001f) range = 1.0f;
            
            var colorCount = parameters.ColorIndices.Length;
            
            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var projection = Vector3.Dot(worldPos - parameters.Origin, parameters.Direction);
                
                // Normalize to 0-1 range
                var t = (projection - minProjection) / range;
                t = Math.Max(0, Math.Min(1, t)); // Clamp
                
                // Apply offset and scale
                t = (t + parameters.Offset) * parameters.Scale;
                t = t % 1.0f; // Wrap
                
                // Map to color index
                var colorIndex = (int)(t * (colorCount - 1));
                colorIndex = Math.Max(0, Math.Min(colorCount - 1, colorIndex));
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }
            
            return result;
        }

        private Dictionary<Vector3I, int> GenerateHexagonal(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var hexSize = parameters.Scale * grid.GridSize * 2;
            
            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var relativePos = worldPos - parameters.Origin;
                
                // Project onto a plane perpendicular to direction
                var planeX = Vector3.Cross(parameters.Direction, Vector3.Up);
                if (planeX.LengthSquared() < 0.001f)
                    planeX = Vector3.Cross(parameters.Direction, Vector3.Right);
                planeX.Normalize();
                var planeY = Vector3.Cross(parameters.Direction, planeX);
                
                var x = Vector3.Dot(relativePos, planeX) / hexSize;
                var y = Vector3.Dot(relativePos, planeY) / hexSize;
                
                // Convert to hex coordinates
                var q = x;
                var r = y - x / 2;
                var s = -q - r;
                
                // Round to nearest hex
                var qRound = Math.Round(q);
                var rRound = Math.Round(r);
                var sRound = Math.Round(s);
                
                // Color based on hex coordinate sum
                var hexSum = Math.Abs(qRound) + Math.Abs(rRound) + Math.Abs(sRound);
                var colorIndex = (int)hexSum % parameters.ColorIndices.Length;
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }
            
            return result;
        }

        private Dictionary<Vector3I, int> GenerateCircuit(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var circuitScale = parameters.Scale * grid.GridSize;
            
            // First, apply a base color to all blocks
            var baseColor = parameters.ColorIndices[0];
            var circuitColor = parameters.ColorIndices.Length > 1 ? parameters.ColorIndices[1] : baseColor + 1;
            
            foreach (var block in blocks)
            {
                result[block.Position] = baseColor;
            }
            
            // Create circuit lines along edges
            var processedEdges = new HashSet<(Vector3I, Vector3I)>();
            
            foreach (var block in blocks)
            {
                // Check each direction for edges
                var directions = new[]
                {
                    Vector3I.Up, Vector3I.Down, Vector3I.Left,
                    Vector3I.Right, Vector3I.Forward, Vector3I.Backward
                };
                
                foreach (var dir in directions)
                {
                    var neighborPos = block.Position + dir;
                    var neighbor = grid.GetCubeBlock(neighborPos);
                    
                    // If no neighbor, this is an edge
                    if (neighbor == null || !blocks.Contains(neighbor))
                    {
                        // Random chance to start a circuit line
                        if (_random.NextDouble() < 0.3f / parameters.Scale)
                        {
                            // Trace a line perpendicular to the edge
                            var perpDirs = directions.Where(d => d != dir && d != -dir).ToArray();
                            var lineDir = perpDirs[_random.Next(perpDirs.Length)];
                            
                            var currentPos = block.Position;
                            var lineLength = _random.Next(3, 10);
                            
                            for (int i = 0; i < lineLength; i++)
                            {
                                var nextPos = currentPos + lineDir;
                                var nextBlock = grid.GetCubeBlock(nextPos);
                                
                                if (nextBlock != null && blocks.Contains(nextBlock))
                                {
                                    result[nextPos] = circuitColor;
                                    currentPos = nextPos;
                                    
                                    // Random chance to branch
                                    if (_random.NextDouble() < 0.2f)
                                    {
                                        lineDir = perpDirs[_random.Next(perpDirs.Length)];
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        private Dictionary<Vector3I, int> GenerateRadial(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var ringWidth = parameters.Scale * grid.GridSize;
            var colorCount = parameters.ColorIndices.Length;
            
            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var distance = Vector3.Distance(worldPos, parameters.Origin);
                
                // Add offset
                distance += parameters.Offset;
                
                // Determine ring index
                var ringIndex = (int)Math.Floor(distance / ringWidth);
                var colorIndex = ringIndex % colorCount;
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }
            
            return result;
        }

        private Dictionary<Vector3I, int> GenerateCheckerboard(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var checkSize = (int)Math.Max(1, parameters.Scale);
            
            foreach (var block in blocks)
            {
                var x = block.Position.X / checkSize;
                var y = block.Position.Y / checkSize;
                var z = block.Position.Z / checkSize;
                
                var sum = x + y + z;
                var colorIndex = Math.Abs(sum) % 2;
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }
            
            return result;
        }

        private Dictionary<Vector3I, int> GenerateWave(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var wavelength = parameters.Scale * grid.GridSize * 2 * (float)Math.PI;
            var colorCount = parameters.ColorIndices.Length;
            
            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var relativePos = worldPos - parameters.Origin;
                
                // Project position onto wave direction
                var distance = Vector3.Dot(relativePos, parameters.Direction);
                
                // Calculate wave value
                var waveValue = Math.Sin((distance / wavelength) * parameters.Frequency + parameters.Offset);
                
                // Map to color index
                var t = (waveValue + 1) / 2; // Normalize to 0-1
                var colorIndex = (int)(t * (colorCount - 1));
                colorIndex = Math.Max(0, Math.Min(colorCount - 1, colorIndex));
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }
            
            return result;
        }

        private Dictionary<Vector3I, int> GenerateNoise(
            MyCubeGrid grid,
            HashSet<MySlimBlock> blocks,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var noiseScale = parameters.Scale * 0.1f;
            var colorCount = parameters.ColorIndices.Length;
            
            foreach (var block in blocks)
            {
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var relativePos = worldPos - parameters.Origin;
                
                // Simple noise function (Perlin-like)
                var noiseValue = SimplexNoise(
                    (float)(relativePos.X * noiseScale * parameters.Frequency),
                    (float)(relativePos.Y * noiseScale * parameters.Frequency),
                    (float)(relativePos.Z * noiseScale * parameters.Frequency)
                );
                
                // Map noise to color index
                var t = (noiseValue + 1) / 2; // Normalize to 0-1
                var colorIndex = (int)(t * (colorCount - 1));
                colorIndex = Math.Max(0, Math.Min(colorCount - 1, colorIndex));
                
                result[block.Position] = parameters.ColorIndices[colorIndex];
            }
            
            return result;
        }

        // Simplified noise function
        private float SimplexNoise(float x, float y, float z)
        {
            // Very basic pseudo-noise
            var n = Math.Sin(x * 1.1f) * Math.Cos(y * 1.3f) * Math.Sin(z * 1.7f);
            n += Math.Cos(x * 2.3f) * Math.Sin(y * 2.9f) * Math.Cos(z * 3.1f) * 0.5f;
            n += Math.Sin(x * 4.1f) * Math.Sin(y * 4.7f) * Math.Sin(z * 5.3f) * 0.25f;
            return (float)n / 1.75f;
        }

        // Helper method to apply patterns with masking
        public Dictionary<Vector3I, int> ApplyPatternWithMask(
            Dictionary<Vector3I, int> baseColors,
            Dictionary<Vector3I, int> pattern,
            HashSet<Vector3I> mask)
        {
            var result = new Dictionary<Vector3I, int>(baseColors);
            
            foreach (var kvp in pattern)
            {
                if (mask == null || mask.Contains(kvp.Key))
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            
            return result;
        }
    }
}