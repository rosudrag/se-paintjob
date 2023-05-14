using System;
using VRageMath;

namespace ClientPlugin
{
    public static class ColorExtensions
    {
        public static Color Darken(this Color color, float percentage)
        {
            return new Color(
                (byte)Math.Max(color.R * (1 - percentage), 0),
                (byte)Math.Max(color.G * (1 - percentage), 0),
                (byte)Math.Max(color.B * (1 - percentage), 0));
        }

        public static Color Lighten(this Color color, float percentage)
        {
            return new Color(
                (byte)Math.Min(color.R * (1 + percentage), 255),
                (byte)Math.Min(color.G * (1 + percentage), 255),
                (byte)Math.Min(color.B * (1 + percentage), 255));
        }
    }
}