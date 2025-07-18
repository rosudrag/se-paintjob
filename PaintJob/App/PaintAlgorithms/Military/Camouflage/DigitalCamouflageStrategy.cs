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
    /// Implements a digital/pixelated camouflage pattern similar to modern military designs.
    /// </summary>
    public class DigitalCamouflageStrategy : IPatternStrategy
    {
        public string Name => "Digital";

        public Dictionary<Vector3I, int> GeneratePattern(
            MyCubeGrid grid,
            IEnumerable<Vector3I> positions,
            int[] colorIndices,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var random = new Random(parameters.Seed);
            var positionsList = positions.ToList();

            if (colorIndices.Length == 0 || positionsList.Count == 0)
                return result;

            // Create digital pattern cells
            var cellSize = (int)Math.Max(1, parameters.Scale);
            var cells = new Dictionary<Vector3I, int>();

            // Generate cell colors
            foreach (var pos in positionsList)
            {
                var cellPos = new Vector3I(
                    pos.X / cellSize,
                    pos.Y / cellSize,
                    pos.Z / cellSize
                );

                if (!cells.ContainsKey(cellPos))
                {
                    // Use noise function for more natural distribution
                    var noise = CalculateNoise(cellPos, parameters.Frequency, random);
                    var colorIndex = (int)(noise * colorIndices.Length);
                    colorIndex = MathUtils.Clamp(colorIndex, 0, colorIndices.Length - 1);
                    cells[cellPos] = colorIndices[colorIndex];
                }

                result[pos] = cells[cellPos];
            }

            // Apply sub-cell variations for more realistic digital camo
            ApplySubCellVariations(result, colorIndices, parameters, random);

            return result;
        }

        private float CalculateNoise(Vector3I position, float frequency, Random random)
        {
            // Multi-octave noise for more interesting patterns
            var noise = 0f;
            var amplitude = 1f;
            var maxValue = 0f;
            var freq = frequency;

            for (var i = 0; i < 3; i++)
            {
                // Scale position by frequency for this octave
                var scaledX = position.X * freq;
                var scaledY = position.Y * freq;
                var scaledZ = position.Z * freq;
                
                // Generate deterministic pseudo-random value based on scaled position
                var hash = unchecked((int)(scaledX * 374761393 + scaledY * 668265263 + scaledZ * 1274126177));
                var octaveRandom = new Random(hash);
                var sample = (float)octaveRandom.NextDouble();
                
                noise += sample * amplitude;
                maxValue += amplitude;
                amplitude *= 0.5f;
                freq *= 2f;
            }

            return noise / maxValue;
        }

        private void ApplySubCellVariations(
            Dictionary<Vector3I, int> pattern,
            int[] colorIndices,
            PatternParameters parameters,
            Random random)
        {
            // Add small patches of different colors within cells
            var positions = pattern.Keys.ToList();
            var variationChance = 0.15f; // 15% chance of variation

            foreach (var pos in positions)
            {
                if (random.NextDouble() < variationChance)
                {
                    var currentColor = pattern[pos];
                    var currentIndex = Array.IndexOf(colorIndices, currentColor);
                    
                    // Pick adjacent color in the palette
                    var offset = random.Next(-1, 2);
                    var newIndex = MathHelper.Clamp(currentIndex + offset, 0, colorIndices.Length - 1);
                    
                    if (newIndex != currentIndex)
                    {
                        pattern[pos] = colorIndices[newIndex];
                    }
                }
            }
        }
    }
}