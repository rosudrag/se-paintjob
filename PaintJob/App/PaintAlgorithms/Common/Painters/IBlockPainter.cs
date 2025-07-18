using System.Collections.Generic;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Common.Painters
{
    /// <summary>
    /// Interface for components that apply colors to blocks.
    /// </summary>
    public interface IBlockPainter
    {
        /// <summary>
        /// Gets the name of this painter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the priority of this painter (higher values override lower).
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Applies colors to blocks based on the painter's logic.
        /// </summary>
        /// <param name="grid">The grid being painted.</param>
        /// <param name="colorResults">The color results dictionary to update.</param>
        /// <param name="context">Context information for painting.</param>
        void ApplyColors(MyCubeGrid grid, Dictionary<Vector3I, int> colorResults, PaintContext context);
    }

    /// <summary>
    /// Context information for painting operations.
    /// </summary>
    public class PaintContext
    {
        public IColorScheme ColorScheme { get; set; }
        public Analysis.ShipGeometryAnalyzer.GeometryAnalysis Geometry { get; set; }
        public Dictionary<MySlimBlock, Analysis.BlockSpatialAnalyzer.BlockSpatialInfo> SpatialData { get; set; }
        public Analysis.SurfaceAnalyzer.SurfaceAnalysis Surfaces { get; set; }
        public Analysis.FunctionalClusterAnalyzer.ClusterAnalysis Functional { get; set; }
        public Analysis.SpatialOrientationAnalyzer.OrientationAnalysis Orientation { get; set; }
        public HashSet<MySlimBlock> Blocks { get; set; }
    }
}