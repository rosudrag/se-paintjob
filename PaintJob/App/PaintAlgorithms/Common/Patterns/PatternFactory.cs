using System;
using System.Collections.Generic;

namespace PaintJob.App.PaintAlgorithms.Common.Patterns
{
    /// <summary>
    /// Factory for creating and managing pattern strategies.
    /// </summary>
    public class PatternFactory
    {
        private readonly Dictionary<string, IPatternStrategy> _strategies;

        public PatternFactory()
        {
            _strategies = new Dictionary<string, IPatternStrategy>();
        }

        /// <summary>
        /// Registers a pattern strategy.
        /// </summary>
        public void RegisterStrategy(string name, IPatternStrategy strategy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            _strategies[name.ToLowerInvariant()] = strategy;
        }

        /// <summary>
        /// Gets a pattern strategy by name.
        /// </summary>
        public IPatternStrategy GetStrategy(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            return _strategies.TryGetValue(name.ToLowerInvariant(), out var strategy)
                ? strategy
                : throw new InvalidOperationException($"Pattern strategy '{name}' not found");
        }

        /// <summary>
        /// Checks if a strategy exists.
        /// </summary>
        public bool HasStrategy(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && 
                   _strategies.ContainsKey(name.ToLowerInvariant());
        }

        /// <summary>
        /// Gets all available strategy names.
        /// </summary>
        public IEnumerable<string> GetAvailableStrategies()
        {
            return _strategies.Keys;
        }

        /// <summary>
        /// Clears all registered strategies.
        /// </summary>
        public void Clear()
        {
            _strategies.Clear();
        }
    }
}