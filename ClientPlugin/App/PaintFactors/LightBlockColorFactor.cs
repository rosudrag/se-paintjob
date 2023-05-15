using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;

namespace ClientPlugin.App.PaintFactors
{
    public class LightBlockColorFactor : IColorFactor
    {
        // TODO: use the first factor for block color BUT because it is a light block 
        // we can set the light settings as such: red light if PORT side and green light if starboard side
        public bool AppliesTo(MySlimBlock block, MyCubeGrid grid)
        {
            var type = block.BlockDefinition.Id.TypeId;
            return type == typeof(MyObjectBuilder_LightingBlock) ||
                   type == typeof(MyObjectBuilder_InteriorLight) ||
                   type == typeof(MyObjectBuilder_ReflectorLight) ||
                   type == typeof(MyObjectBuilder_SignalLight);
        }

        public Color GetColor(MySlimBlock block, MyCubeGrid grid, Color current, IList<Color> colors)
        {
            if (GridUtilities.IsExteriorBlock(block, grid))
            {
                // Assuming the light block is an instance of IMyLightingBlock
                var lightBlock = block.FatBlock as IMyLightingBlock;
                if (lightBlock == null)
                    return current;

                var playerEntity = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;
                if (playerEntity == null)
                    return current;

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
                    return Color.Red;

                }
                else
                {
                    // Starboard side - green light
                    lightBlock.Color = Color.Green;
                    lightBlock.BlinkIntervalSeconds = 0;
                    lightBlock.BlinkLength = 50;
                    lightBlock.BlinkOffset = 0;
                    lightBlock.Intensity = 2;
                    lightBlock.Radius = 20;
                    lightBlock.Falloff = 1;
                    return Color.Green;
                }


            }

            return current;
        }

        public void Clean()
        {

        }
    }

}