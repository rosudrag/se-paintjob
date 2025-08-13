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

        private readonly Random _random;
        
        public ColorSchemeGenerator(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

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
        
        /// <summary>
        /// Generates a complete military color palette with 12 colors.
        /// </summary>
        public Vector3[] GenerateMilitaryPalette(string variant = "standard")
        {
            var colors = new Vector3[16]; // Increased to include navigation tints and letter colors
            
            // Military variant-specific base colors
            float baseHue, baseSat, baseVal;
            float accentHue;
            
            switch (variant.ToLower())
            {
                case "stealth":
                    baseHue = 0f; // Pure black/grey
                    baseSat = 0.05f;
                    baseVal = 0.15f;
                    accentHue = 0.583f; // Dark blue accent
                    break;
                    
                case "asteroid":
                    baseHue = 0.083f; // Orange-brown like asteroid rock
                    baseSat = 0.25f;
                    baseVal = 0.35f;
                    accentHue = 0.056f; // Reddish brown
                    break;
                    
                case "industrial":
                    baseHue = 0.167f; // Yellow-grey (industrial warning colors)
                    baseSat = 0.15f;
                    baseVal = 0.45f;
                    accentHue = 0.092f; // Orange accent
                    break;
                    
                case "deep_space":
                    baseHue = 0.667f; // Deep blue
                    baseSat = 0.3f;
                    baseVal = 0.25f;
                    accentHue = 0.75f; // Purple accent
                    break;
                    
                case "standard":
                default:
                    baseHue = 0.222f; // Military green-grey
                    baseSat = 0.35f;
                    baseVal = 0.4f;
                    accentHue = 0.167f; // Yellow-green accent
                    break;
            }
            
            // Generate base camouflage colors with variation
            var hueVariation = 0.04f + (float)_random.NextDouble() * 0.02f;
            var satVariation = 0.1f + (float)_random.NextDouble() * 0.05f;
            var valVariation = 0.08f + (float)_random.NextDouble() * 0.04f;
            
            colors[0] = new Vector3(baseHue, baseSat, baseVal).HSVToColorMask(); // Primary Hull
            colors[1] = new Vector3(baseHue + hueVariation, baseSat - satVariation, baseVal - valVariation).HSVToColorMask(); // Secondary Hull
            colors[2] = new Vector3(accentHue, baseSat + satVariation, baseVal * 0.7f).HSVToColorMask(); // Camo Accent
            colors[3] = new Vector3(baseHue - hueVariation, baseSat * 0.8f, baseVal + valVariation).HSVToColorMask(); // Variant
            
            // Weapon systems - metallic greys with slight environment tint
            colors[4] = new Vector3(baseHue, 0.1f, 0.3f).HSVToColorMask(); // Weapon Systems
            
            // Technical and functional areas - very dark with environment tint
            colors[5] = new Vector3(baseHue, 0.15f, 0.12f).HSVToColorMask(); // Technical Areas
            
            // Warning colors - consistent across environments
            colors[6] = new Vector3(0.092f, 0.95f, 0.85f).HSVToColorMask(); // Hazard Warning (orange)
            
            // Navigation lights - subdued military versions
            colors[7] = new Vector3(0f, 0.6f, 0.35f).HSVToColorMask(); // Port (dark red)
            colors[8] = new Vector3(0.333f, 0.5f, 0.3f).HSVToColorMask(); // Starboard (dark green)
            
            // Interior and functional - neutral with slight environment tint
            colors[9] = new Vector3(baseHue, 0.08f, 0.45f).HSVToColorMask(); // Interior Spaces
            colors[10] = new Vector3(baseHue, 0.12f, 0.18f).HSVToColorMask(); // Functional Dark
            colors[11] = new Vector3(baseHue, 0.1f, 0.38f).HSVToColorMask(); // Functional Light
            
            // Navigation light tints - blend with primary hull color
            var primaryHullHsv = new Vector3(baseHue, baseSat, baseVal);
            var portHsv = new Vector3(0f, 0.6f, 0.35f);
            var starboardHsv = new Vector3(0.333f, 0.5f, 0.3f);
            
            // Create subtle tints
            colors[12] = new Vector3(
                portHsv.X,
                primaryHullHsv.Y * 0.8f + portHsv.Y * 0.2f,
                primaryHullHsv.Z * 0.85f + portHsv.Z * 0.15f
            ).HSVToColorMask(); // Port tint
            
            colors[13] = new Vector3(
                starboardHsv.X,
                primaryHullHsv.Y * 0.8f + starboardHsv.Y * 0.2f,
                primaryHullHsv.Z * 0.85f + starboardHsv.Z * 0.15f
            ).HSVToColorMask(); // Starboard tint
            
            // Letter block colors - high contrast with hull
            if (primaryHullHsv.Z < 0.5f)
            {
                // Dark hull - light letters
                colors[14] = new Vector3(primaryHullHsv.X, primaryHullHsv.Y * 0.3f, 0.85f).HSVToColorMask();
            }
            else
            {
                // Light hull - dark letters
                colors[14] = new Vector3(primaryHullHsv.X, primaryHullHsv.Y * 0.5f, 0.15f).HSVToColorMask();
            }
            
            // Letter background - slightly different from hull
            colors[15] = new Vector3(
                primaryHullHsv.X,
                primaryHullHsv.Y * 0.7f,
                primaryHullHsv.Z + (primaryHullHsv.Z < 0.5f ? 0.1f : -0.1f)
            ).HSVToColorMask();
            
            return colors;
        }

        public Vector3[] GenerateRacingPalette(string variant = "formula1")
        {
            var colors = new Vector3[16];
            
            float primaryHue, primarySat, primaryVal;
            float accentHue1, accentHue2;
            
            switch (variant.ToLower())
            {
                case "formula1":
                    primaryHue = 0f; // Red
                    primarySat = 0.9f;
                    primaryVal = 0.8f;
                    accentHue1 = 0f; // White
                    accentHue2 = 0f; // Black
                    break;
                    
                case "rally":
                    primaryHue = 0.583f; // Blue
                    primarySat = 0.85f;
                    primaryVal = 0.7f;
                    accentHue1 = 0.167f; // Yellow
                    accentHue2 = 0f; // White
                    break;
                    
                case "street":
                    primaryHue = 0.083f; // Orange
                    primarySat = 0.95f;
                    primaryVal = 0.85f;
                    accentHue1 = 0.75f; // Purple
                    accentHue2 = 0.333f; // Green
                    break;
                    
                case "drag":
                    primaryHue = 0.333f; // Green
                    primarySat = 1f;
                    primaryVal = 0.9f;
                    accentHue1 = 0.917f; // Pink
                    accentHue2 = 0.167f; // Yellow
                    break;
                    
                case "endurance":
                default:
                    primaryHue = 0.222f; // Teal
                    primarySat = 0.7f;
                    primaryVal = 0.6f;
                    accentHue1 = 0.083f; // Orange
                    accentHue2 = 0f; // Grey
                    break;
            }
            
            // Base colors
            colors[0] = new Vector3(primaryHue, primarySat, primaryVal).HSVToColorMask();
            colors[1] = new Vector3(primaryHue, primarySat * 0.8f, primaryVal * 1.2f).HSVToColorMask();
            
            // Racing stripes
            colors[2] = new Vector3(accentHue1, 0.9f, 0.9f).HSVToColorMask();
            colors[3] = new Vector3(accentHue2, variant == "formula1" ? 0f : 0.8f, variant == "formula1" ? 0.1f : 0.8f).HSVToColorMask();
            
            // Metallic/hot colors for functional parts
            colors[4] = new Vector3(0.083f, 0.7f, 1f).HSVToColorMask(); // Hot exhaust
            colors[5] = new Vector3(0f, 0.9f, 0.5f).HSVToColorMask(); // Weapon red
            colors[6] = new Vector3(0.583f, 0.3f, 0.7f).HSVToColorMask(); // Canopy tint
            colors[7] = new Vector3(0f, 0f, 1f).HSVToColorMask(); // Number white
            
            // Additional accent colors
            for (int i = 8; i < 16; i++)
            {
                var variation = (float)_random.NextDouble() * 0.1f - 0.05f;
                colors[i] = new Vector3(
                    primaryHue + variation,
                    primarySat * (0.7f + (float)_random.NextDouble() * 0.3f),
                    primaryVal * (0.8f + (float)_random.NextDouble() * 0.4f)
                ).HSVToColorMask();
            }
            
            return colors;
        }

        public Vector3[] GeneratePiratePalette(string variant = "skull")
        {
            var colors = new Vector3[16];
            
            // Pirate colors: dark, weathered, with red and bone accents
            float baseHue = 0f; // Red-brown spectrum
            float baseSat = 0.3f;
            float baseVal = 0.2f;
            
            switch (variant.ToLower())
            {
                case "kraken":
                    baseHue = 0.583f; // Dark blue-green
                    baseSat = 0.4f;
                    break;
                case "blackbeard":
                    baseHue = 0f; // Pure black/grey
                    baseSat = 0.05f;
                    break;
                case "corsair":
                    baseHue = 0.917f; // Deep purple
                    baseSat = 0.35f;
                    break;
                case "marauder":
                    baseHue = 0.083f; // Orange-brown
                    baseSat = 0.45f;
                    break;
            }
            
            // Weathered base colors
            colors[0] = new Vector3(baseHue, baseSat, baseVal).HSVToColorMask(); // Dark base
            colors[1] = new Vector3(baseHue, baseSat * 0.5f, baseVal * 1.5f).HSVToColorMask(); // Weathered metal
            colors[2] = new Vector3(0.05f, 0.6f, 0.4f).HSVToColorMask(); // Rust
            colors[3] = new Vector3(0f, 0f, 0.15f).HSVToColorMask(); // Scorch marks
            
            // Pirate emblems
            colors[4] = new Vector3(0.167f, 0.1f, 0.85f).HSVToColorMask(); // Bone white
            colors[5] = new Vector3(0f, 0f, 0.05f).HSVToColorMask(); // Deep black
            colors[6] = new Vector3(0.05f, 0.7f, 0.5f).HSVToColorMask(); // Rust edges
            
            // Weapon colors
            colors[7] = new Vector3(0f, 0.8f, 0.3f).HSVToColorMask(); // Blood red
            colors[8] = new Vector3(0f, 0f, 0.25f).HSVToColorMask(); // Gun metal
            
            // Additional weathering variations
            for (int i = 9; i < 16; i++)
            {
                colors[i] = new Vector3(
                    baseHue + (float)_random.NextDouble() * 0.1f,
                    baseSat * (0.3f + (float)_random.NextDouble() * 0.4f),
                    baseVal * (0.8f + (float)_random.NextDouble() * 0.4f)
                ).HSVToColorMask();
            }
            
            return colors;
        }

        public Vector3[] GenerateCorporatePalette(string variant = "mining_corp")
        {
            var colors = new Vector3[16];
            
            float primaryHue, primarySat, primaryVal;
            float departmentHue1, departmentHue2, departmentHue3;
            
            switch (variant.ToLower())
            {
                case "mining_corp":
                    primaryHue = 0.167f; // Yellow
                    departmentHue1 = 0.083f; // Orange (engineering)
                    departmentHue2 = 0f; // Grey (logistics)
                    departmentHue3 = 0.333f; // Green (operations)
                    break;
                    
                case "transport_co":
                    primaryHue = 0.583f; // Blue
                    departmentHue1 = 0.667f; // Navy (engineering)
                    departmentHue2 = 0.5f; // Cyan (logistics)
                    departmentHue3 = 0f; // White (operations)
                    break;
                    
                case "security_firm":
                    primaryHue = 0f; // Red
                    departmentHue1 = 0f; // Black (engineering)
                    departmentHue2 = 0.083f; // Orange (logistics)
                    departmentHue3 = 0f; // Grey (operations)
                    break;
                    
                case "research_lab":
                    primaryHue = 0.75f; // Purple
                    departmentHue1 = 0.583f; // Blue (engineering)
                    departmentHue2 = 0.333f; // Green (logistics)
                    departmentHue3 = 0.167f; // Yellow (operations)
                    break;
                    
                case "construction":
                default:
                    primaryHue = 0.083f; // Orange
                    departmentHue1 = 0.167f; // Yellow (engineering)
                    departmentHue2 = 0f; // Grey (logistics)
                    departmentHue3 = 0.583f; // Blue (operations)
                    break;
            }
            
            primarySat = 0.7f;
            primaryVal = 0.8f;
            
            // Corporate base colors
            colors[0] = new Vector3(primaryHue, primarySat, primaryVal).HSVToColorMask();
            
            // Department colors
            colors[1] = new Vector3(departmentHue1, 0.6f, 0.7f).HSVToColorMask(); // Engineering
            colors[2] = new Vector3(departmentHue2, departmentHue2 == 0f ? 0f : 0.5f, 0.75f).HSVToColorMask(); // Logistics
            colors[3] = new Vector3(departmentHue3, 0.55f, 0.7f).HSVToColorMask(); // Operations
            colors[4] = new Vector3(0f, 0f, 0.9f).HSVToColorMask(); // Command (white/silver)
            colors[5] = new Vector3(0f, 0.8f, 0.6f).HSVToColorMask(); // Security (red)
            colors[6] = new Vector3(0.333f, 0.3f, 0.85f).HSVToColorMask(); // Medical (white-green)
            
            // Corporate branding
            colors[7] = new Vector3(primaryHue, primarySat * 1.2f, primaryVal * 0.9f).HSVToColorMask(); // Logo
            colors[8] = new Vector3(primaryHue, primarySat * 0.8f, primaryVal * 1.1f).HSVToColorMask(); // Stripe
            
            // Safety markings
            colors[9] = new Vector3(0.167f, 1f, 0.9f).HSVToColorMask(); // Hazard yellow
            colors[10] = new Vector3(0f, 0.95f, 0.8f).HSVToColorMask(); // Emergency red
            colors[11] = new Vector3(0f, 0.6f, 0.4f).HSVToColorMask(); // Port red
            colors[12] = new Vector3(0.333f, 0.6f, 0.4f).HSVToColorMask(); // Starboard green
            
            // Additional corporate variations
            for (int i = 13; i < 16; i++)
            {
                colors[i] = new Vector3(primaryHue, primarySat * 0.5f, primaryVal * (0.9f + i * 0.02f)).HSVToColorMask();
            }
            
            return colors;
        }

        public Vector3[] GenerateAlienPalette(string variant = "organic")
        {
            var colors = new Vector3[16];
            
            float baseHue, baseSat, baseVal;
            float glowHue1, glowHue2;
            
            switch (variant.ToLower())
            {
                case "organic":
                    baseHue = 0.333f; // Green-organic
                    glowHue1 = 0.75f; // Purple glow
                    glowHue2 = 0.5f; // Cyan glow
                    break;
                    
                case "crystalline":
                    baseHue = 0.75f; // Purple crystal
                    glowHue1 = 0.917f; // Pink glow
                    glowHue2 = 0.583f; // Blue glow
                    break;
                    
                case "techno_organic":
                    baseHue = 0.5f; // Cyan tech
                    glowHue1 = 0.333f; // Green organic
                    glowHue2 = 0.083f; // Orange energy
                    break;
                    
                case "hive":
                    baseHue = 0.167f; // Yellow-brown hive
                    glowHue1 = 0.083f; // Orange glow
                    glowHue2 = 0f; // Red glow
                    break;
                    
                case "ancient":
                default:
                    baseHue = 0.222f; // Teal ancient
                    glowHue1 = 0.167f; // Gold glow
                    glowHue2 = 0.75f; // Purple energy
                    break;
            }
            
            baseSat = 0.4f;
            baseVal = 0.3f;
            
            // Organic base textures
            colors[0] = new Vector3(baseHue, baseSat, baseVal).HSVToColorMask(); // Deep organic
            colors[1] = new Vector3(baseHue, baseSat * 0.7f, baseVal * 1.3f).HSVToColorMask(); // Mid organic
            colors[2] = new Vector3(baseHue, baseSat * 0.5f, baseVal * 1.6f).HSVToColorMask(); // Light tissue
            
            // Bio-luminescent veins
            colors[3] = new Vector3(glowHue1, 0.8f, 0.9f).HSVToColorMask(); // Primary glow
            colors[4] = new Vector3(glowHue1, 0.6f, 0.7f).HSVToColorMask(); // Secondary glow
            colors[5] = new Vector3(glowHue2, 0.7f, 0.8f).HSVToColorMask(); // Tertiary glow
            
            // Energy nodes
            colors[6] = new Vector3(glowHue1, 1f, 1f).HSVToColorMask(); // Bright energy
            colors[7] = new Vector3(glowHue2, 0.9f, 0.95f).HSVToColorMask(); // Alt energy
            colors[8] = new Vector3((glowHue1 + glowHue2) / 2f, 0.85f, 0.85f).HSVToColorMask(); // Mixed energy
            
            // Membrane effects
            colors[9] = new Vector3(baseHue, 0.2f, 0.6f).HSVToColorMask(); // Translucent membrane
            colors[10] = new Vector3(baseHue, 0.3f, 0.5f).HSVToColorMask(); // Membrane shadow
            
            // Exotic functional colors
            colors[11] = new Vector3(0.833f, 0.9f, 0.95f).HSVToColorMask(); // Plasma exhaust
            colors[12] = new Vector3(0.417f, 0.8f, 0.9f).HSVToColorMask(); // Energy weapon
            colors[13] = new Vector3(0.75f, 0.6f, 1f).HSVToColorMask(); // Crystal energy
            colors[14] = new Vector3(0.75f, 0.7f, 0.8f).HSVToColorMask(); // Psychic purple
            
            // Additional organic variation
            colors[15] = new Vector3(
                baseHue + (float)_random.NextDouble() * 0.2f - 0.1f,
                baseSat * (0.6f + (float)_random.NextDouble() * 0.4f),
                baseVal * (0.8f + (float)_random.NextDouble() * 0.4f)
            ).HSVToColorMask();
            
            return colors;
        }

        public Vector3[] GenerateRetroPalette(string variant = "80s_neon")
        {
            var colors = new Vector3[16];
            
            switch (variant.ToLower())
            {
                case "80s_neon":
                    // Dark base with bright neon accents
                    colors[0] = new Vector3(0.75f, 0.8f, 0.15f).HSVToColorMask(); // Dark purple base
                    colors[1] = new Vector3(0.917f, 1f, 1f).HSVToColorMask(); // Hot pink neon
                    colors[2] = new Vector3(0.5f, 1f, 1f).HSVToColorMask(); // Cyan neon
                    colors[3] = new Vector3(0.333f, 1f, 1f).HSVToColorMask(); // Green neon
                    colors[4] = new Vector3(0.917f, 0.9f, 0.95f).HSVToColorMask(); // Pink accent
                    colors[5] = new Vector3(0.5f, 0.9f, 0.95f).HSVToColorMask(); // Cyan accent
                    colors[6] = new Vector3(0.75f, 0.7f, 0.8f).HSVToColorMask(); // Purple trim
                    break;
                    
                case "50s_chrome":
                    // Chrome, pastels, and two-tone
                    colors[0] = new Vector3(0f, 0f, 0.85f).HSVToColorMask(); // Chrome silver
                    colors[1] = new Vector3(0.5f, 0.2f, 0.9f).HSVToColorMask(); // Pastel mint
                    colors[2] = new Vector3(0f, 0f, 0.95f).HSVToColorMask(); // Bright chrome
                    colors[3] = new Vector3(0f, 0.8f, 0.7f).HSVToColorMask(); // Red accent
                    colors[4] = new Vector3(0f, 0f, 1f).HSVToColorMask(); // White
                    colors[5] = new Vector3(0.917f, 0.3f, 0.85f).HSVToColorMask(); // Pastel pink
                    break;
                    
                case "70s_disco":
                    // Earth tones and psychedelic
                    colors[0] = new Vector3(0.083f, 0.6f, 0.5f).HSVToColorMask(); // Brown-orange
                    colors[1] = new Vector3(0.222f, 0.5f, 0.45f).HSVToColorMask(); // Avocado green
                    colors[2] = new Vector3(0.167f, 0.7f, 0.6f).HSVToColorMask(); // Mustard yellow
                    colors[3] = new Vector3(0.05f, 0.8f, 0.55f).HSVToColorMask(); // Burnt orange
                    colors[4] = new Vector3(0f, 0f, 0.8f).HSVToColorMask(); // Mirror silver
                    colors[5] = new Vector3(0.083f, 0.7f, 0.35f).HSVToColorMask(); // Shag brown
                    break;
                    
                case "90s_cyber":
                    // Matrix green and black
                    colors[0] = new Vector3(0f, 0f, 0.05f).HSVToColorMask(); // Black
                    colors[1] = new Vector3(0.333f, 1f, 0.8f).HSVToColorMask(); // Bright matrix green
                    colors[2] = new Vector3(0.333f, 0.8f, 0.5f).HSVToColorMask(); // Dark matrix green
                    colors[3] = new Vector3(0.333f, 0.9f, 0.9f).HSVToColorMask(); // Terminal green
                    colors[4] = new Vector3(0.583f, 0.7f, 0.6f).HSVToColorMask(); // Screen blue
                    colors[5] = new Vector3(0f, 0f, 0.2f).HSVToColorMask(); // Dark grey
                    break;
                    
                case "art_deco":
                default:
                    // Gold, black, and geometric
                    colors[0] = new Vector3(0f, 0f, 0.1f).HSVToColorMask(); // Black
                    colors[1] = new Vector3(0.167f, 0.6f, 0.7f).HSVToColorMask(); // Gold
                    colors[2] = new Vector3(0.083f, 0.4f, 0.6f).HSVToColorMask(); // Brass
                    colors[3] = new Vector3(0f, 0f, 0.75f).HSVToColorMask(); // Silver trim
                    colors[4] = new Vector3(0.167f, 0.3f, 0.85f).HSVToColorMask(); // Champagne
                    colors[5] = new Vector3(0f, 0f, 0.3f).HSVToColorMask(); // Dark grey
                    break;
            }
            
            // Fill remaining colors with variations
            for (int i = 6; i < 16; i++)
            {
                if (colors[i] == Vector3.Zero)
                {
                    // Generate variation based on primary colors
                    var baseColor = colors[i % 6];
                    colors[i] = new Vector3(
                        baseColor.X,
                        baseColor.Y * (0.7f + (float)_random.NextDouble() * 0.3f),
                        baseColor.Z * (0.8f + (float)_random.NextDouble() * 0.4f)
                    );
                }
            }
            
            return colors;
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