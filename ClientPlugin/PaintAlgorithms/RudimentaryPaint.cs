﻿using System.Linq;
using Sandbox.Game.Entities;
using VRage.Game.ModAPI;
using VRageMath;

namespace ClientPlugin.PaintAlgorithms
{
    public abstract class PaintAlgorithm
    {
        protected readonly PaintJobStateSystem _state = PaintJobStateSystem.Instance;

        public abstract void Apply(IMyCubeGrid grid);
    }
    public class RudimentaryPaint: PaintAlgorithm
    {
        public static readonly RudimentaryPaint Instance = new RudimentaryPaint();

        public override void Apply(IMyCubeGrid grid)
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