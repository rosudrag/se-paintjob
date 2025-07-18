namespace PaintJob.App.PaintAlgorithms.Common
{
    /// <summary>
    /// Math helper methods for .NET Framework 4.8 compatibility.
    /// </summary>
    internal static class MathUtils
    {
        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        
        /// <summary>
        /// Clamps a value between a minimum and maximum value.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}