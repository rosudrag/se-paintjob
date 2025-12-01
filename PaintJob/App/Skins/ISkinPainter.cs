using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRage.Utils;
using VRageMath;

namespace PaintJob.App.Skins
{
    /// <summary>
    /// Interface for painters that can apply skins to blocks
    /// </summary>
    public interface ISkinPainter
    {
        /// <summary>
        /// Name of the skin painter
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Priority for application order (lower = earlier)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Applies skins to blocks based on the painter's logic
        /// </summary>
        /// <param name="grid">The grid to apply skins to</param>
        /// <param name="skinResults">Dictionary to store skin assignments by position</param>
        /// <param name="skinPalette">Available skin palette</param>
        /// <param name="context">Additional context data</param>
        void ApplySkins(
            MyCubeGrid grid, 
            Dictionary<Vector3I, MyStringHash> skinResults,
            SkinPalette skinPalette,
            object context = null);
    }
}