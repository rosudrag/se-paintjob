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
    /// Enhanced pirate paint job with weathered skins and battle damage
    /// </summary>
    public class EnhancedPiratePaintJob : SkinAwarePaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "skull";
        private Random _random;

        public EnhancedPiratePaintJob() : base()
        {
            _colorResults = new Dictionary<Vector3I, int>();
            
            // Set pirate theme for skins
            SkinTheme = "pirate";
            EnableSkins = true;
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "skull";
            
            // Update skin theme based on variant
            switch (_variant)
            {
                case "ghost":
                    SkinTheme = "ghostship";
                    break;
                case "blood":
                    SkinTheme = "bloodied";
                    break;
                case "treasure":
                    SkinTheme = "golden";
                    break;
                default:
                    SkinTheme = "pirate";
                    break;
            }
        }

        protected override void InitializeSkinPainters()
        {
            // Add pirate-specific skin painters
            var seed = _random?.Next() ?? 0;
            _skinPainters.Clear();
            _skinPainters.Add(new PirateSkinPainter(_skinProvider, seed));
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
                
                // Apply base weathered appearance
                ApplyWeatheredBase(blocks);
                
                // Apply battle damage simulation
                ApplyBattleDamage(blocks);
                
                // Apply pirate markings
                ApplyPirateMarkings(blocks);
                
                // Apply rust and wear to edges
                ApplyRustAndWear(blocks);
                
                // Apply treasure/loot colors to cargo areas
                ApplyTreasureAccents(blocks);
                
                // Apply both colors and skins
                ApplyColorsAndSkins(grid, _colorResults, _colorPalette);
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply pirate paint job: {ex.Message}");
            }
        }

        protected override void GenerateColorPalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            
            // Generate pirate-themed color palette
            var colors = new List<Vector3>();
            
            switch (_variant)
            {
                case "skull":
                    // Black, bone white, blood red
                    colors.Add(new Vector3(0.1f, 0.1f, 0.1f)); // Black
                    colors.Add(new Vector3(0.9f, 0.85f, 0.75f)); // Bone
                    colors.Add(new Vector3(0.4f, 0.05f, 0.05f)); // Dark red
                    colors.Add(new Vector3(0.3f, 0.25f, 0.2f)); // Weathered wood
                    break;
                    
                case "ghost":
                    // Pale blues, grays, translucent whites
                    colors.Add(new Vector3(0.7f, 0.8f, 0.9f)); // Pale blue
                    colors.Add(new Vector3(0.5f, 0.5f, 0.55f)); // Ghost gray
                    colors.Add(new Vector3(0.9f, 0.95f, 1.0f)); // Ethereal white
                    colors.Add(new Vector3(0.3f, 0.4f, 0.5f)); // Deep ocean
                    break;
                    
                case "blood":
                    // Deep reds, rusty browns, black
                    colors.Add(new Vector3(0.5f, 0.0f, 0.0f)); // Blood red
                    colors.Add(new Vector3(0.3f, 0.1f, 0.05f)); // Dried blood
                    colors.Add(new Vector3(0.15f, 0.08f, 0.05f)); // Dark brown
                    colors.Add(new Vector3(0.05f, 0.05f, 0.05f)); // Near black
                    break;
                    
                case "treasure":
                    // Gold, copper, bronze, jewel tones
                    colors.Add(new Vector3(0.9f, 0.7f, 0.2f)); // Gold
                    colors.Add(new Vector3(0.7f, 0.4f, 0.2f)); // Copper
                    colors.Add(new Vector3(0.6f, 0.5f, 0.3f)); // Bronze
                    colors.Add(new Vector3(0.2f, 0.5f, 0.3f)); // Emerald
                    break;
                    
                default:
                    // Standard pirate colors
                    colors.Add(new Vector3(0.15f, 0.12f, 0.1f)); // Dark wood
                    colors.Add(new Vector3(0.6f, 0.1f, 0.1f)); // Pirate red
                    colors.Add(new Vector3(0.8f, 0.75f, 0.6f)); // Canvas
                    colors.Add(new Vector3(0.3f, 0.3f, 0.25f)); // Weathered metal
                    break;
            }
            
            // Add some randomized weathering colors
            var random = new Random(seed);
            for (int i = 0; i < 3; i++)
            {
                var rustColor = new Vector3(
                    0.3f + (float)random.NextDouble() * 0.2f,
                    0.15f + (float)random.NextDouble() * 0.1f,
                    0.05f + (float)random.NextDouble() * 0.05f
                );
                colors.Add(rustColor);
            }
            
            _colorPalette = colors.ToArray();
        }

        protected override void ApplySkins(MyCubeGrid grid)
        {
            if (!EnableSkins)
                return;
            
            LogInfo("Applying pirate skins...");
            
            // Apply weathered and battle-damaged skins
            ApplyPirateThemedSkins(grid);
            
            // Apply standard skin patterns
            base.ApplySkins(grid);
        }

        private void ApplyPirateThemedSkins(MyCubeGrid grid)
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
                    PatternType = DeterminePiratePattern(block),
                    Zone = DeterminePirateZone(block),
                    Seed = seed
                };
                
                var selectedSkin = _skinProvider.SelectSkin(context);
                
                if (selectedSkin != MyStringHash.NullOrEmpty)
                {
                    _skinManager.SetBlockSkin(block.Position, selectedSkin);
                }
            }
        }

        private string DeterminePiratePattern(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            // Heavily weathered exterior
            if (subtype.Contains("Armor"))
                return "rusty";
            // Battle scarred weapons
            else if (subtype.Contains("Weapon") || subtype.Contains("Turret"))
                return "worn";
            // Makeshift repairs
            else if (subtype.Contains("Interior"))
                return "scifi";
            else
                return "rusty";
        }

        private string DeterminePirateZone(MySlimBlock block)
        {
            var subtype = block.BlockDefinition?.Id.SubtypeId.String ?? "";
            
            if (subtype.Contains("Cargo"))
                return "treasure";
            else if (subtype.Contains("Weapon"))
                return "combat";
            else if (subtype.Contains("Thrust"))
                return "propulsion";
            else
                return "hull";
        }

        private void ApplyWeatheredBase(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 3; // Weathered metal base
            }
        }

        private void ApplyBattleDamage(HashSet<MySlimBlock> blocks)
        {
            foreach (var block in blocks)
            {
                // Simulate battle damage with darker colors
                if (_random.NextDouble() < 0.3)
                {
                    _colorResults[block.Position] = 0; // Black scorch marks
                }
            }
        }

        private void ApplyPirateMarkings(HashSet<MySlimBlock> blocks)
        {
            // Apply skull and crossbones patterns on large flat surfaces
            var largeArmor = blocks.Where(b => 
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Large") == true &&
                b.BlockDefinition?.Id.SubtypeId.String?.Contains("Armor") == true
            ).ToList();
            
            foreach (var block in largeArmor)
            {
                if (_random.NextDouble() < 0.15)
                {
                    _colorResults[block.Position] = 1; // Bone white for skull
                }
            }
        }

        private void ApplyRustAndWear(HashSet<MySlimBlock> blocks)
        {
            // Apply rust to edges and exposed areas
            foreach (var block in blocks)
            {
                var neighbors = GetNeighborCount(block, blocks);
                if (neighbors < 4) // Edge block
                {
                    if (_random.NextDouble() < 0.6)
                    {
                        _colorResults[block.Position] = 4 + _random.Next(3); // Rust colors
                    }
                }
            }
        }

        private void ApplyTreasureAccents(HashSet<MySlimBlock> blocks)
        {
            if (_variant == "treasure")
            {
                var cargoBlocks = blocks.Where(b => 
                    b.BlockDefinition?.Id.SubtypeId.String?.Contains("Cargo") == true
                ).ToList();
                
                foreach (var block in cargoBlocks)
                {
                    _colorResults[block.Position] = 0; // Gold
                }
            }
        }

        private int GetNeighborCount(MySlimBlock block, HashSet<MySlimBlock> allBlocks)
        {
            int count = 0;
            var directions = new[] {
                Vector3I.Up, Vector3I.Down,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Forward, Vector3I.Backward
            };
            
            foreach (var dir in directions)
            {
                var neighborPos = block.Position + dir;
                if (allBlocks.Any(b => b.Position == neighborPos))
                    count++;
            }
            
            return count;
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedPiratePaint", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("EnhancedPiratePaint", $"ERROR: {message}");
        }
    }
}