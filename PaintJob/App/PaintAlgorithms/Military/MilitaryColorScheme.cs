using System.Collections.Generic;
using PaintJob.App.Analysis;
using PaintJob.App.Extensions;
using PaintJob.App.PaintAlgorithms.Common;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Military
{
    /// <summary>
    /// Defines the military color scheme configuration with semantic color mappings.
    /// </summary>
    public class MilitaryColorScheme : IColorScheme
    {
        /// <summary>
        /// Color index definitions for military paint scheme.
        /// </summary>
        public enum ColorIndex
        {
            PrimaryHull = 0,        // Dark Green - Primary hull color
            SecondaryHull = 1,      // Olive Green - Secondary hull color
            WeaponSystems = 2,      // Dark Grey - Weapons and utility
            TechnicalAreas = 3,     // Black - Joints and recesses
            CamouflageAccent = 4,   // Dark Brown - Camouflage accent
            DesertVariant = 5,      // Sand Brown - Desert variant
            HazardWarning = 6,      // Warning stripes (yellow/orange)
            NavigationPort = 7,     // Red - Port navigation
            NavigationStarboard = 8,// Green - Starboard navigation
            InteriorSpaces = 9,     // Interior Grey - Interior spaces
            FunctionalDark = 10,    // Dark functional blocks
            FunctionalLight = 11,   // Light functional blocks
            NavigationPortTint = 12,    // Subtle red tint for port adjacent blocks
            NavigationStarboardTint = 13 // Subtle green tint for starboard adjacent blocks
        }

        /// <summary>
        /// Maps functional system types to their designated colors.
        /// </summary>
        public Dictionary<FunctionalClusterAnalyzer.FunctionalSystemType, ColorIndex> SystemColorMap { get; }

        /// <summary>
        /// Maps layer types to their base colors.
        /// </summary>
        public Dictionary<BlockSpatialAnalyzer.LayerType, ColorIndex> LayerColorMap { get; }

        /// <summary>
        /// Gets the color palette for camouflage patterns.
        /// </summary>
        public ColorIndex[] CamouflageColors { get; }

        /// <summary>
        /// Gets the actual color values for the scheme.
        /// </summary>
        public Vector3[] ColorPalette { get; private set; }

        public MilitaryColorScheme()
        {
            SystemColorMap = InitializeSystemColorMap();
            LayerColorMap = InitializeLayerColorMap();
            CamouflageColors = new[] 
            { 
                ColorIndex.PrimaryHull, 
                ColorIndex.SecondaryHull, 
                ColorIndex.CamouflageAccent 
            };
        }

        /// <summary>
        /// Initializes the color palette with actual color values.
        /// </summary>
        public void InitializePalette(Vector3[] generatedColors)
        {
            ColorPalette = BuildColorPalette(generatedColors);
        }

        private Dictionary<FunctionalClusterAnalyzer.FunctionalSystemType, ColorIndex> InitializeSystemColorMap()
        {
            return new Dictionary<FunctionalClusterAnalyzer.FunctionalSystemType, ColorIndex>
            {
                { FunctionalClusterAnalyzer.FunctionalSystemType.OffensiveWeapons, ColorIndex.WeaponSystems },
                { FunctionalClusterAnalyzer.FunctionalSystemType.DefensiveWeapons, ColorIndex.WeaponSystems },
                { FunctionalClusterAnalyzer.FunctionalSystemType.PowerGeneration, ColorIndex.HazardWarning },
                { FunctionalClusterAnalyzer.FunctionalSystemType.PowerStorage, ColorIndex.FunctionalDark },
                { FunctionalClusterAnalyzer.FunctionalSystemType.MainThrusters, ColorIndex.TechnicalAreas },
                { FunctionalClusterAnalyzer.FunctionalSystemType.ManeuveringThrusters, ColorIndex.TechnicalAreas },
                { FunctionalClusterAnalyzer.FunctionalSystemType.CommandAndControl, ColorIndex.InteriorSpaces },
                { FunctionalClusterAnalyzer.FunctionalSystemType.CargoStorage, ColorIndex.DesertVariant },
                { FunctionalClusterAnalyzer.FunctionalSystemType.FluidStorage, ColorIndex.DesertVariant },
                { FunctionalClusterAnalyzer.FunctionalSystemType.Medical, ColorIndex.InteriorSpaces },
                { FunctionalClusterAnalyzer.FunctionalSystemType.Sensors, ColorIndex.TechnicalAreas },
                { FunctionalClusterAnalyzer.FunctionalSystemType.Communication, ColorIndex.TechnicalAreas }
            };
        }

        private Dictionary<BlockSpatialAnalyzer.LayerType, ColorIndex> InitializeLayerColorMap()
        {
            return new Dictionary<BlockSpatialAnalyzer.LayerType, ColorIndex>
            {
                { BlockSpatialAnalyzer.LayerType.Exterior, ColorIndex.PrimaryHull },
                { BlockSpatialAnalyzer.LayerType.Interior, ColorIndex.InteriorSpaces },
                { BlockSpatialAnalyzer.LayerType.Guts, ColorIndex.TechnicalAreas }
            };
        }

        private Vector3[] BuildColorPalette(Vector3[] generatedColors)
        {
            var palette = new Vector3[14]; // Increased to 14 for tint colors
            
            // Use generated colors if available and sufficient
            if (generatedColors != null && generatedColors.Length >= 12)
            {
                // Use all generated colors directly
                for (var i = 0; i < 12; i++)
                {
                    palette[i] = generatedColors[i];
                }
                // Generate tint colors based on navigation lights
                GenerateNavigationTints(palette, generatedColors);
            }
            else if (generatedColors != null && generatedColors.Length >= 5)
            {
                // Partial generation - use what we have and generate the rest
                palette[(int)ColorIndex.PrimaryHull] = generatedColors[0];
                palette[(int)ColorIndex.SecondaryHull] = generatedColors[1];
                palette[(int)ColorIndex.CamouflageAccent] = generatedColors[2];
                palette[(int)ColorIndex.DesertVariant] = generatedColors[3];
                palette[(int)ColorIndex.WeaponSystems] = generatedColors[4];
                
                // Generate remaining colors based on the base palette
                GenerateRemainingColors(palette, generatedColors);
                GenerateNavigationTints(palette, generatedColors);
            }
            else
            {
                // Full fallback - generate all colors with a preset seed
                var generator = new Utils.ColorSchemeGenerator(42); // Fixed seed for consistency
                var fullPalette = generator.GenerateMilitaryPalette();
                for (var i = 0; i < 12; i++)
                {
                    palette[i] = fullPalette[i];
                }
                GenerateNavigationTints(palette, fullPalette);
            }

            return palette;
        }
        
        private void GenerateRemainingColors(Vector3[] palette, Vector3[] baseColors)
        {
            // Generate remaining functional colors based on the base military colors
            var baseHsv = baseColors[0].ColorMaskToHSV();
            
            // Technical areas - very dark version of primary
            palette[(int)ColorIndex.TechnicalAreas] = new Vector3(baseHsv.X, baseHsv.Y * 0.5f, baseHsv.Z * 0.2f).HSVToColorMask();
            
            // Hazard warning - high saturation yellow/orange
            palette[(int)ColorIndex.HazardWarning] = new Vector3(0.092f, 1f, 0.9f).HSVToColorMask();
            
            // Navigation lights - subdued military red/green
            palette[(int)ColorIndex.NavigationPort] = new Vector3(0f, 0.6f, 0.35f).HSVToColorMask();
            palette[(int)ColorIndex.NavigationStarboard] = new Vector3(0.333f, 0.5f, 0.3f).HSVToColorMask();
            
            // Interior - neutral grey based on base brightness
            palette[(int)ColorIndex.InteriorSpaces] = new Vector3(baseHsv.X, baseHsv.Y * 0.1f, 0.5f).HSVToColorMask();
            
            // Functional blocks - variations of grey
            palette[(int)ColorIndex.FunctionalDark] = new Vector3(baseHsv.X, baseHsv.Y * 0.2f, 0.15f).HSVToColorMask();
            palette[(int)ColorIndex.FunctionalLight] = new Vector3(baseHsv.X, baseHsv.Y * 0.15f, 0.4f).HSVToColorMask();
        }
        
        private void GenerateNavigationTints(Vector3[] palette, Vector3[] sourceColors)
        {
            // Get the port and starboard colors and hull color
            var portHsv = palette[(int)ColorIndex.NavigationPort].ColorMaskToHSV();
            var starboardHsv = palette[(int)ColorIndex.NavigationStarboard].ColorMaskToHSV();
            var hullHsv = palette[(int)ColorIndex.PrimaryHull].ColorMaskToHSV();
            
            // Create tinted versions by blending navigation colors with hull color
            // Port tint: 80% hull color, 20% port color
            var portTintHsv = new Vector3(
                portHsv.X, // Keep the hue from port
                hullHsv.Y * 0.8f + portHsv.Y * 0.2f, // Blend saturation
                hullHsv.Z * 0.85f + portHsv.Z * 0.15f // Blend value, slightly darker
            );
            
            // Starboard tint: 80% hull color, 20% starboard color  
            var starboardTintHsv = new Vector3(
                starboardHsv.X, // Keep the hue from starboard
                hullHsv.Y * 0.8f + starboardHsv.Y * 0.2f, // Blend saturation
                hullHsv.Z * 0.85f + starboardHsv.Z * 0.15f // Blend value, slightly darker
            );
            
            palette[(int)ColorIndex.NavigationPortTint] = portTintHsv.HSVToColorMask();
            palette[(int)ColorIndex.NavigationStarboardTint] = starboardTintHsv.HSVToColorMask();
        }

        /// <summary>
        /// Gets the color index for a specific functional system type.
        /// </summary>
        public ColorIndex GetSystemColor(FunctionalClusterAnalyzer.FunctionalSystemType systemType, bool isPrimary = false)
        {
            if (SystemColorMap.TryGetValue(systemType, out var colorIndex))
            {
                // Special handling for primary power systems
                if (isPrimary && (systemType == FunctionalClusterAnalyzer.FunctionalSystemType.PowerGeneration ||
                                  systemType == FunctionalClusterAnalyzer.FunctionalSystemType.PowerStorage))
                {
                    return ColorIndex.HazardWarning;
                }
                return colorIndex;
            }
            return ColorIndex.FunctionalDark;
        }

        /// <summary>
        /// Gets the base color index for a specific layer type.
        /// </summary>
        public ColorIndex GetLayerColor(BlockSpatialAnalyzer.LayerType layerType)
        {
            return LayerColorMap.TryGetValue(layerType, out var colorIndex) 
                ? colorIndex 
                : ColorIndex.PrimaryHull;
        }
    }
}