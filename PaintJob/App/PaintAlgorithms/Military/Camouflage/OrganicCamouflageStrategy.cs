using System;
using System.Collections.Generic;
using System.Linq;
using PaintJob.App.PaintAlgorithms.Common;
using PaintJob.App.PaintAlgorithms.Common.Patterns;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military.Camouflage
{
    /// <summary>
    /// Implements an organic noise-based camouflage pattern suitable for curved ships.
    /// </summary>
    public class OrganicCamouflageStrategy : IPatternStrategy
    {
        public string Name => "Organic";

        public Dictionary<Vector3I, int> GeneratePattern(
            MyCubeGrid grid,
            IEnumerable<Vector3I> positions,
            int[] colorIndices,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            
            if (colorIndices.Length == 0)
                return result;

            var positionsList = positions.ToList();
            var bounds = CalculateBounds(positionsList);
            var noiseScale = parameters.Scale * 0.1f; // Convert to noise-appropriate scale

            foreach (var pos in positionsList)
            {
                // Calculate 3D Perlin-like noise
                var noiseValue = Calculate3DNoise(pos, bounds, noiseScale, parameters);
                
                // Map noise to color index
                var colorIndex = MapNoiseToColorIndex(noiseValue, colorIndices.Length);
                result[pos] = colorIndices[colorIndex];
            }

            // Apply smoothing for more organic appearance
            ApplySmoothing(result, positionsList, colorIndices, parameters);

            return result;
        }

        private (Vector3I min, Vector3I max) CalculateBounds(List<Vector3I> positions)
        {
            if (!positions.Any())
                return (Vector3I.Zero, Vector3I.Zero);

            var min = positions[0];
            var max = positions[0];

            foreach (var pos in positions)
            {
                min = Vector3I.Min(min, pos);
                max = Vector3I.Max(max, pos);
            }

            return (min, max);
        }

        private float Calculate3DNoise(Vector3I position, (Vector3I min, Vector3I max) bounds, float scale, PatternParameters parameters)
        {
            // Normalize position within bounds
            var size = bounds.max - bounds.min;
            var normalized = new Vector3(
                size.X > 0 ? (float)(position.X - bounds.min.X) / size.X : 0,
                size.Y > 0 ? (float)(position.Y - bounds.min.Y) / size.Y : 0,
                size.Z > 0 ? (float)(position.Z - bounds.min.Z) / size.Z : 0
            );

            // Apply scale and offset
            var scaled = normalized * scale + new Vector3(parameters.Seed * 0.1f);

            // Multi-octave noise for organic patterns
            var noise = 0f;
            var amplitude = 1f;
            var frequency = parameters.Frequency;
            var maxValue = 0f;

            for (var octave = 0; octave < 4; octave++)
            {
                var octaveValue = SimplexNoise(
                    scaled.X * frequency,
                    scaled.Y * frequency,
                    scaled.Z * frequency
                );

                noise += octaveValue * amplitude;
                maxValue += amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }

            // Normalize to 0-1 range
            noise = (noise / maxValue + 1f) * 0.5f;

            // Apply turbulence for more interesting patterns
            var turbulence = parameters.CustomParameters.TryGetValue("turbulence", out var turbObj) 
                ? Convert.ToSingle(turbObj) 
                : 0.2f;
            
            noise += (SimplexNoise(scaled.X * 10, scaled.Y * 10, scaled.Z * 10) * turbulence);
            
            return MathUtils.Clamp(noise, 0f, 1f);
        }

        private float SimplexNoise(float x, float y, float z)
        {
            // Simplified noise function - in production, use a proper Simplex noise implementation
            var n = Math.Sin(x * 12.9898 + y * 78.233 + z * 37.719) * 43758.5453;
            return (float)(n - Math.Floor(n)) * 2f - 1f;
        }

        private int MapNoiseToColorIndex(float noise, int colorCount)
        {
            // Use a non-linear mapping for more interesting distribution
            var adjusted = (float)Math.Pow(noise, 1.5);
            var index = (int)(adjusted * colorCount);
            return MathUtils.Clamp(index, 0, colorCount - 1);
        }

        private void ApplySmoothing(
            Dictionary<Vector3I, int> pattern,
            List<Vector3I> positions,
            int[] colorIndices,
            PatternParameters parameters)
        {
            var smoothingPasses = parameters.CustomParameters.TryGetValue("smoothing", out var smoothObj)
                ? Convert.ToInt32(smoothObj)
                : 1;

            var directions = new[]
            {
                Vector3I.Up, Vector3I.Down,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Forward, Vector3I.Backward
            };

            for (var pass = 0; pass < smoothingPasses; pass++)
            {
                var updates = new Dictionary<Vector3I, int>();

                foreach (var pos in positions)
                {
                    if (!pattern.TryGetValue(pos, out var currentColor))
                        continue;

                    // Count neighboring colors
                    var colorCounts = new Dictionary<int, int> { { currentColor, 1 } };

                    foreach (var dir in directions)
                    {
                        var neighbor = pos + dir;
                        if (pattern.TryGetValue(neighbor, out var neighborColor))
                        {
                            colorCounts[neighborColor] = colorCounts.GetValueOrDefault(neighborColor) + 1;
                        }
                    }

                    // Use most common color (with bias toward current)
                    var dominantColor = colorCounts
                        .OrderByDescending(kvp => kvp.Value)
                        .ThenBy(kvp => kvp.Key == currentColor ? 0 : 1)
                        .First().Key;

                    if (dominantColor != currentColor)
                    {
                        updates[pos] = dominantColor;
                    }
                }

                // Apply updates
                foreach (var update in updates)
                {
                    pattern[update.Key] = update.Value;
                }
            }
        }
    }
}