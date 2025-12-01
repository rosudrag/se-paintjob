using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Definitions;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Utils;

namespace PaintJob.App.Skins
{
    /// <summary>
    /// Default implementation of ISkinProvider that discovers skins from game definitions
    /// </summary>
    public class DefaultSkinProvider : ISkinProvider
    {
        private readonly List<MyStringHash> _availableSkins;
        private readonly Random _random;
        private MySessionComponentGameInventory _inventoryComponent;
        
        public DefaultSkinProvider(int? seed = null)
        {
            _availableSkins = new List<MyStringHash>();
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            DiscoverAvailableSkins();
        }
        
        /// <summary>
        /// Discovers all available skins from game definitions
        /// </summary>
        private void DiscoverAvailableSkins()
        {
            try
            {
                // Get all asset modifier definitions (skins)
                var skinDefinitions = MyDefinitionManager.Static.GetAssetModifierDefinitions();
                
                foreach (var skinDef in skinDefinitions)
                {
                    if (skinDef != null && skinDef.Id.SubtypeId != MyStringHash.NullOrEmpty)
                    {
                        _availableSkins.Add(skinDef.Id.SubtypeId);
                        LogInfo($"Discovered skin: {skinDef.Id.SubtypeId}");
                    }
                }
                
                // Add some known common skins if not already present
                AddKnownSkin("Rusty_Armor");
                AddKnownSkin("Heavy_Rust_Armor");
                AddKnownSkin("Golden_Armor");
                AddKnownSkin("Glamour_Armor");
                AddKnownSkin("Carbon_Armor");
                AddKnownSkin("Digital_Camo_Armor");
                AddKnownSkin("Moss_Armor");
                AddKnownSkin("Sand_Armor");
                AddKnownSkin("Silver_Armor");
                AddKnownSkin("Wood_Armor");
                AddKnownSkin("Disco_Armor");
                AddKnownSkin("Mossy_Armor");
                AddKnownSkin("Frozen_Armor");
                AddKnownSkin("Neon_Colorable_Surface");
                AddKnownSkin("Retro_Armor");
                AddKnownSkin("Sci_Fi_Armor");
                AddKnownSkin("Battered_Armor");
                AddKnownSkin("Corroded_Armor");
                
                LogInfo($"Total skins discovered: {_availableSkins.Count}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to discover skins: {ex.Message}");
            }
        }
        
        private void AddKnownSkin(string skinName)
        {
            var skinHash = MyStringHash.GetOrCompute(skinName);
            if (skinHash != MyStringHash.NullOrEmpty && !_availableSkins.Contains(skinHash))
            {
                _availableSkins.Add(skinHash);
            }
        }
        
        public IReadOnlyList<MyStringHash> GetAvailableSkins()
        {
            return _availableSkins;
        }
        
        public MyStringHash SelectSkin(SkinSelectionContext context)
        {
            if (context == null || _availableSkins.Count == 0)
                return MyStringHash.NullOrEmpty;
            
            // Use pattern type to select appropriate skin
            switch (context.PatternType?.ToLower())
            {
                case "camouflage":
                    return SelectCamouflageSkin(context);
                    
                case "rusty":
                case "worn":
                    return SelectWornSkin(context);
                    
                case "tech":
                case "scifi":
                    return SelectTechSkin(context);
                    
                case "decorative":
                    return SelectDecorativeSkin(context);
                    
                default:
                    // Random selection based on seed
                    var index = Math.Abs(context.Seed + context.Position.GetHashCode()) % _availableSkins.Count;
                    return _availableSkins[index];
            }
        }
        
        private MyStringHash SelectCamouflageSkin(SkinSelectionContext context)
        {
            var camoSkins = _availableSkins.Where(s => 
                s.String != null && (
                s.String.Contains("Camo", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Moss", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Sand", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (camoSkins.Any())
            {
                var index = Math.Abs(context.Seed + context.Position.GetHashCode()) % camoSkins.Count;
                return camoSkins[index];
            }
            
            return MyStringHash.NullOrEmpty;
        }
        
        private MyStringHash SelectWornSkin(SkinSelectionContext context)
        {
            var wornSkins = _availableSkins.Where(s => 
                s.String != null && (
                s.String.Contains("Rust", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Battered", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Corroded", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Worn", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (wornSkins.Any())
            {
                var index = Math.Abs(context.Seed + context.Position.GetHashCode()) % wornSkins.Count;
                return wornSkins[index];
            }
            
            return MyStringHash.NullOrEmpty;
        }
        
        private MyStringHash SelectTechSkin(SkinSelectionContext context)
        {
            var techSkins = _availableSkins.Where(s => 
                s.String != null && (
                s.String.Contains("Sci", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Carbon", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Neon", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Digital", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (techSkins.Any())
            {
                var index = Math.Abs(context.Seed + context.Position.GetHashCode()) % techSkins.Count;
                return techSkins[index];
            }
            
            return MyStringHash.NullOrEmpty;
        }
        
        private MyStringHash SelectDecorativeSkin(SkinSelectionContext context)
        {
            var decorativeSkins = _availableSkins.Where(s => 
                s.String != null && (
                s.String.Contains("Golden", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Glamour", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Disco", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Wood", StringComparison.OrdinalIgnoreCase) ||
                s.String.Contains("Silver", StringComparison.OrdinalIgnoreCase)
            )).ToList();
            
            if (decorativeSkins.Any())
            {
                var index = Math.Abs(context.Seed + context.Position.GetHashCode()) % decorativeSkins.Count;
                return decorativeSkins[index];
            }
            
            return MyStringHash.NullOrEmpty;
        }
        
        public MyStringHash ValidateSkin(MyStringHash skinId, ulong playerId)
        {
            if (skinId == MyStringHash.NullOrEmpty)
                return skinId;
            
            try
            {
                if (_inventoryComponent != null)
                {
                    return _inventoryComponent.ValidateArmor(skinId, playerId);
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to validate skin {skinId}: {ex.Message}");
            }
            
            // If validation fails or component is not available, return the original skin
            // (the game will handle the validation)
            return skinId;
        }
        
        private void LogInfo(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("DefaultSkinProvider", message);
        }
        
        private void LogWarning(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("DefaultSkinProvider", $"WARNING: {message}");
        }
        
        private void LogError(string message)
        {
            MyAPIGateway.Utilities?.ShowMessage("DefaultSkinProvider", $"ERROR: {message}");
        }
    }
}