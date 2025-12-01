using System.Collections.Generic;
using VRage.Utils;

namespace PaintJob.App.Skins
{
    /// <summary>
    /// Represents a palette of skins that can be applied alongside colors
    /// </summary>
    public class SkinPalette
    {
        private readonly List<MyStringHash> _skins;
        private readonly Dictionary<string, List<MyStringHash>> _categorizedSkins;
        
        /// <summary>
        /// All available skins in the palette
        /// </summary>
        public IReadOnlyList<MyStringHash> Skins => _skins;
        
        /// <summary>
        /// Primary skin for base surfaces
        /// </summary>
        public MyStringHash PrimarySkin { get; set; }
        
        /// <summary>
        /// Secondary skin for accent surfaces
        /// </summary>
        public MyStringHash SecondarySkin { get; set; }
        
        /// <summary>
        /// Detail skin for special features
        /// </summary>
        public MyStringHash DetailSkin { get; set; }
        
        public SkinPalette()
        {
            _skins = new List<MyStringHash>();
            _categorizedSkins = new Dictionary<string, List<MyStringHash>>();
            PrimarySkin = MyStringHash.NullOrEmpty;
            SecondarySkin = MyStringHash.NullOrEmpty;
            DetailSkin = MyStringHash.NullOrEmpty;
        }
        
        /// <summary>
        /// Adds a skin to the palette
        /// </summary>
        public void AddSkin(MyStringHash skinId, string category = "default")
        {
            if (!_skins.Contains(skinId))
            {
                _skins.Add(skinId);
            }
            
            if (!_categorizedSkins.ContainsKey(category))
            {
                _categorizedSkins[category] = new List<MyStringHash>();
            }
            
            if (!_categorizedSkins[category].Contains(skinId))
            {
                _categorizedSkins[category].Add(skinId);
            }
        }
        
        /// <summary>
        /// Gets skins by category
        /// </summary>
        public IReadOnlyList<MyStringHash> GetSkinsByCategory(string category)
        {
            return _categorizedSkins.TryGetValue(category, out var skins) 
                ? skins 
                : new List<MyStringHash>();
        }
        
        /// <summary>
        /// Clears all skins from the palette
        /// </summary>
        public void Clear()
        {
            _skins.Clear();
            _categorizedSkins.Clear();
            PrimarySkin = MyStringHash.NullOrEmpty;
            SecondarySkin = MyStringHash.NullOrEmpty;
            DetailSkin = MyStringHash.NullOrEmpty;
        }
        
        /// <summary>
        /// Gets a skin by index, or NullOrEmpty if index is out of range
        /// </summary>
        public MyStringHash GetSkinByIndex(int index)
        {
            return index >= 0 && index < _skins.Count 
                ? _skins[index] 
                : MyStringHash.NullOrEmpty;
        }
    }
}