using System;
using System.Linq;
using VRageMath;
using PaintJob.App.Extensions;

namespace PaintJob.App.Utils
{
    /// <summary>
    /// Generates color schemes based on color theory principles
    /// Adapted from BuildColors for faction-based painting algorithms
    /// </summary>
    public class ColorSchemeGenerator
    {
        private static readonly float[,] _presetRanges = {
            { 0.0f, 1.0f, 0.0f, 1.0f, 0f, 360f }, // Default
            { 0.2f, 0.4f, 0.8f, 0.9f, 0f, 360f }, // Pastel
            { 0.3f, 0.6f, 0.6f, 0.8f, 0f, 360f }, // Soft
            { 0.8f, 1.0f, 0.9f, 1.0f, 0f, 360f }, // Light
            { 0.5f, 1.0f, 0.0f, 0.5f, 0f, 360f }, // Hard
            { 0.5f, 0.7f, 0.6f, 0.8f, 0f, 360f }, // Pale
            { 0.5f, 1.0f, 0.5f, 1.0f, 0f, 360f }, // Vibrant
            { 0.0f, 0.3f, 0.3f, 0.6f, 0f, 60f }, // Muted
            { 0.5f, 0.8f, 0.4f, 0.6f, 30f, 60f }, // Warm
            { 0.2f, 0.5f, 0.4f, 0.7f, 180f, 270f }, // Cool
            { 0.2f, 0.5f, 0.05f, 0.5f, 0f, 360f }, // Dark
            { 0.5f, 1.0f, 0.8f, 1.0f, 0f, 360f } // Lighter
        };

        private readonly Random _random = new Random();

        public enum Preset
        {
            None,
            Pastel,
            Soft,
            Light,
            Hard,
            Pale,
            Vibrant,
            Muted,
            Warm,
            Cool,
            Dark,
            Lighter
        }

        public enum Scheme
        {
            Default,
            Analogous,
            Complementary,
            Monochromatic,
            Tetradic,
            Triadic
        }

        /// <summary>
        /// Generates a color scheme in Space Engineers Color Mask format
        /// </summary>
        public Vector3[] GenerateColorMasks(Vector3? baseColorMask = null, Scheme scheme = Scheme.Complementary, Preset preset = Preset.None)
        {
            // Convert from Color Mask to HSV for manipulation
            var baseHsv = baseColorMask?.ColorMaskToHSV() ?? GetRandomHSV();
            
            // Apply preset adjustments
            var adjustedHsv = ApplyPreset(baseHsv, preset);
            
            // Generate color scheme
            var hsvColors = GenerateScheme(adjustedHsv, scheme);
            
            // Convert back to Color Mask format
            return hsvColors.Select(hsv => hsv.HSVToColorMask()).ToArray();
        }

        /// <summary>
        /// Generates faction-specific color palettes
        /// </summary>
        public Vector3[] GenerateFactionPalette(string factionType)
        {
            switch (factionType.ToLower())
            {
                case "borg":
                    // Dark metallic greens with neon accent
                    var borgBase = new Vector3(120f / 360f, 0.8f, 0.3f); // Dark green
                    return GenerateColorMasks(borgBase.HSVToColorMask(), Scheme.Monochromatic, Preset.Dark);
                    
                case "federation":
                    // Blue-grey with bright accents
                    var fedBase = new Vector3(210f / 360f, 0.4f, 0.7f); // Blue-grey
                    return GenerateColorMasks(fedBase.HSVToColorMask(), Scheme.Complementary, Preset.Soft);
                    
                case "klingon":
                    // Dark reds and metallics
                    var klingonBase = new Vector3(0f, 0.9f, 0.4f); // Dark red
                    return GenerateColorMasks(klingonBase.HSVToColorMask(), Scheme.Analogous, Preset.Dark);
                    
                case "military":
                    // Olive and camo colors
                    var militaryBase = new Vector3(80f / 360f, 0.6f, 0.4f); // Olive
                    return GenerateColorMasks(militaryBase.HSVToColorMask(), Scheme.Analogous, Preset.Muted);
                    
                default:
                    return GenerateColorMasks(null, Scheme.Default, Preset.None);
            }
        }

        private Vector3[] GenerateScheme(Vector3 baseHsv, Scheme scheme)
        {
            switch (scheme)
            {
                case Scheme.Analogous:
                    return GenerateAnalogousScheme(baseHsv);
                case Scheme.Complementary:
                    return GenerateComplementaryScheme(baseHsv);
                case Scheme.Monochromatic:
                    return GenerateMonochromaticScheme(baseHsv);
                case Scheme.Tetradic:
                    return GenerateTetradicScheme(baseHsv);
                case Scheme.Triadic:
                    return GenerateTriadicScheme(baseHsv);
                default:
                    return GenerateDefaultScheme(baseHsv);
            }
        }

        private Vector3[] GenerateAnalogousScheme(Vector3 color, int count = 5)
        {
            var colors = new Vector3[count];
            colors[0] = color;
            
            var angleStep = 30f / 360f; // 30 degrees in normalized space
            
            for (var i = 1; i < count; i++)
            {
                var hue = color.X + (i % 2 == 0 ? -1 : 1) * ((i + 1) / 2) * angleStep;
                hue = hue < 0 ? hue + 1 : (hue > 1 ? hue - 1 : hue);
                
                // Vary saturation and value slightly
                var sat = Math.Max(0, Math.Min(1, color.Y + (float)Math.Sin(i) * 0.1f));
                var val = Math.Max(0, Math.Min(1, color.Z + (float)Math.Cos(i) * 0.1f));
                
                colors[i] = new Vector3(hue, sat, val);
            }
            
            return colors;
        }

        private Vector3[] GenerateComplementaryScheme(Vector3 color, int count = 6)
        {
            var colors = new Vector3[count];
            
            // Base color variations
            for (var i = 0; i < count / 2; i++)
            {
                var variation = i * 0.15f;
                colors[i] = new Vector3(
                    color.X,
                    Math.Max(0, Math.Min(1, color.Y - variation)),
                    Math.Max(0, Math.Min(1, color.Z + variation))
                );
            }
            
            // Complementary color (180 degrees)
            var compHue = (color.X + 0.5f) % 1f;
            
            // Complementary variations
            for (var i = count / 2; i < count; i++)
            {
                var variation = (i - count / 2) * 0.15f;
                colors[i] = new Vector3(
                    compHue,
                    Math.Max(0, Math.Min(1, color.Y - variation)),
                    Math.Max(0, Math.Min(1, color.Z + variation))
                );
            }
            
            return colors;
        }

        private Vector3[] GenerateMonochromaticScheme(Vector3 color, int count = 5)
        {
            var colors = new Vector3[count];
            
            var minValue = Math.Max(0.1f, color.Z - 0.4f);
            var maxValue = Math.Min(1f, color.Z + 0.4f);
            var valueStep = (maxValue - minValue) / (count - 1);
            
            for (var i = 0; i < count; i++)
            {
                var value = minValue + i * valueStep;
                // Also vary saturation slightly
                var sat = color.Y * (0.7f + 0.3f * (float)i / count);
                colors[i] = new Vector3(color.X, sat, value);
            }
            
            return colors;
        }

        private Vector3[] GenerateTetradicScheme(Vector3 color, int count = 8)
        {
            var colors = new Vector3[count];
            
            // Four base colors at 90 degree intervals
            var baseColors = new Vector3[4];
            for (var i = 0; i < 4; i++)
            {
                var hue = (color.X + i * 0.25f) % 1f;
                baseColors[i] = new Vector3(hue, color.Y, color.Z);
            }
            
            // Create variations of each base
            var colorIndex = 0;
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < count / 4; j++)
                {
                    if (colorIndex >= count) break;
                    
                    var variation = j * 0.2f;
                    colors[colorIndex++] = new Vector3(
                        baseColors[i].X,
                        Math.Max(0, Math.Min(1, baseColors[i].Y - variation * 0.5f)),
                        Math.Max(0, Math.Min(1, baseColors[i].Z + variation * 0.3f))
                    );
                }
            }
            
            return colors;
        }

        private Vector3[] GenerateTriadicScheme(Vector3 color, int count = 6)
        {
            var colors = new Vector3[count];
            
            // Three base colors at 120 degree intervals
            var baseColors = new Vector3[3];
            for (var i = 0; i < 3; i++)
            {
                var hue = (color.X + i * 0.333f) % 1f;
                baseColors[i] = new Vector3(hue, color.Y, color.Z);
            }
            
            // Create variations
            var colorIndex = 0;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < count / 3; j++)
                {
                    if (colorIndex >= count) break;
                    
                    var variation = j * 0.25f;
                    colors[colorIndex++] = new Vector3(
                        baseColors[i].X,
                        Math.Max(0, Math.Min(1, baseColors[i].Y - variation * 0.4f)),
                        Math.Max(0, Math.Min(1, baseColors[i].Z + variation * 0.2f))
                    );
                }
            }
            
            return colors;
        }

        private Vector3[] GenerateDefaultScheme(Vector3 color)
        {
            // Mix of monochromatic and complementary
            var mono = GenerateMonochromaticScheme(color, 3);
            var comp = GenerateComplementaryScheme(color, 4);
            
            return mono.Concat(comp).ToArray();
        }

        private Vector3 ApplyPreset(Vector3 hsv, Preset preset)
        {
            if (preset == Preset.None) return hsv;
            
            var presetIndex = (int)preset;
            
            var minSat = _presetRanges[presetIndex, 0];
            var maxSat = _presetRanges[presetIndex, 1];
            var minVal = _presetRanges[presetIndex, 2];
            var maxVal = _presetRanges[presetIndex, 3];
            var minHue = _presetRanges[presetIndex, 4] / 360f;
            var maxHue = _presetRanges[presetIndex, 5] / 360f;
            
            // Remap values to preset ranges
            var hue = hsv.X;
            if (maxHue > minHue)
            {
                hue = minHue + (maxHue - minHue) * hsv.X;
            }
            
            var sat = minSat + (maxSat - minSat) * hsv.Y;
            var val = minVal + (maxVal - minVal) * hsv.Z;
            
            return new Vector3(hue, sat, val);
        }

        private Vector3 GetRandomHSV()
        {
            return new Vector3(
                (float)_random.NextDouble(),
                (float)_random.NextDouble(),
                (float)_random.NextDouble()
            );
        }
    }
}