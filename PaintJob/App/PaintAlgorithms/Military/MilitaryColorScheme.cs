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
            FunctionalLight = 11    // Light functional blocks
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
            var palette = new Vector3[12];
            
            // Use generated colors if available
            if (generatedColors != null && generatedColors.Length >= 5)
            {
                palette[(int)ColorIndex.PrimaryHull] = generatedColors[0];
                palette[(int)ColorIndex.SecondaryHull] = generatedColors[1];
                palette[(int)ColorIndex.CamouflageAccent] = generatedColors[2];
                palette[(int)ColorIndex.DesertVariant] = generatedColors[3];
            }
            else
            {
                // Fallback colors
                palette[(int)ColorIndex.PrimaryHull] = ColorMaskExtensions.CreateColorMask(120, 80, 30);
                palette[(int)ColorIndex.SecondaryHull] = ColorMaskExtensions.CreateColorMask(80, 60, 40);
                palette[(int)ColorIndex.CamouflageAccent] = ColorMaskExtensions.CreateColorMask(30, 70, 25);
                palette[(int)ColorIndex.DesertVariant] = ColorMaskExtensions.CreateColorMask(40, 35, 76);
            }

            // Fixed colors
            palette[(int)ColorIndex.WeaponSystems] = ColorMaskExtensions.CreateGreyMask(30);
            palette[(int)ColorIndex.TechnicalAreas] = ColorMaskExtensions.CreateGreyMask(5);
            palette[(int)ColorIndex.HazardWarning] = ColorMaskExtensions.CreateColorMask(33, 100, 100);
            palette[(int)ColorIndex.NavigationPort] = ColorMaskExtensions.CreateColorMask(0, 100, 80);
            palette[(int)ColorIndex.NavigationStarboard] = ColorMaskExtensions.CreateColorMask(120, 100, 80);
            palette[(int)ColorIndex.InteriorSpaces] = ColorMaskExtensions.CreateGreyMask(50);
            palette[(int)ColorIndex.FunctionalDark] = ColorMaskExtensions.CreateGreyMask(15);
            palette[(int)ColorIndex.FunctionalLight] = ColorMaskExtensions.CreateGreyMask(40);

            return palette;
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