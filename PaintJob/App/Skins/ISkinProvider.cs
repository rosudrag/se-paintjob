using System.Collections.Generic;
using VRage.Utils;

namespace PaintJob.App.Skins
{
    /// <summary>
    /// Interface for providing skin selections for blocks
    /// </summary>
    public interface ISkinProvider
    {
        /// <summary>
        /// Gets available skin IDs that can be applied to blocks
        /// </summary>
        IReadOnlyList<MyStringHash> GetAvailableSkins();
        
        /// <summary>
        /// Selects a skin based on the given criteria
        /// </summary>
        /// <param name="context">The skin selection context containing block and analysis data</param>
        /// <returns>The selected skin ID, or NullOrEmpty for no skin</returns>
        MyStringHash SelectSkin(SkinSelectionContext context);
        
        /// <summary>
        /// Validates if a skin can be applied by the current player
        /// </summary>
        /// <param name="skinId">The skin ID to validate</param>
        /// <param name="playerId">The player's Steam ID</param>
        /// <returns>The validated skin ID, or NullOrEmpty if not valid</returns>
        MyStringHash ValidateSkin(MyStringHash skinId, ulong playerId);
    }
}