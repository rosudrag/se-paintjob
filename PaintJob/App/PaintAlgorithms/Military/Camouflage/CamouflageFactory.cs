using PaintJob.App.Analysis;
using PaintJob.App.PaintAlgorithms.Common.Patterns;

namespace PaintJob.App.PaintAlgorithms.Military.Camouflage
{
    /// <summary>
    /// Factory for creating camouflage strategies based on ship characteristics.
    /// Extends the generic PatternFactory with military-specific functionality.
    /// </summary>
    public class CamouflageFactory : PatternFactory
    {
        public CamouflageFactory()
        {
            // Register default military camouflage patterns
            RegisterStrategy("digital", new DigitalCamouflageStrategy());
            RegisterStrategy("hexagonal", new HexagonalCamouflageStrategy());
            RegisterStrategy("organic", new OrganicCamouflageStrategy());
        }


        /// <summary>
        /// Selects an appropriate camouflage strategy based on ship profile.
        /// </summary>
        public IPatternStrategy SelectStrategyForProfile(ShipGeometryAnalyzer.ShipProfile profile)
        {
            switch (profile)
            {
                case ShipGeometryAnalyzer.ShipProfile.Wedge:
                case ShipGeometryAnalyzer.ShipProfile.Flat:
                    return GetStrategy("hexagonal");
                case ShipGeometryAnalyzer.ShipProfile.Cylindrical:
                case ShipGeometryAnalyzer.ShipProfile.Elongated:
                    return GetStrategy("organic");
                default:
                    return GetStrategy("digital");
            }
        }

    }
}