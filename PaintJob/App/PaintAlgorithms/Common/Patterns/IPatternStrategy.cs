using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;

namespace PaintJob.App.PaintAlgorithms.Common.Patterns
{
    /// <summary>
    /// Defines the contract for pattern generation strategies.
    /// </summary>
    public interface IPatternStrategy
    {
        /// <summary>
        /// Gets the name of the pattern.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Generates a pattern for the specified positions.
        /// </summary>
        /// <param name="grid">The grid to apply the pattern to.</param>
        /// <param name="positions">The positions to apply the pattern to.</param>
        /// <param name="colorIndices">Available color indices for the pattern.</param>
        /// <param name="parameters">Additional parameters for pattern generation.</param>
        /// <returns>A dictionary mapping positions to color indices.</returns>
        Dictionary<Vector3I, int> GeneratePattern(
            MyCubeGrid grid,
            IEnumerable<Vector3I> positions,
            int[] colorIndices,
            PatternParameters parameters);
    }

    /// <summary>
    /// Parameters for pattern generation.
    /// </summary>
    public class PatternParameters
    {
        public Vector3 Origin { get; set; }
        public float Scale { get; set; } = 2.0f;
        public float Frequency { get; set; } = 1.5f;
        public float Rotation { get; set; } = 0.0f;
        public int Seed { get; set; } = 0;
        
        /// <summary>
        /// Additional pattern-specific parameters.
        /// </summary>
        public Dictionary<string, object> CustomParameters { get; set; } = new Dictionary<string, object>();
    }
}