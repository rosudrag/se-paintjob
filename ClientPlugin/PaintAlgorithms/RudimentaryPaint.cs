using System.Linq;
using Sandbox.Game.Entities;
using VRageMath;

namespace ClientPlugin.PaintAlgorithms
{
    public class RudimentaryPaint : PaintAlgorithm
    {
        public RudimentaryPaint(IPaintJobStateSystem stateSystem) : base(stateSystem)
        {
        }
        private static IPaintJobStateSystem _state => IoC.Resolve<IPaintJobStateSystem>();

        public override void Apply(MyCubeGrid grid)
        {
            // Get the first color in the state
            var firstColor = _state.GetColors().First();
            var colorHSV = firstColor.ColorToHSV();

            // Iterate through all the blocks in the grid and paint them with the first color
            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                grid.ChangeColorAndSkin(block, colorHSV);
            }
        }
        public override void Clean()
        {
        }
    }
}