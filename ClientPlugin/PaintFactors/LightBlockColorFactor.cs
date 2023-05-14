using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRageMath;

namespace ClientPlugin.PaintFactors
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

                var worldMatrix = grid.WorldMatrix;
                MatrixD.Invert(ref worldMatrix, out var invertedWorldMatrix);
                var relativePos = Vector3D.Transform(block.WorldPosition, invertedWorldMatrix);

                var gridOrientation = GridUtilities.GetGridOrientation(grid);

                var relativePosRotated = Vector3D.Transform(relativePos, MatrixD.Transpose(gridOrientation));

                if (relativePosRotated.X > 0)
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

            return current;
        }
        public void Clean()
        {

        }
    }

}