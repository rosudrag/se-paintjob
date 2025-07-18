using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Common
{
    /// <summary>
    /// Base interface for paint job color schemes.
    /// </summary>
    public interface IColorScheme
    {
        /// <summary>
        /// Gets the color palette.
        /// </summary>
        Vector3[] ColorPalette { get; }

        /// <summary>
        /// Initializes the color palette.
        /// </summary>
        void InitializePalette(Vector3[] generatedColors);
    }
}