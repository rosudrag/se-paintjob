using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;
using VRage.Utils;
using PaintJob.App.Skins;
using PaintJob.App.Skins.Painters;

namespace PaintJob.App.PaintAlgorithms
{
    /// <summary>
    /// Enhanced alien paint job with exotic, organic, and energy-based skins
    /// </summary>
    public class EnhancedAlienPaintJob : SkinAwarePaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "organic";
        private Random _random;

        public EnhancedAlienPaintJob() : base()
        {
            _colorResults = new Dictionary<Vector3I, int>();
            
            // Set alien theme for skins
            SkinTheme = "alien";
            EnableSkins = true;
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "organic";
            
            // Update skin theme based on variant
            switch (_variant)
            {
                case "crystalline":
                    SkinTheme = "crystal";
                    break;
                case "biomechanical":
                    SkinTheme = "biomech";
                    break;
                case "energy":
                    SkinTheme = "energy";
                    break;
                case "void":
                    SkinTheme = "void";
                    break;
                default:
                    SkinTheme = "alien";
                    break;
            }
        }

        protected override void InitializeSkinPainters()
        {
            var seed = _random?.Next() ?? 0;
            _skinPainters.Clear();
            _skinPainters.Add(new AlienSkinPainter(_skinProvider, seed));
            _skinPainters.Add(new PatternSkinPainter(seed));
        }

        public override void Clean()
        {
            base.Clean();
            _colorResults.Clear();
            _colorPalette = null;
        }

        protected override void Apply(MyCubeGrid grid)
        {
            try
            {
                var blocks = grid.GetBlocks();
                if (blocks == null || blocks.Count == 0)
                    return;
                    
                _random = new Random(unchecked((int)grid.EntityId));
                
                // Apply base alien coloration
                ApplyBaseAlienColors(blocks);
                
                // Apply organic patterns
                ApplyOrganicPatterns(blocks);
                
                // Apply bioluminescent effects
                ApplyBioluminescence(blocks);
                
                // Apply energy field effects
                ApplyEnergyFields(blocks);
                
                // Apply exotic material highlights
                ApplyExoticMaterials(blocks);
                
                // Apply both colors and skins
                ApplyColorsAndSkins(grid, _colorResults, _colorPalette);
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply alien paint job: {ex.Message}");
            }
        }

        protected override void GenerateColorPalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            var colors = new List<Vector3>();
            
            switch (_variant)
            {
                case "organic":
                    // Bio-organic colors
                    colors.Add(new Vector3(0.2f, 0.5f, 0.3f)); // Organic green
                    colors.Add(new Vector3(0.4f, 0.3f, 0.5f)); // Bio purple
                    colors.Add(new Vector3(0.1f, 0.3f, 0.2f)); // Deep bio green
                    colors.Add(new Vector3(0.6f, 0.7f, 0.4f)); // Acid yellow-green
                    colors.Add(new Vector3(0.3f, 0.1f, 0.2f)); // Dark flesh
                    break;
                    
                case "crystalline":
                    // Crystal/mineral colors
                    colors.Add(new Vector3(0.3f, 0.6f, 0.9f)); // Crystal blue
                    colors.Add(new Vector3(0.7f, 0.4f, 0.8f)); // Amethyst purple
                    colors.Add(new Vector3(0.4f, 0.9f, 0.7f)); // Aquamarine
                    colors.Add(new Vector3(0.9f, 0.3f, 0.6f)); // Rose quartz
                    colors.Add(new Vector3(0.2f, 0.2f, 0.3f)); // Dark crystal
                    break;
                    
                case "biomechanical":
                    // Giger-esque biomech
                    colors.Add(new Vector3(0.15f, 0.15f, 0.2f)); // Dark biometal
                    colors.Add(new Vector3(0.3f, 0.35f, 0.3f)); // Organic metal
                    colors.Add(new Vector3(0.05f, 0.05f, 0.05f)); // Near black
                    colors.Add(new Vector3(0.4f, 0.5f, 0.4f)); // Pale bio
                    colors.Add(new Vector3(0.2f, 0.25f, 0.15f)); // Dark green metal
                    break;
                    
                case "energy":
                    // Pure energy being colors
                    colors.Add(new Vector3(0.0f, 0.8f, 1.0f)); // Energy blue
                    colors.Add(new Vector3(1.0f, 0.4f, 0.0f)); // Plasma orange
                    colors.Add(new Vector3(0.8f, 0.0f, 1.0f)); // Energy purple
                    colors.Add(new Vector3(0.0f, 1.0f, 0.6f)); // Electric green
                    colors.Add(new Vector3(1.0f, 1.0f, 0.0f)); // Pure energy yellow
                    break;
                    
                case "void":
                    // Dark void alien
                    colors.Add(new Vector3(0.0f, 0.0f, 0.1f)); // Deep void blue
                    colors.Add(new Vector3(0.1f, 0.0f, 0.15f)); // Void purple
                    colors.Add(new Vector3(0.0f, 0.05f, 0.05f)); // Near black
                    colors.Add(new Vector3(0.2f, 0.0f, 0.3f)); // Dark matter purple
                    colors.Add(new Vector3(0.0f, 0.1f, 0.2f)); // Abyss blue
                    break;
                    
                default:
                    // Generic alien
                    colors.Add(new Vector3(0.3f, 0.5f, 0.6f)); // Alien teal
                    colors.Add(new Vector3(0.5f, 0.3f, 0.7f)); // Alien purple
                    colors.Add(new Vector3(0.2f, 0.6f, 0.3f)); // Alien green
                    colors.Add(new Vector3(0.7f, 0.4f, 0.2f)); // Alien orange
                    colors.Add(new Vector3(0.1f, 0.2f, 0.3f)); // Dark alien
                    break;
            }
            
            // Add bioluminescent accent colors
            colors.Add(new Vector3(0.0f, 1.0f, 0.8f)); // Cyan glow
            colors.Add(new Vector3(1.0f, 0.0f, 0.5f)); // Magenta glow
            colors.Add(new Vector3(0.5f, 1.0f, 0.0f)); // Lime glow
            
            _colorPalette = colors.ToArray();
        }

        protected override void ApplySkins(MyCubeGrid grid)
        {
            if (!EnableSkins)
                return;
            
            LogInfo("Applying alien skins...");
            
            // Apply exotic alien skins
            ApplyAlienThemedSkins(grid);
            
            // Apply standard skin patterns
            base.ApplySkins(grid);
        }

        private void ApplyAlienThemedSkins(MyCubeGrid grid)
        {
            var blocks = grid.GetBlocks();
            if (blocks == null || blocks.Count == 0)
                return;
                
            var seed = unchecked((int)grid.EntityId);
            
            foreach (var block in blocks)
            {
                var context = new SkinSelectionContext
                {
                    Block = block,
                    Position = block.Position,
                    BlockSubtype = block.BlockDefinition?.Id.SubtypeId.String ?? "",
                    ColorIndex = _colorResults.ContainsKey(block.Position) ? _colorResults[block.Position] : (int?)null,
                    PatternType = DetermineAlienPattern(block),
                    Zone = DetermineAlienZone(block),
                    Seed = seed
                };
                
                var selectedSkin = _skinProvider.SelectSkin(context);
                
                if (selectedSkin != MyStringHash.NullOrEmpty)
                {
                    _skinManager.SetBlockSkin(block.Position, selectedSkin);
                }
            }
        }

        private string DetermineAlienPattern(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (_variant == "crystalline")
                return "decorative"; // Crystalline structures
            else if (_variant == "biomechanical")
                return "tech"; // Biomechanical fusion
            else if (_variant == "energy")
                return "scifi"; // Pure energy
            else if (subtype.Contains("Armor"))
                return "alien"; // Organic carapace
            else
                return "scifi";
        }

        private string DetermineAlienZone(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (subtype.Contains("Reactor") || subtype.Contains("Battery"))
                return "core"; // Energy core
            else if (subtype.Contains("Sensor") || subtype.Contains("Antenna"))
                return "sensory"; // Sensory organs
            else if (subtype.Contains("Weapon"))
                return "offensive"; // Attack organs
            else if (subtype.Contains("Thrust"))
                return "locomotion"; // Movement organs
            else
                return "carapace"; // Outer shell
        }

        private void ApplyBaseAlienColors(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 0; // Primary alien color
            }
        }

        private void ApplyOrganicPatterns(HashSet<MySlimBlock> blocks)
        {
            if (_variant != "organic" && _variant != "biomechanical")
                return;
                
            // Create organic vein-like patterns
            foreach (var block in blocks)
            {
                var noise = GetOrganicNoise(block.Position);
                if (noise > 0.6f)
                {
                    _colorResults[block.Position] = 1; // Vein color
                }
                else if (noise > 0.4f)
                {
                    _colorResults[block.Position] = 2; // Transition color
                }
            }
        }

        private void ApplyBioluminescence(HashSet<MySlimBlock> blocks)
        {
            // Apply glowing effects to specific blocks
            var glowBlocks = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Light") == true ||
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Sensor") == true ||
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Antenna") == true
            ).ToList();
            
            foreach (var block in glowBlocks)
            {
                _colorResults[block.Position] = 5 + _random.Next(3); // Bioluminescent colors
            }
            
            // Random bioluminescent spots
            foreach (var block in blocks)
            {
                if (_random.NextDouble() < 0.05) // 5% chance
                {
                    _colorResults[block.Position] = 5 + _random.Next(3);
                }
            }
        }

        private void ApplyEnergyFields(HashSet<MySlimBlock> blocks)
        {
            if (_variant != "energy" && _variant != "crystalline")
                return;
                
            // Apply energy field effects around power systems
            var energyBlocks = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Reactor") == true ||
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Battery") == true ||
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Solar") == true
            ).ToList();
            
            foreach (var energyBlock in energyBlocks)
            {
                _colorResults[energyBlock.Position] = 3; // Energy core color
                
                // Apply energy field to nearby blocks
                foreach (var block in blocks)
                {
                    var distance = Vector3I.DistanceManhattan(block.Position, energyBlock.Position);
                    if (distance <= 2 && distance > 0)
                    {
                        _colorResults[block.Position] = 4; // Energy field color
                    }
                }
            }
        }

        private void ApplyExoticMaterials(HashSet<MySlimBlock> blocks)
        {
            // Apply exotic materials to special blocks
            var exoticBlocks = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Heavy") == true ||
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Blast") == true
            ).ToList();
            
            foreach (var block in exoticBlocks)
            {
                if (_variant == "crystalline")
                    _colorResults[block.Position] = 2; // Crystal material
                else if (_variant == "void")
                    _colorResults[block.Position] = 1; // Void material
            }
        }

        private float GetOrganicNoise(Vector3I position)
        {
            // Simple noise function for organic patterns
            float x = position.X * 0.3f;
            float y = position.Y * 0.3f;
            float z = position.Z * 0.3f;
            
            return (float)(Math.Sin(x) * Math.Cos(y) * Math.Sin(z) + 1.0f) * 0.5f;
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedAlienPaint", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedAlienPaint", $"ERROR: {message}");
        }
    }
}