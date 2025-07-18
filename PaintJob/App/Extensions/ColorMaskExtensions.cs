using VRage.Game;
using VRageMath;

namespace PaintJob.App.Extensions
{
    /// <summary>
    /// Extension methods for converting between HSV colors and Space Engineers' Color Mask format.
    /// Based on BuildColors project analysis.
    /// </summary>
    public static class ColorMaskExtensions
    {
        /// <summary>
        /// Converts Space Engineers' color mask to standard HSV values.
        /// </summary>
        /// <param name="colorMask">The SE color mask with Y and Z in -1 to 1 range</param>
        /// <returns>Standard HSV values with all components in 0 to 1 range</returns>
        public static Vector3 ColorMaskToHSV(this Vector3 colorMask)
        {
            var h = MathHelper.Clamp(colorMask.X, 0f, 1f);
            var s = MathHelper.Clamp(colorMask.Y + MyColorPickerConstants.SATURATION_DELTA, 0f, 1f);
            var v = MathHelper.Clamp(colorMask.Z + MyColorPickerConstants.VALUE_DELTA - MyColorPickerConstants.VALUE_COLORIZE_DELTA, 0f, 1f);
            return new Vector3(h, s, v);
        }

        /// <summary>
        /// Converts standard HSV values to Space Engineers' color mask format.
        /// </summary>
        /// <param name="hsv">Standard HSV with all components in 0 to 1 range</param>
        /// <returns>SE color mask with Y and Z in -1 to 1 range</returns>
        public static Vector3 HSVToColorMask(this Vector3 hsv)
        {
            return new Vector3(
                MathHelper.Clamp(hsv.X, 0f, 1f), 
                MathHelper.Clamp(hsv.Y - MyColorPickerConstants.SATURATION_DELTA, -1f, 1f), 
                MathHelper.Clamp(hsv.Z - MyColorPickerConstants.VALUE_DELTA + MyColorPickerConstants.VALUE_COLORIZE_DELTA, -1f, 1f)
            );
        }

        /// <summary>
        /// Creates a color mask directly from HSV values, handling the SE format internally.
        /// This is a convenience method for creating military colors.
        /// </summary>
        /// <param name="hue">Hue in degrees (0-360)</param>
        /// <param name="saturation">Saturation percentage (0-100)</param>
        /// <param name="value">Value/brightness percentage (0-100)</param>
        /// <returns>SE color mask ready to use with ColorBlocks</returns>
        public static Vector3 CreateColorMask(float hue, float saturation, float value)
        {
            // Convert to 0-1 range
            var h = hue / 360f;
            var s = saturation / 100f;
            var v = value / 100f;
            
            return new Vector3(h, s, v).HSVToColorMask();
        }

        /// <summary>
        /// Creates a grey color mask with specified brightness.
        /// </summary>
        /// <param name="brightness">Brightness percentage (0-100)</param>
        /// <returns>SE color mask for grey color</returns>
        public static Vector3 CreateGreyMask(float brightness)
        {
            return CreateColorMask(0, 0, brightness);
        }
    }
}