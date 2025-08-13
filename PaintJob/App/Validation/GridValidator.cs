using System.Linq;
using Sandbox.Game.Entities;
using VRageMath;
using PaintJob.App.Constants;

namespace PaintJob.App.Validation
{
    public class GridValidator
    {
        public static bool IsGridInRange(MyCubeGrid grid, Vector3D playerPosition)
        {
            if (grid == null || grid.Physics == null) return false;
            
            var distance = Vector3D.Distance(playerPosition, grid.PositionComp.GetPosition());
            return distance <= PaintJobConstants.MAX_GRID_RANGE;
        }
        
        public static float GetDistanceToGrid(MyCubeGrid grid, Vector3D playerPosition)
        {
            if (grid == null || grid.Physics == null) return float.MaxValue;
            return (float)Vector3D.Distance(playerPosition, grid.PositionComp.GetPosition());
        }
        
        public static bool IsGridOwnedByPlayer(MyCubeGrid grid, long playerId)
        {
            if (grid == null) return false;
            
            var blocks = grid.GetBlocks();
            if (blocks.Count == 0) return false;
            
            int ownedBlocks = 0;
            foreach (var block in blocks)
            {
                if (block.FatBlock != null && block.FatBlock.OwnerId == playerId)
                {
                    ownedBlocks++;
                }
            }
            
            var blocksWithOwnership = blocks.Count(b => b.FatBlock != null);
            if (blocksWithOwnership > 0 && (float)ownedBlocks / blocksWithOwnership > PaintJobConstants.MIN_OWNERSHIP_PERCENTAGE)
                return true;
            
            return grid.BigOwners.Contains(playerId);
        }
        
        public static bool CanPlayerPaintGrid(MyCubeGrid grid, long playerId, Vector3D playerPosition)
        {
            return IsGridInRange(grid, playerPosition) && IsGridOwnedByPlayer(grid, playerId);
        }
    }
}