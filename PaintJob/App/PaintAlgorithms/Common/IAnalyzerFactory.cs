using PaintJob.App.Analysis;

namespace PaintJob.App.PaintAlgorithms.Common
{
    /// <summary>
    /// Factory interface for creating analysis components used in paint jobs.
    /// </summary>
    public interface IAnalyzerFactory
    {
        /// <summary>
        /// Creates a ship geometry analyzer instance.
        /// </summary>
        ShipGeometryAnalyzer CreateGeometryAnalyzer();

        /// <summary>
        /// Creates a block spatial analyzer instance.
        /// </summary>
        BlockSpatialAnalyzer CreateSpatialAnalyzer();

        /// <summary>
        /// Creates a surface analyzer instance.
        /// </summary>
        SurfaceAnalyzer CreateSurfaceAnalyzer();

        /// <summary>
        /// Creates a functional cluster analyzer instance.
        /// </summary>
        FunctionalClusterAnalyzer CreateFunctionalAnalyzer();

        /// <summary>
        /// Creates a spatial orientation analyzer instance.
        /// </summary>
        SpatialOrientationAnalyzer CreateOrientationAnalyzer();

        /// <summary>
        /// Creates a pattern generator instance.
        /// </summary>
        PatternGenerator CreatePatternGenerator();
    }

}