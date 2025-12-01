using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PaintJob.App.Skins
{
    /// <summary>
    /// Manages skin application to blocks and grids
    /// </summary>
    public class SkinManager
    {
        private readonly ISkinProvider _skinProvider;
        private readonly Dictionary<Vector3I, MyStringHash> _skinResults;
        private SkinPalette _currentPalette;
        
        public SkinManager(ISkinProvider skinProvider)
        {
            _skinProvider = skinProvider ?? throw new ArgumentNullException(nameof(skinProvider));
            _skinResults = new Dictionary<Vector3I, MyStringHash>();
            _currentPalette = new SkinPalette();
        }
        
        /// <summary>
        /// Gets the current skin palette
        /// </summary>
        public SkinPalette CurrentPalette => _currentPalette;
        
        /// <summary>
        /// Sets a skin for a specific block position
        /// </summary>
        public void SetBlockSkin(Vector3I position, MyStringHash skinId)
        {
            _skinResults[position] = skinId;
        }
        
        /// <summary>
        /// Gets the skin assigned to a block position
        /// </summary>
        public MyStringHash GetBlockSkin(Vector3I position)
        {
            return _skinResults.TryGetValue(position, out var skinId) 
                ? skinId 
                : MyStringHash.NullOrEmpty;
        }
        
        /// <summary>
        /// Applies skins and colors to a grid
        /// </summary>
        public void ApplySkinsAndColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, Vector3[] colorPalette)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            
            var playerId = GetLocalPlayerId();
            var appliedCount = 0;
            var blocks = grid.GetBlocks();
            
            if (blocks == null || blocks.Count == 0)
                return;
            
            foreach (var block in blocks)
            {
                try
                {
                    // Get color for this block
                    Vector3? color = null;
                    if (colorResults != null && colorResults.TryGetValue(block.Position, out var colorIndex))
                    {
                        if (colorPalette != null && colorIndex >= 0 && colorIndex < colorPalette.Length)
                        {
                            color = colorPalette[colorIndex];
                        }
                    }
                    
                    // Get skin for this block
                    var skinId = GetBlockSkin(block.Position);
                    
                    // Validate skin if present
                    if (skinId != MyStringHash.NullOrEmpty && playerId > 0)
                    {
                        skinId = _skinProvider.ValidateSkin(skinId, playerId);
                    }
                    
                    // Apply both color and skin
                    if (color.HasValue || skinId != MyStringHash.NullOrEmpty)
                    {
                        grid.ChangeColorAndSkin(block, color, skinId != MyStringHash.NullOrEmpty ? (MyStringHash?)skinId : null);
                        appliedCount++;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to apply skin/color to block at {block.Position}: {ex.Message}");
                }
            }
            
            LogInfo($"Applied skins/colors to {appliedCount} blocks");
        }
        
        /// <summary>
        /// Generates a skin palette based on theme
        /// </summary>
        public void GenerateSkinPalette(string theme, int seed)
        {
            _currentPalette.Clear();
            var random = new Random(seed);
            var availableSkins = _skinProvider.GetAvailableSkins();
            
            if (availableSkins == null || availableSkins.Count == 0)
            {
                LogWarning("No available skins found");
                return;
            }
            
            // Select skins based on theme
            switch (theme?.ToLower())
            {
                case "military":
                    SelectMilitarySkins(availableSkins, random);
                    break;
                case "industrial":
                    SelectIndustrialSkins(availableSkins, random);
                    break;
                case "racing":
                    SelectRacingSkins(availableSkins, random);
                    break;
                case "alien":
                    SelectAlienSkins(availableSkins, random);
                    break;
                default:
                    SelectDefaultSkins(availableSkins, random);
                    break;
            }
        }
        
        private void SelectMilitarySkins(IReadOnlyList<MyStringHash> availableSkins, Random random)
        {
            // Prefer camouflage, armor, and tactical skins
            var militarySkins = FilterSkinsByKeywords(availableSkins, "camo", "armor", "heavy", "tactical", "military");
            
            if (militarySkins.Any())
            {
                _currentPalette.PrimarySkin = militarySkins[random.Next(militarySkins.Count)];
                _currentPalette.AddSkin(_currentPalette.PrimarySkin, "primary");
            }
            
            // Add additional military-themed skins
            foreach (var skin in militarySkins.Take(3))
            {
                _currentPalette.AddSkin(skin, "military");
            }
        }
        
        private void SelectIndustrialSkins(IReadOnlyList<MyStringHash> availableSkins, Random random)
        {
            // Prefer rusty, worn, and industrial skins
            var industrialSkins = FilterSkinsByKeywords(availableSkins, "rust", "worn", "industrial", "dirty", "scratched");
            
            if (industrialSkins.Any())
            {
                _currentPalette.PrimarySkin = industrialSkins[random.Next(industrialSkins.Count)];
                _currentPalette.AddSkin(_currentPalette.PrimarySkin, "primary");
            }
        }
        
        private void SelectRacingSkins(IReadOnlyList<MyStringHash> availableSkins, Random random)
        {
            // Prefer carbon fiber, neon, and racing skins
            var racingSkins = FilterSkinsByKeywords(availableSkins, "carbon", "neon", "racing", "sport", "speed");
            
            if (racingSkins.Any())
            {
                _currentPalette.PrimarySkin = racingSkins[random.Next(racingSkins.Count)];
                _currentPalette.AddSkin(_currentPalette.PrimarySkin, "primary");
            }
        }
        
        private void SelectAlienSkins(IReadOnlyList<MyStringHash> availableSkins, Random random)
        {
            // Prefer sci-fi, alien, and exotic skins
            var alienSkins = FilterSkinsByKeywords(availableSkins, "alien", "scifi", "exotic", "energy", "plasma");
            
            if (alienSkins.Any())
            {
                _currentPalette.PrimarySkin = alienSkins[random.Next(alienSkins.Count)];
                _currentPalette.AddSkin(_currentPalette.PrimarySkin, "primary");
            }
        }
        
        private void SelectDefaultSkins(IReadOnlyList<MyStringHash> availableSkins, Random random)
        {
            // Select random skins
            if (availableSkins.Count > 0)
            {
                _currentPalette.PrimarySkin = availableSkins[random.Next(availableSkins.Count)];
                _currentPalette.AddSkin(_currentPalette.PrimarySkin, "primary");
            }
        }
        
        private List<MyStringHash> FilterSkinsByKeywords(IReadOnlyList<MyStringHash> skins, params string[] keywords)
        {
            var filtered = new List<MyStringHash>();
            
            foreach (var skin in skins)
            {
                var skinString = skin.String?.ToLower() ?? "";
                if (keywords.Any(keyword => skinString.Contains(keyword.ToLower())))
                {
                    filtered.Add(skin);
                }
            }
            
            return filtered.Any() ? filtered : skins.ToList();
        }
        
        private ulong GetLocalPlayerId()
        {
            try
            {
                return MyAPIGateway.Session?.LocalHumanPlayer?.SteamUserId ?? 0;
            }
            catch
            {
                return 0;
            }
        }
        
        /// <summary>
        /// Clears all skin assignments
        /// </summary>
        public void Clear()
        {
            _skinResults.Clear();
            _currentPalette.Clear();
        }
        
        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("SkinManager", message);
        }
        
        private void LogWarning(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("SkinManager", $"WARNING: {message}");
        }
        
        private void LogError(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("SkinManager", $"ERROR: {message}");
        }
    }
}