using System.Collections.Generic;
using System.Linq;
using PaintJob.App.Extensions;
using PaintJob.Extensions;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;

namespace PaintJob.App.PaintFactors
{
    public class LightBlockColorFactor : BaseFactor
    {
        private static bool AppliesTo(MySlimBlock block, MyCubeGrid grid)
        {
            var type = block.BlockDefinition.Id.TypeId;
            return GridUtilities.IsExteriorBlock(block, grid) &&
                   (type == typeof(MyObjectBuilder_LightingBlock) ||
                    type == typeof(MyObjectBuilder_InteriorLight) ||
                    type == typeof(MyObjectBuilder_ReflectorLight) ||
                    type == typeof(MyObjectBuilder_SignalLight));
        }

        private static void ChangeLightEffect(MySlimBlock block)
        {
            // Assuming the light block is an instance of IMyLightingBlock
            if (!(block.FatBlock is IMyLightingBlock lightBlock))
                return;

            var playerEntity = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;
            if (playerEntity == null)
                return;

            var playerRight = playerEntity.WorldMatrix.Right;
            var blockToPlayer = playerEntity.WorldMatrix.Translation - block.WorldPosition;

            var dotProduct = Vector3.Dot(playerRight, blockToPlayer);

            if (dotProduct >= 0)
            {
                // Port side - red light
                lightBlock.Color = Color.Red;
                lightBlock.BlinkIntervalSeconds = 0;
                lightBlock.BlinkLength = 50;
                lightBlock.BlinkOffset = 0;
                lightBlock.Intensity = 2;
                lightBlock.Radius = 20;
                lightBlock.Falloff = 1;
                return;

            }
            // Starboard side - green light
            lightBlock.Color = Color.Green;
            lightBlock.BlinkIntervalSeconds = 0;
            lightBlock.BlinkLength = 50;
            lightBlock.BlinkOffset = 0;
            lightBlock.Intensity = 2;
            lightBlock.Radius = 20;
            lightBlock.Falloff = 1;
        }

        public override Dictionary<Vector3I, int> Apply(MyCubeGrid grid, Dictionary<Vector3I, int> currentColors)
        {
            var blocks = grid.GetBlocks();

            foreach (var block in blocks.Where(block => AppliesTo(block, grid)))
            {
                ChangeLightEffect(block);
            }

            return currentColors;
        }
    }

}