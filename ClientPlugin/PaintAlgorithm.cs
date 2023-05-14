using System.Linq;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRageMath;

namespace ClientPlugin
{
    public class PaintAlgorithm
    {
        private readonly PaintJobStateSystem _state = PaintJobStateSystem.Instance;

        public void ApplyRudimentary(IMyCubeGrid grid)
        {
            if (grid is MyCubeGrid targetCubeGrid)
            {
                // Get the first color in the state
                var firstColor = _state.GetColors().First();
                var colorHSV = firstColor.ColorToHSV();

                // Iterate through all the blocks in the grid and paint them with the first color
                var blocks = targetCubeGrid.GetBlocks();

                foreach (var block in blocks)
                {
                    targetCubeGrid.ChangeColorAndSkin(block, colorHSV);
                }
            }
        }
    }
}