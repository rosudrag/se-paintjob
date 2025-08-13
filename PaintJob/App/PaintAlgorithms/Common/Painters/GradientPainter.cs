using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Common.Painters
{
    public class GradientPainter
    {
        private readonly Random _random;

        public GradientPainter(int seed)
        {
            _random = new Random(seed);
        }

        public enum GradientType
        {
            Linear,
            Radial,
            Spherical,
            Cylindrical,
            Wave,
            Diamond,
            Spiral
        }

        public void ApplyGradient(
            HashSet<MySlimBlock> blocks,
            Dictionary<Vector3I, int> colorResults,
            GradientType type,
            int startColorIndex,
            int endColorIndex,
            Vector3? customCenter = null,
            Vector3? direction = null)
        {
            var bounds = CalculateBounds(blocks);
            var center = customCenter ?? new Vector3(
                (bounds.Min.X + bounds.Max.X) / 2f,
                (bounds.Min.Y + bounds.Max.Y) / 2f,
                (bounds.Min.Z + bounds.Max.Z) / 2f
            );

            var maxDistance = Vector3.Distance(bounds.Min, bounds.Max);

            foreach (var block in blocks)
            {
                var pos = new Vector3(block.Position);
                float gradientValue = 0f;

                switch (type)
                {
                    case GradientType.Linear:
                        gradientValue = CalculateLinearGradient(pos, bounds, direction ?? Vector3.Forward);
                        break;

                    case GradientType.Radial:
                        gradientValue = CalculateRadialGradient(pos, center, maxDistance);
                        break;

                    case GradientType.Spherical:
                        gradientValue = CalculateSphericalGradient(pos, center, maxDistance);
                        break;

                    case GradientType.Cylindrical:
                        gradientValue = CalculateCylindricalGradient(pos, center, bounds, direction ?? Vector3.Up);
                        break;

                    case GradientType.Wave:
                        gradientValue = CalculateWaveGradient(pos, center, maxDistance);
                        break;

                    case GradientType.Diamond:
                        gradientValue = CalculateDiamondGradient(pos, center, bounds);
                        break;

                    case GradientType.Spiral:
                        gradientValue = CalculateSpiralGradient(pos, center, maxDistance);
                        break;
                }

                // Interpolate between start and end colors
                var colorIndex = InterpolateColor(gradientValue, startColorIndex, endColorIndex);
                
                // Only apply if not already colored or with some probability for blending
                if (!colorResults.ContainsKey(block.Position) || _random.NextDouble() > 0.3)
                {
                    colorResults[block.Position] = colorIndex;
                }
            }
        }

        private float CalculateLinearGradient(Vector3 pos, BoundingBox bounds, Vector3 direction)
        {
            direction.Normalize();
            var size = bounds.Max - bounds.Min;
            var relativePos = pos - bounds.Min;
            
            // Project position onto direction vector
            var projection = Vector3.Dot(relativePos, direction);
            var maxProjection = Vector3.Dot(size, Abs(direction));
            
            return System.Math.Max(0, System.Math.Min(1, projection / maxProjection));
        }

        private float CalculateRadialGradient(Vector3 pos, Vector3 center, float maxDistance)
        {
            var distance = Vector3.Distance(pos, center);
            return System.Math.Min(1f, distance / (maxDistance * 0.5f));
        }

        private float CalculateSphericalGradient(Vector3 pos, Vector3 center, float maxDistance)
        {
            var distance = Vector3.Distance(pos, center);
            var normalized = distance / (maxDistance * 0.5f);
            
            // Create spherical layers
            return (float)(System.Math.Sin(normalized * System.Math.PI) * 0.5 + 0.5);
        }

        private float CalculateCylindricalGradient(Vector3 pos, Vector3 center, BoundingBox bounds, Vector3 axis)
        {
            axis.Normalize();
            var toPos = pos - center;
            
            // Project onto plane perpendicular to axis
            var projection = toPos - axis * Vector3.Dot(toPos, axis);
            var distance = projection.Length();
            
            var maxRadius = System.Math.Max(
                System.Math.Abs(bounds.Max.X - bounds.Min.X),
                System.Math.Max(
                    System.Math.Abs(bounds.Max.Y - bounds.Min.Y),
                    System.Math.Abs(bounds.Max.Z - bounds.Min.Z)
                )
            ) * 0.5f;
            
            return System.Math.Min(1f, distance / maxRadius);
        }

        private float CalculateWaveGradient(Vector3 pos, Vector3 center, float maxDistance)
        {
            var distance = Vector3.Distance(pos, center);
            var normalized = distance / (maxDistance * 0.5f);
            
            // Create wave pattern
            var wave = (float)(System.Math.Sin(normalized * System.Math.PI * 4) * 0.5 + 0.5);
            return wave;
        }

        private float CalculateDiamondGradient(Vector3 pos, Vector3 center, BoundingBox bounds)
        {
            var relPos = pos - center;
            var size = bounds.Max - bounds.Min;
            
            // Manhattan distance normalized
            var manhattan = System.Math.Abs(relPos.X) / (size.X * 0.5f) +
                          System.Math.Abs(relPos.Y) / (size.Y * 0.5f) +
                          System.Math.Abs(relPos.Z) / (size.Z * 0.5f);
            
            return System.Math.Min(1f, manhattan / 3f);
        }

        private float CalculateSpiralGradient(Vector3 pos, Vector3 center, float maxDistance)
        {
            var relPos = pos - center;
            var distance = relPos.Length();
            var angle = System.Math.Atan2(relPos.Y, relPos.X);
            
            // Create spiral effect
            var spiral = (float)((angle / (2 * System.Math.PI) + distance / maxDistance * 2) % 1.0);
            return spiral;
        }

        private int InterpolateColor(float gradientValue, int startIndex, int endIndex)
        {
            // Ensure gradient value is between 0 and 1
            gradientValue = System.Math.Max(0, System.Math.Min(1, gradientValue));
            
            if (startIndex == endIndex)
                return startIndex;
            
            // Simple linear interpolation between color indices
            var range = endIndex - startIndex;
            var interpolated = startIndex + (int)(gradientValue * range);
            
            return System.Math.Max(startIndex, System.Math.Min(endIndex, interpolated));
        }

        private BoundingBox CalculateBounds(HashSet<MySlimBlock> blocks)
        {
            if (blocks.Count == 0)
                return new BoundingBox();
            
            var first = blocks.First();
            var min = new Vector3(first.Position);
            var max = new Vector3(first.Position);
            
            foreach (var block in blocks)
            {
                min = Vector3.Min(min, block.Position);
                max = Vector3.Max(max, block.Position);
            }
            
            return new BoundingBox(min, max);
        }

        private static Vector3 Abs(Vector3 v)
        {
            return new Vector3(System.Math.Abs(v.X), System.Math.Abs(v.Y), System.Math.Abs(v.Z));
        }
    }
}