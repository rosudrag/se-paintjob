using System.Collections.Generic;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.Skins
{
    /// <summary>
    /// Context information for skin selection
    /// </summary>
    public class SkinSelectionContext
    {
        /// <summary>
        /// The block to apply skin to
        /// </summary>
        public MySlimBlock Block { get; set; }
        
        /// <summary>
        /// The block's position in grid coordinates
        /// </summary>
        public Vector3I Position { get; set; }
        
        /// <summary>
        /// The block's subtype ID (e.g., "LargeBlockArmorBlock")
        /// </summary>
        public string BlockSubtype { get; set; }
        
        /// <summary>
        /// The selected color index for this block (if any)
        /// </summary>
        public int? ColorIndex { get; set; }
        
        /// <summary>
        /// Pattern type being applied (e.g., "camouflage", "stripes", "digital")
        /// </summary>
        public string PatternType { get; set; }
        
        /// <summary>
        /// Zone or area classification (e.g., "hull", "interior", "weapon")
        /// </summary>
        public string Zone { get; set; }
        
        /// <summary>
        /// Random seed for deterministic skin selection
        /// </summary>
        public int Seed { get; set; }
        
        /// <summary>
        /// Additional metadata for skin selection
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }
        
        public SkinSelectionContext()
        {
            Metadata = new Dictionary<string, object>();
        }
    }
}