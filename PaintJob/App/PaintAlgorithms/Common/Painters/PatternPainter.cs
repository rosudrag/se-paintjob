using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Common.Painters
{
    public class PatternPainter
    {
        private readonly Random _random;

        public PatternPainter(int seed)
        {
            _random = new Random(seed);
        }

        public enum PatternType
        {
            Stripes,
            Checkerboard,
            Dots,
            Hexagons,
            Triangles,
            Waves,
            Zigzag,
            Camo,
            Scales,
            Circuit,
            Honeycomb,
            Fractal
        }

        public void ApplyPattern(
            HashSet<MySlimBlock> blocks,
            Dictionary<Vector3I, int> colorResults,
            PatternType type,
            int primaryColor,
            int secondaryColor,
            float scale = 1f,
            Vector3? direction = null)
        {
            var bounds = CalculateBounds(blocks);
            var patternDirection = direction ?? Vector3.Forward;
            patternDirection.Normalize();

            foreach (var block in blocks)
            {
                var shouldApplyPattern = false;
                var useSecondary = false;

                switch (type)
                {
                    case PatternType.Stripes:
                        (shouldApplyPattern, useSecondary) = CalculateStripes(block.Position, bounds, patternDirection, scale);
                        break;

                    case PatternType.Checkerboard:
                        (shouldApplyPattern, useSecondary) = CalculateCheckerboard(block.Position, scale);
                        break;

                    case PatternType.Dots:
                        (shouldApplyPattern, useSecondary) = CalculateDots(block.Position, scale);
                        break;

                    case PatternType.Hexagons:
                        (shouldApplyPattern, useSecondary) = CalculateHexagons(block.Position, scale);
                        break;

                    case PatternType.Triangles:
                        (shouldApplyPattern, useSecondary) = CalculateTriangles(block.Position, scale);
                        break;

                    case PatternType.Waves:
                        (shouldApplyPattern, useSecondary) = CalculateWaves(block.Position, bounds, scale);
                        break;

                    case PatternType.Zigzag:
                        (shouldApplyPattern, useSecondary) = CalculateZigzag(block.Position, bounds, scale);
                        break;

                    case PatternType.Camo:
                        (shouldApplyPattern, useSecondary) = CalculateCamo(block.Position, scale);
                        break;

                    case PatternType.Scales:
                        (shouldApplyPattern, useSecondary) = CalculateScales(block.Position, scale);
                        break;

                    case PatternType.Circuit:
                        (shouldApplyPattern, useSecondary) = CalculateCircuit(block.Position, bounds);
                        break;

                    case PatternType.Honeycomb:
                        (shouldApplyPattern, useSecondary) = CalculateHoneycomb(block.Position, scale);
                        break;

                    case PatternType.Fractal:
                        (shouldApplyPattern, useSecondary) = CalculateFractal(block.Position, bounds, scale);
                        break;
                }

                if (shouldApplyPattern)
                {
                    colorResults[block.Position] = useSecondary ? secondaryColor : primaryColor;
                }
            }
        }

        private (bool apply, bool useSecondary) CalculateStripes(Vector3I pos, BoundingBox bounds, Vector3 direction, float scale)
        {
            // Project position onto perpendicular plane
            var relPos = new Vector3(pos) - bounds.Min;
            float projection;

            if (Math.Abs(direction.X) > 0.5f)
                projection = relPos.Y + relPos.Z;
            else if (Math.Abs(direction.Y) > 0.5f)
                projection = relPos.X + relPos.Z;
            else
                projection = relPos.X + relPos.Y;

            var stripeWidth = 3f * scale;
            var stripeIndex = (int)(projection / stripeWidth);
            
            return (true, stripeIndex % 2 == 0);
        }

        private (bool apply, bool useSecondary) CalculateCheckerboard(Vector3I pos, float scale)
        {
            var checkSize = (int)(3 * scale);
            var x = pos.X / checkSize;
            var y = pos.Y / checkSize;
            var z = pos.Z / checkSize;
            
            var sum = x + y + z;
            return (true, sum % 2 == 0);
        }

        private (bool apply, bool useSecondary) CalculateDots(Vector3I pos, float scale)
        {
            var dotSpacing = (int)(4 * scale);
            var dotSize = (int)(1.5f * scale);
            
            var modX = Math.Abs(pos.X % dotSpacing);
            var modY = Math.Abs(pos.Y % dotSpacing);
            var modZ = Math.Abs(pos.Z % dotSpacing);
            
            var isNearDot = (modX < dotSize && modY < dotSize) ||
                           (modY < dotSize && modZ < dotSize) ||
                           (modX < dotSize && modZ < dotSize);
            
            return (isNearDot, false);
        }

        private (bool apply, bool useSecondary) CalculateHexagons(Vector3I pos, float scale)
        {
            // Simplified hexagonal grid
            var hexSize = 3f * scale;
            var x = pos.X / hexSize;
            var y = pos.Y / hexSize;
            
            // Create hexagonal pattern
            var row = (int)y;
            var col = (int)(x + (row % 2) * 0.5f);
            
            return (true, (row + col) % 2 == 0);
        }

        private (bool apply, bool useSecondary) CalculateTriangles(Vector3I pos, float scale)
        {
            var triSize = 4f * scale;
            
            // Create triangular tessellation
            var x = pos.X / triSize;
            var y = pos.Y / triSize;
            var z = pos.Z / triSize;
            
            var triIndex = (int)(x + y * 0.5f) + (int)(z * 0.3f);
            
            return (true, triIndex % 3 != 0);
        }

        private (bool apply, bool useSecondary) CalculateWaves(Vector3I pos, BoundingBox bounds, float scale)
        {
            var waveLength = 8f * scale;
            var amplitude = 2f * scale;
            
            var relX = (pos.X - bounds.Min.X) / waveLength;
            var waveY = Math.Sin(relX * Math.PI * 2) * amplitude;
            
            var distance = Math.Abs(pos.Y - bounds.Min.Y - waveY);
            
            return (distance < amplitude, distance < amplitude * 0.5f);
        }

        private (bool apply, bool useSecondary) CalculateZigzag(Vector3I pos, BoundingBox bounds, float scale)
        {
            var zigzagPeriod = 6f * scale;
            var zigzagAmplitude = 3f * scale;
            
            var relX = (pos.X - bounds.Min.X) / zigzagPeriod;
            var phase = relX % 1.0f;
            
            float zigzagY;
            if (phase < 0.5f)
                zigzagY = phase * 2 * zigzagAmplitude;
            else
                zigzagY = (1 - (phase - 0.5f) * 2) * zigzagAmplitude;
            
            var distance = Math.Abs(pos.Y - bounds.Min.Y - zigzagY);
            
            return (distance < scale, false);
        }

        private (bool apply, bool useSecondary) CalculateCamo(Vector3I pos, float scale)
        {
            // Create blob-like camo pattern using multiple overlapping circles
            var blobSize = 5f * scale;
            
            // Generate pseudo-random blob centers based on position
            var seed = pos.X * 73 + pos.Y * 149 + pos.Z * 283;
            var localRandom = new Random(seed);
            
            var noise1 = SimplexNoise(pos.X / blobSize, pos.Y / blobSize, pos.Z / blobSize);
            var noise2 = SimplexNoise(pos.X / (blobSize * 0.5f), pos.Y / (blobSize * 0.5f), pos.Z / (blobSize * 0.5f));
            
            var combined = noise1 * 0.7f + noise2 * 0.3f;
            
            return (true, combined > 0);
        }

        private (bool apply, bool useSecondary) CalculateScales(Vector3I pos, float scale)
        {
            var scaleSize = 3f * scale;
            
            // Create overlapping scale pattern
            var row = (int)(pos.Y / scaleSize);
            var col = (int)(pos.X / scaleSize + (row % 2) * 0.5f);
            
            var localX = pos.X % scaleSize;
            var localY = pos.Y % scaleSize;
            
            // Scale shape (semi-circle like)
            var centerX = scaleSize * 0.5f;
            var dist = Math.Sqrt((localX - centerX) * (localX - centerX) + localY * localY);
            
            var isScale = dist < scaleSize * 0.7f;
            
            return (isScale, (row + col) % 2 == 0);
        }

        private (bool apply, bool useSecondary) CalculateCircuit(Vector3I pos, BoundingBox bounds)
        {
            // Create circuit board-like pattern
            var gridSize = 2;
            
            // Main traces along axes
            var onXTrace = pos.Y % gridSize == 0 || pos.Z % gridSize == 0;
            var onYTrace = pos.X % gridSize == 0 || pos.Z % gridSize == 0;
            var onZTrace = pos.X % gridSize == 0 || pos.Y % gridSize == 0;
            
            // Connection points (vias)
            var isVia = pos.X % 4 == 0 && pos.Y % 4 == 0 && pos.Z % 4 == 0;
            
            var isCircuit = onXTrace || onYTrace || onZTrace || isVia;
            
            return (isCircuit, isVia);
        }

        private (bool apply, bool useSecondary) CalculateHoneycomb(Vector3I pos, float scale)
        {
            // Honeycomb hexagonal pattern
            var hexSize = 3f * scale;
            var sqrt3 = 1.732f;
            
            var x = pos.X;
            var y = pos.Y * sqrt3;
            
            var row = (int)(y / (hexSize * sqrt3));
            var col = (int)(x / (hexSize * 1.5f));
            
            var localX = x % (hexSize * 1.5f);
            var localY = y % (hexSize * sqrt3);
            
            // Determine if on hexagon edge
            var onEdge = localX < scale * 0.5f || localY < scale * 0.5f;
            
            return (onEdge, false);
        }

        private (bool apply, bool useSecondary) CalculateFractal(Vector3I pos, BoundingBox bounds, float scale)
        {
            // Sierpinski triangle-like fractal pattern
            var level = 4;
            var size = (int)(Math.Pow(2, level) * scale);
            
            var x = Math.Abs(pos.X) % size;
            var y = Math.Abs(pos.Y) % size;
            var z = Math.Abs(pos.Z) % size;
            
            // Check if position is in fractal
            while (x > 0 || y > 0 || z > 0)
            {
                if ((x & 1) == 1 && (y & 1) == 1)
                    return (false, false);
                if ((y & 1) == 1 && (z & 1) == 1)
                    return (false, false);
                if ((x & 1) == 1 && (z & 1) == 1)
                    return (false, false);
                    
                x >>= 1;
                y >>= 1;
                z >>= 1;
            }
            
            return (true, false);
        }

        private float SimplexNoise(float x, float y, float z)
        {
            // Simplified noise function
            var n = Math.Sin(x * 2.1f) * 1000 + Math.Sin(y * 3.3f) * 1000 + Math.Sin(z * 4.7f) * 1000;
            return (float)(n - Math.Floor(n)) * 2f - 1f;
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
    }
}