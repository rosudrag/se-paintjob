using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common;

namespace PaintJob.App.PaintAlgorithms.Military.Analyzers
{
    /// <summary>
    /// Military-specific implementation of the analyzer factory.
    /// </summary>
    public class MilitaryAnalyzerFactory : IAnalyzerFactory
    {
        private readonly CachedAnalysisContext _cache;

        public MilitaryAnalyzerFactory(CachedAnalysisContext cache = null)
        {
            _cache = cache;
        }

        public ShipGeometryAnalyzer CreateGeometryAnalyzer()
        {
            if (_cache != null)
                return _cache.GetOrCreateAnalysis("geometry", () => new ShipGeometryAnalyzer());
            return new ShipGeometryAnalyzer();
        }

        public BlockSpatialAnalyzer CreateSpatialAnalyzer()
        {
            if (_cache != null)
                return _cache.GetOrCreateAnalysis("spatial", () => new BlockSpatialAnalyzer());
            return new BlockSpatialAnalyzer();
        }

        public SurfaceAnalyzer CreateSurfaceAnalyzer()
        {
            if (_cache != null)
                return _cache.GetOrCreateAnalysis("surface", () => new SurfaceAnalyzer());
            return new SurfaceAnalyzer();
        }

        public FunctionalClusterAnalyzer CreateFunctionalAnalyzer()
        {
            if (_cache != null)
                return _cache.GetOrCreateAnalysis("functional", () => new FunctionalClusterAnalyzer());
            return new FunctionalClusterAnalyzer();
        }

        public SpatialOrientationAnalyzer CreateOrientationAnalyzer()
        {
            if (_cache != null)
                return _cache.GetOrCreateAnalysis("orientation", () => new SpatialOrientationAnalyzer());
            return new SpatialOrientationAnalyzer();
        }

        public PatternGenerator CreatePatternGenerator()
        {
            if (_cache != null)
                return _cache.GetOrCreateAnalysis("pattern", () => new PatternGenerator());
            return new PatternGenerator();
        }
    }
}