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
    /// Enhanced racing paint job with carbon fiber and neon accent skins
    /// </summary>
    public class EnhancedRacingPaintJob : SkinAwarePaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "speedster";
        private Random _random;

        public EnhancedRacingPaintJob() : base()
        {
            _colorResults = new Dictionary<Vector3I, int>();
            
            // Set racing theme for skins
            SkinTheme = "racing";
            EnableSkins = true;
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "speedster";
            
            // Update skin theme based on variant
            switch (_variant)
            {
                case "formula":
                    SkinTheme = "formula1";
                    break;
                case "street":
                    SkinTheme = "streetracing";
                    break;
                case "drift":
                    SkinTheme = "drift";
                    break;
                case "rally":
                    SkinTheme = "rally";
                    break;
                default:
                    SkinTheme = "racing";
                    break;
            }
        }

        protected override void InitializeSkinPainters()
        {
            var seed = _random?.Next() ?? 0;
            _skinPainters.Clear();
            _skinPainters.Add(new RacingSkinPainter(_skinProvider, seed));
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
                
                // Apply base racing colors
                ApplyBaseRacingColors(blocks);
                
                // Apply racing stripes
                ApplyRacingStripes(blocks);
                
                // Apply sponsor decals (simulated with color blocks)
                ApplySponsorColors(blocks);
                
                // Apply aerodynamic highlights
                ApplyAeroHighlights(blocks);
                
                // Apply speed blur effects on thrusters
                ApplySpeedEffects(blocks);
                
                // Apply both colors and skins
                ApplyColorsAndSkins(grid, _colorResults, _colorPalette);
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply racing paint job: {ex.Message}");
            }
        }

        protected override void GenerateColorPalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            var colors = new List<Vector3>();
            
            switch (_variant)
            {
                case "speedster":
                    // Classic racing red with white stripes
                    colors.Add(new Vector3(0.9f, 0.1f, 0.1f)); // Racing red
                    colors.Add(new Vector3(1.0f, 1.0f, 1.0f)); // Pure white
                    colors.Add(new Vector3(0.1f, 0.1f, 0.1f)); // Carbon black
                    colors.Add(new Vector3(0.8f, 0.8f, 0.8f)); // Silver
                    break;
                    
                case "formula":
                    // F1 inspired - team colors
                    colors.Add(new Vector3(0.0f, 0.3f, 0.7f)); // Team blue
                    colors.Add(new Vector3(1.0f, 0.5f, 0.0f)); // Team orange
                    colors.Add(new Vector3(0.2f, 0.2f, 0.2f)); // Dark gray
                    colors.Add(new Vector3(0.9f, 0.9f, 0.9f)); // Light gray
                    break;
                    
                case "street":
                    // Street racing neon
                    colors.Add(new Vector3(0.0f, 1.0f, 0.5f)); // Neon green
                    colors.Add(new Vector3(1.0f, 0.0f, 1.0f)); // Neon purple
                    colors.Add(new Vector3(0.05f, 0.05f, 0.05f)); // Matte black
                    colors.Add(new Vector3(0.0f, 0.5f, 1.0f)); // Electric blue
                    break;
                    
                case "drift":
                    // Japanese drift style
                    colors.Add(new Vector3(1.0f, 0.0f, 0.3f)); // Hot pink
                    colors.Add(new Vector3(0.1f, 0.1f, 0.1f)); // Black
                    colors.Add(new Vector3(1.0f, 1.0f, 0.0f)); // Yellow
                    colors.Add(new Vector3(0.5f, 0.0f, 0.8f)); // Purple
                    break;
                    
                case "rally":
                    // Rally racing colors
                    colors.Add(new Vector3(0.0f, 0.4f, 0.2f)); // Rally green
                    colors.Add(new Vector3(0.9f, 0.6f, 0.1f)); // Rally orange
                    colors.Add(new Vector3(0.8f, 0.8f, 0.8f)); // Dust gray
                    colors.Add(new Vector3(0.3f, 0.2f, 0.1f)); // Mud brown
                    break;
                    
                default:
                    // Generic racing
                    colors.Add(new Vector3(0.8f, 0.0f, 0.0f)); // Red
                    colors.Add(new Vector3(0.0f, 0.0f, 0.0f)); // Black
                    colors.Add(new Vector3(1.0f, 1.0f, 1.0f)); // White
                    colors.Add(new Vector3(0.5f, 0.5f, 0.5f)); // Gray
                    break;
            }
            
            // Add metallic accent colors
            colors.Add(new Vector3(0.7f, 0.7f, 0.8f)); // Chrome
            colors.Add(new Vector3(0.9f, 0.7f, 0.3f)); // Gold accent
            
            _colorPalette = colors.ToArray();
        }

        protected override void ApplySkins(MyCubeGrid grid)
        {
            if (!EnableSkins)
                return;
            
            LogInfo("Applying racing skins...");
            
            // Apply performance-oriented skins
            ApplyRacingThemedSkins(grid);
            
            // Apply standard skin patterns
            base.ApplySkins(grid);
        }

        private void ApplyRacingThemedSkins(MyCubeGrid grid)
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
                    PatternType = DetermineRacingPattern(block),
                    Zone = DetermineRacingZone(block),
                    Seed = seed
                };
                
                var selectedSkin = _skinProvider.SelectSkin(context);
                
                if (selectedSkin != MyStringHash.NullOrEmpty)
                {
                    _skinManager.SetBlockSkin(block.Position, selectedSkin);
                }
            }
        }

        private string DetermineRacingPattern(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (subtype.Contains("Armor") && subtype.Contains("Slope"))
                return "tech"; // Aerodynamic surfaces
            else if (subtype.Contains("Armor"))
                return "tech"; // Carbon fiber body
            else if (subtype.Contains("Thrust"))
                return "decorative"; // Performance parts
            else
                return "tech";
        }

        private string DetermineRacingZone(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (subtype.Contains("Cockpit") || subtype.Contains("Control"))
                return "cockpit";
            else if (subtype.Contains("Thrust"))
                return "engine";
            else if (subtype.Contains("Wheel") || subtype.Contains("Suspension"))
                return "wheels";
            else
                return "body";
        }

        private void ApplyBaseRacingColors(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 0; // Primary team color
            }
        }

        private void ApplyRacingStripes(HashSet<MySlimBlock> blocks)
        {
            // Apply racing stripes along the main axis
            var minZ = blocks.Min(b => b.Position.Z);
            var maxZ = blocks.Max(b => b.Position.Z);
            var centerX = (blocks.Min(b => b.Position.X) + blocks.Max(b => b.Position.X)) / 2;
            
            foreach (var block in blocks)
            {
                // Center stripes
                if (Math.Abs(block.Position.X - centerX) <= 1)
                {
                    _colorResults[block.Position] = 1; // Stripe color
                }
                // Side stripes
                else if (Math.Abs(block.Position.X - centerX) == 3)
                {
                    _colorResults[block.Position] = 2; // Accent stripe
                }
            }
        }

        private void ApplySponsorColors(HashSet<MySlimBlock> blocks)
        {
            // Simulate sponsor decals on large flat surfaces
            var largeArmor = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Large") == true &&
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Armor") == true
            ).ToList();
            
            foreach (var block in largeArmor)
            {
                if (_random.NextDouble() < 0.1) // 10% chance for sponsor patch
                {
                    _colorResults[block.Position] = 3 + _random.Next(2); // Sponsor colors
                }
            }
        }

        private void ApplyAeroHighlights(HashSet<MySlimBlock> blocks)
        {
            // Highlight aerodynamic elements
            var aeroBlocks = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Slope") == true ||
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Corner") == true
            ).ToList();
            
            foreach (var block in aeroBlocks)
            {
                if (_random.NextDouble() < 0.3)
                {
                    _colorResults[block.Position] = 5; // Chrome/metallic
                }
            }
        }

        private void ApplySpeedEffects(HashSet<MySlimBlock> blocks)
        {
            // Apply special colors to thrusters for speed effect
            var thrusters = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Thrust") == true
            ).ToList();
            
            foreach (var thruster in thrusters)
            {
                _colorResults[thruster.Position] = _variant == "street" ? 3 : 2; // Neon or accent
            }
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedRacingPaint", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedRacingPaint", $"ERROR: {message}");
        }
    }
}