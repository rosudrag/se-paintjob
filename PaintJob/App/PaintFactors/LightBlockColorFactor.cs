﻿using System.Collections.Generic;
using MonoMod.Utils;
using PaintJob.App.Extensions;
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

        private static Color GetColor(MySlimBlock block)
        {
            // Assuming the light block is an instance of IMyLightingBlock
            var lightBlock = block.FatBlock as IMyLightingBlock;
            if (lightBlock == null)
                return Color.Black;

            var playerEntity = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;
            if (playerEntity == null)
                return Color.Black;

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

        public override Dictionary<Vector3I, Color> Apply(MyCubeGrid grid, Dictionary<Vector3I, Color> currentColors)
        {
            var result = new Dictionary<Vector3I, Color>();
            result.AddRange(currentColors);
            var blocks = grid.GetBlocks();

            foreach (var block in blocks)
            {
                if (AppliesTo(block, grid))
                {
                    var color = GetColor(block);
                    result[block.Position] = color;
                }
            }

            return result;
        }
    }

}