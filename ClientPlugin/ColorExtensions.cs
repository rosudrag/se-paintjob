using System;
using VRageMath;

namespace ClientPlugin
{
    public static class ColorExtensions
    {
        public static Color Darken(this Color color, float percentage)
        {
            var factor = 1f - percentage;
            return new Color(
                ClampByte(color.R * factor),
                ClampByte(color.G * factor),
                ClampByte(color.B * factor));
        }

        public static Color Lighten(this Color color, float percentage)
        {
            var factor = 1f + percentage;
            return new Color(
                ClampByte(color.R * factor),
                ClampByte(color.G * factor),
                ClampByte(color.B * factor));
        }

        private static byte ClampByte(float value)
        {
            return (byte)Math.Min(Math.Max(value, 0), 255);
        }
    }
}