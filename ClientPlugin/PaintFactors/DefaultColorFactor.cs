using System;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.ObjectBuilders;
using VRageMath;

namespace ClientPlugin.PaintFactors
{
    public class DefaultColorFactor : IColorFactor
    {
        private Dictionary<MyObjectBuilderType, Color> _colorCache = new Dictionary<MyObjectBuilderType, Color>();

        public bool AppliesTo(MySlimBlock block)
        {
            // This is the default factor, it applies to all blocks
            return true;
        }

        public Color GetColor(MySlimBlock block, IList<Color> colors, Vector3I gridSize, Vector3I relativePos, MyCubeGrid grid)
        {
            if (!_colorCache.TryGetValue(block.BlockDefinition.Id.TypeId, out var color))
            {
                // Calculate how far along the grid the block is as a fraction (from 0 to 1)
                var fraction = new Vector3D((double)relativePos.X / gridSize.X, (double)relativePos.Y / gridSize.Y, (double)relativePos.Z / gridSize.Z);

                // Clamp the index to be within the bounds of the color list
                var index = (int)Math.Round(fraction.Length() * (colors.Count - 1));
                index = Math.Max(0, Math.Min(index, colors.Count - 1));

                color = colors[index];

                _colorCache[block.BlockDefinition.Id.TypeId] = color;
            }

            return color;
        }

        public void Clean()
        {
            _colorCache = new Dictionary<MyObjectBuilderType, Color>();
        }
    }
}