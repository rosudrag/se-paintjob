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
    /// Implements a hexagonal camouflage pattern suitable for angular ships.
    /// </summary>
    public class HexagonalCamouflageStrategy : IPatternStrategy
    {
        public string Name => "Hexagonal";

        private const float Sqrt3 = MathF.Sqrt(3);

        public Dictionary<Vector3I, int> GeneratePattern(
            MyCubeGrid grid,
            IEnumerable<Vector3I> positions,
            int[] colorIndices,
            PatternParameters parameters)
        {
            var result = new Dictionary<Vector3I, int>();
            var random = new Random(parameters.Seed);
            var hexSize = parameters.Scale;

            if (colorIndices.Length == 0)
                return result;

            // Pre-calculate hex centers and their colors
            var hexColors = new Dictionary<Vector2, int>();
            var processedHexes = new HashSet<Vector2>();

            foreach (var pos in positions)
            {
                // Project to 2D for hex calculation (using the most visible plane)
                var pos2D = GetDominantPlaneProjection(pos, parameters.Origin);
                var hexCoord = PixelToHex(pos2D, hexSize);
                var hexCenter = HexToPixel(hexCoord, hexSize);

                if (!processedHexes.Contains(hexCenter))
                {
                    processedHexes.Add(hexCenter);
                    
                    // Generate color based on hex coordinate
                    var noise = GenerateHexNoise(hexCoord, parameters.Frequency, random);
                    var colorIndex = (int)(noise * colorIndices.Length);
                    colorIndex = MathUtils.Clamp(colorIndex, 0, colorIndices.Length - 1);
                    hexColors[hexCenter] = colorIndices[colorIndex];
                }

                result[pos] = hexColors[hexCenter];
            }

            // Add edge blending between hexagons
            ApplyHexagonEdgeBlending(result, positions, colorIndices, parameters, hexSize);

            return result;
        }

        private Vector2 GetDominantPlaneProjection(Vector3I pos, Vector3 origin)
        {
            // Project onto the plane that provides best visibility
            var relativePos = pos - origin;
            var absPos = Vector3.Abs(relativePos);

            if (absPos.X >= absPos.Y && absPos.X >= absPos.Z)
            {
                // YZ plane
                return new Vector2(pos.Y, pos.Z);
            }
            else if (absPos.Y >= absPos.X && absPos.Y >= absPos.Z)
            {
                // XZ plane
                return new Vector2(pos.X, pos.Z);
            }
            else
            {
                // XY plane
                return new Vector2(pos.X, pos.Y);
            }
        }

        private Vector2 PixelToHex(Vector2 point, float size)
        {
            var q = (Sqrt3 / 3f * point.X - 1f / 3f * point.Y) / size;
            var r = (2f / 3f * point.Y) / size;
            return HexRound(new Vector2(q, r));
        }

        private Vector2 HexToPixel(Vector2 hex, float size)
        {
            var x = size * (Sqrt3 * hex.X + Sqrt3 / 2f * hex.Y);
            var y = size * (3f / 2f * hex.Y);
            return new Vector2(x, y);
        }

        private Vector2 HexRound(Vector2 hex)
        {
            var q = (int)Math.Round(hex.X);
            var r = (int)Math.Round(hex.Y);
            var s = (int)Math.Round(-hex.X - hex.Y);

            var qDiff = Math.Abs(q - hex.X);
            var rDiff = Math.Abs(r - hex.Y);
            var sDiff = Math.Abs(s - (-hex.X - hex.Y));

            if (qDiff > rDiff && qDiff > sDiff)
            {
                q = -r - s;
            }
            else if (rDiff > sDiff)
            {
                r = -q - s;
            }

            return new Vector2(q, r);
        }

        private float GenerateHexNoise(Vector2 hexCoord, float frequency, Random random)
        {
            // Create interesting patterns using hex coordinates
            var hash = hexCoord.GetHashCode();
            random = new Random(hash);

            // Layer multiple noise octaves
            var noise = 0f;
            var amplitude = 1f;
            var maxValue = 0f;

            for (var octave = 0; octave < 3; octave++)
            {
                var sampleX = hexCoord.X * frequency * Math.Pow(2, octave);
                var sampleY = hexCoord.Y * frequency * Math.Pow(2, octave);
                
                // Use trigonometric functions for smooth transitions
                var value = (float)(Math.Sin(sampleX * 0.1) * Math.Cos(sampleY * 0.1) + 1) * 0.5f;
                value = (value + (float)random.NextDouble()) * 0.5f;
                
                noise += value * amplitude;
                maxValue += amplitude;
                amplitude *= 0.5f;
            }

            return noise / maxValue;
        }

        private void ApplyHexagonEdgeBlending(
            Dictionary<Vector3I, int> pattern,
            IEnumerable<Vector3I> positions,
            int[] colorIndices,
            PatternParameters parameters,
            float hexSize)
        {
            // Add transition zones between hexagons for more realistic appearance
            var positionsList = positions.ToList();
            var random = new Random(parameters.Seed + 1);

            foreach (var pos in positionsList)
            {
                var pos2D = GetDominantPlaneProjection(pos, parameters.Origin);
                var hexCoord = PixelToHex(pos2D, hexSize);
                var hexCenter = HexToPixel(hexCoord, hexSize);
                
                var distanceToCenter = Vector2.Distance(pos2D, hexCenter);
                var edgeThreshold = hexSize * 0.8f;
                
                if (distanceToCenter > edgeThreshold && random.NextDouble() < 0.3)
                {
                    // Near edge - potentially blend with neighboring hex color
                    var currentColor = pattern[pos];
                    var currentIndex = Array.IndexOf(colorIndices, currentColor);
                    
                    if (currentIndex > 0 && currentIndex < colorIndices.Length - 1)
                    {
                        var blend = random.Next(-1, 2);
                        var newIndex = MathUtils.Clamp(currentIndex + blend, 0, colorIndices.Length - 1);
                        pattern[pos] = colorIndices[newIndex];
                    }
                }
            }
        }
    }
}