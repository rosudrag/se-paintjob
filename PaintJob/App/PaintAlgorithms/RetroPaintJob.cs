using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms
{
    public class RetroPaintJob : PaintAlgorithm
    {
        private readonly Dictionary<Vector3I, int> _colorResults;
        private Vector3[] _colorPalette;
        private string _variant = "80s_neon";
        private Random _random;

        public RetroPaintJob()
        {
            _colorResults = new Dictionary<Vector3I, int>();
        }

        public void SetVariant(string variant)
        {
            _variant = variant?.ToLower() ?? "80s_neon";
        }

        public override void Clean()
        {
            _colorResults.Clear();
            _colorPalette = null;
        }

        protected override void Apply(MyCubeGrid grid)
        {
            try
            {
                var blocks = grid.GetBlocks();
                _random = new Random(unchecked((int)grid.EntityId));
                
                switch (_variant)
                {
                    case "80s_neon":
                        Apply80sNeon(blocks);
                        break;
                    case "50s_chrome":
                        Apply50sChrome(blocks);
                        break;
                    case "70s_disco":
                        Apply70sDisco(blocks);
                        break;
                    case "90s_cyber":
                        Apply90sCyber(blocks);
                        break;
                    case "art_deco":
                        ApplyArtDeco(blocks);
                        break;
                    default:
                        Apply80sNeon(blocks);
                        break;
                }
                
                // Apply final colors to grid
                foreach (var block in blocks)
                {
                    if (_colorResults.TryGetValue(block.Position, out var colorIndex))
                    {
                        if (colorIndex >= 0 && colorIndex < _colorPalette.Length)
                        {
                            grid.ColorBlocks(block.Min, block.Max, _colorPalette[colorIndex], false);
                        }
                    }
                }
                
                LogInfo($"Applied retro paint job variant '{_variant}' to grid");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply retro paint job: {ex.Message}");
                throw;
            }
        }

        private void Apply80sNeon(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            
            // Apply dark base with neon accents
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 0; // Dark base (black/dark purple)
            }
            
            // Neon grid lines pattern
            foreach (var block in blocks)
            {
                var pos = block.Position;
                
                // Create grid pattern
                if (pos.X % 4 == 0 || pos.Y % 4 == 0 || pos.Z % 4 == 0)
                {
                    _colorResults[pos] = _random.Next(1, 4); // Random neon color
                }
            }
            
            // Neon accents on functional blocks
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                if (blockDef.Contains("Light"))
                {
                    _colorResults[block.Position] = 4; // Hot pink neon
                }
                else if (blockDef.Contains("Thrust"))
                {
                    _colorResults[block.Position] = 5; // Cyan neon
                }
                else if (blockDef.Contains("Cockpit"))
                {
                    _colorResults[block.Position] = 6; // Purple neon trim
                }
            }
        }

        private void Apply50sChrome(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            
            // Chrome and pastel base
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 0; // Chrome/silver base
            }
            
            // Two-tone design with fins
            var centerY = (bounds.Min.Y + bounds.Max.Y) / 2f;
            
            foreach (var block in blocks)
            {
                // Upper half gets pastel color
                if (block.Position.Y > centerY)
                {
                    _colorResults[block.Position] = 1; // Pastel mint/pink
                }
                
                // Chrome trim line
                if (Math.Abs(block.Position.Y - centerY) < 1)
                {
                    _colorResults[block.Position] = 2; // Bright chrome
                }
            }
            
            // Tail fin accent (rear blocks)
            var rearBlocks = blocks.Where(b => b.Position.Z >= bounds.Max.Z - 2).ToList();
            foreach (var block in rearBlocks)
            {
                if (block.Position.Y > centerY + 2)
                {
                    _colorResults[block.Position] = 3; // Red accent
                }
            }
            
            // Whitewall tire effect on sides
            var sideBlocks = blocks.Where(b => 
                Math.Abs(b.Position.X - bounds.Min.X) < 2 || 
                Math.Abs(b.Position.X - bounds.Max.X) < 2).ToList();
            
            foreach (var block in sideBlocks)
            {
                if (block.Position.Y < bounds.Min.Y + 2)
                {
                    _colorResults[block.Position] = 4; // White accent
                }
            }
        }

        private void Apply70sDisco(HashSet<MySlimBlock> blocks)
        {
            // Psychedelic patterns with earth tones and bright accents
            var bounds = CalculateGridBounds(blocks);
            
            foreach (var block in blocks)
            {
                var pos = block.Position;
                
                // Create swirling pattern using sine waves
                var pattern = Math.Sin(pos.X * 0.5f) + Math.Sin(pos.Y * 0.5f) + Math.Sin(pos.Z * 0.5f);
                
                if (pattern < -1)
                {
                    _colorResults[pos] = 0; // Brown/orange
                }
                else if (pattern < 0)
                {
                    _colorResults[pos] = 1; // Avocado green
                }
                else if (pattern < 1)
                {
                    _colorResults[pos] = 2; // Mustard yellow
                }
                else
                {
                    _colorResults[pos] = 3; // Burnt orange
                }
            }
            
            // Disco ball effect on lights
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                if (blockDef.Contains("Light"))
                {
                    _colorResults[block.Position] = 4; // Mirror/silver
                }
                else if (blockDef.Contains("Cockpit"))
                {
                    _colorResults[block.Position] = 5; // Shag carpet brown
                }
            }
        }

        private void Apply90sCyber(HashSet<MySlimBlock> blocks)
        {
            // Matrix-style digital rain pattern
            var bounds = CalculateGridBounds(blocks);
            
            // Black base
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 0; // Black
            }
            
            // Digital rain columns
            for (int x = (int)bounds.Min.X; x <= bounds.Max.X; x += 3)
            {
                for (int z = (int)bounds.Min.Z; z <= bounds.Max.Z; z += 3)
                {
                    var rainHeight = _random.Next(3, 8);
                    var startY = _random.Next((int)bounds.Min.Y, (int)bounds.Max.Y - rainHeight);
                    
                    foreach (var block in blocks)
                    {
                        if (block.Position.X == x && block.Position.Z == z)
                        {
                            if (block.Position.Y >= startY && block.Position.Y < startY + rainHeight)
                            {
                                var intensity = 1f - (block.Position.Y - startY) / (float)rainHeight;
                                _colorResults[block.Position] = intensity > 0.5f ? 1 : 2; // Green gradient
                            }
                        }
                    }
                }
            }
            
            // Tech accents
            foreach (var block in blocks)
            {
                var blockDef = block.BlockDefinition.Id.SubtypeName;
                
                if (blockDef.Contains("Computer") || blockDef.Contains("Control") || 
                    blockDef.Contains("Programmable"))
                {
                    _colorResults[block.Position] = 3; // Matrix green
                }
                else if (blockDef.Contains("LCD") || blockDef.Contains("Panel"))
                {
                    _colorResults[block.Position] = 4; // Screen blue
                }
            }
        }

        private void ApplyArtDeco(HashSet<MySlimBlock> blocks)
        {
            var bounds = CalculateGridBounds(blocks);
            
            // Gold and black base
            foreach (var block in blocks)
            {
                _colorResults[block.Position] = 0; // Black base
            }
            
            // Geometric sunburst pattern from center
            var centerX = (bounds.Min.X + bounds.Max.X) / 2f;
            var centerY = (bounds.Min.Y + bounds.Max.Y) / 2f;
            var centerZ = (bounds.Min.Z + bounds.Max.Z) / 2f;
            var center = new Vector3(centerX, centerY, centerZ);
            
            foreach (var block in blocks)
            {
                var pos = new Vector3(block.Position);
                var direction = pos - center;
                direction.Normalize();
                
                // Create radiating lines
                var angle = Math.Atan2(direction.Y, direction.X);
                var segmentCount = 16;
                var segment = (int)((angle + Math.PI) / (2 * Math.PI) * segmentCount);
                
                if (segment % 2 == 0)
                {
                    _colorResults[block.Position] = 1; // Gold
                }
                
                // Concentric rings
                var distance = Vector3.Distance(pos, center);
                if (Math.Abs(distance % 5) < 1)
                {
                    _colorResults[block.Position] = 2; // Brass
                }
            }
            
            // Art deco trim on edges
            foreach (var block in blocks)
            {
                var neighborCount = CountNeighbors(block, blocks);
                if (neighborCount < 4)
                {
                    _colorResults[block.Position] = 3; // Silver trim
                }
            }
        }

        private int CountNeighbors(MySlimBlock block, HashSet<MySlimBlock> allBlocks)
        {
            var count = 0;
            var offsets = new[] {
                Vector3I.Forward, Vector3I.Backward,
                Vector3I.Left, Vector3I.Right,
                Vector3I.Up, Vector3I.Down
            };
            
            foreach (var offset in offsets)
            {
                var neighborPos = block.Position + offset;
                if (allBlocks.Any(b => b.Position == neighborPos))
                {
                    count++;
                }
            }
            
            return count;
        }

        private BoundingBox CalculateGridBounds(HashSet<MySlimBlock> blocks)
        {
            if (blocks.Count == 0)
                return new BoundingBox();
            
            var firstPos = blocks.First().Position;
            var min = new Vector3(firstPos);
            var max = new Vector3(firstPos);
            
            foreach (var block in blocks)
            {
                min = Vector3.Min(min, block.Position);
                max = Vector3.Max(max, block.Position);
            }
            
            return new BoundingBox(min, max);
        }

        protected override void GeneratePalette(MyCubeGrid grid)
        {
            var seed = unchecked((int)grid.EntityId);
            var colorGenerator = new Utils.ColorSchemeGenerator(seed);
            
            // Generate retro-specific palette based on era
            _colorPalette = colorGenerator.GenerateRetroPalette(_variant);
            
            if (_colorPalette == null || _colorPalette.Length == 0)
            {
                throw new InvalidOperationException("Failed to generate retro color palette");
            }
            
            LogInfo($"Generated retro palette with {_colorPalette.Length} colors for variant '{_variant}'");
        }

        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("RetroPaintJob", message);
        }

        private void LogError(string message)
        {
            MyAPIGateway.Utilities.ShowMessage("RetroPaintJob", $"ERROR: {message}");
        }
    }
}