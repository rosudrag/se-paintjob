using System.Collections.Generic;
using Sandbox.Game.Entities;
using VRageMath;
using PaintJob.App.Skins;
using VRage.Utils;

namespace PaintJob.App.PaintAlgorithms
{
    /// <summary>
    /// Base class for paint algorithms that support both colors and skins
    /// </summary>
    public abstract class SkinAwarePaintAlgorithm : PaintAlgorithm
    {
        protected SkinManager _skinManager;
        protected ISkinProvider _skinProvider;
        protected List<ISkinPainter> _skinPainters;
        
        /// <summary>
        /// Whether to apply skins in addition to colors
        /// </summary>
        public bool EnableSkins { get; set; } = true;
        
        /// <summary>
        /// Skin theme to use (e.g., "military", "industrial", "racing")
        /// </summary>
        public string SkinTheme { get; set; } = "default";
        
        protected SkinAwarePaintAlgorithm()
        {
            _skinProvider = new DefaultSkinProvider();
            _skinManager = new SkinManager(_skinProvider);
            _skinPainters = new List<ISkinPainter>();
            InitializeSkinPainters();
        }
        
        /// <summary>
        /// Initializes the skin painters for this algorithm
        /// </summary>
        protected virtual void InitializeSkinPainters()
        {
            // Default implementation adds pattern painter
            _skinPainters.Add(new Skins.Painters.PatternSkinPainter());
        }
        
        /// <summary>
        /// Generates both color and skin palettes
        /// </summary>
        protected override void GeneratePalette(MyCubeGrid grid)
        {
            // Generate color palette (implemented by derived classes)
            GenerateColorPalette(grid);
            
            // Generate skin palette if skins are enabled
            if (EnableSkins)
            {
                var seed = unchecked((int)grid.EntityId);
                _skinManager.GenerateSkinPalette(SkinTheme, seed);
            }
        }
        
        /// <summary>
        /// Generates the color palette (must be implemented by derived classes)
        /// </summary>
        protected abstract void GenerateColorPalette(MyCubeGrid grid);
        
        /// <summary>
        /// Applies both colors and skins to the grid
        /// </summary>
        protected virtual void ApplyColorsAndSkins(
            MyCubeGrid grid, 
            Dictionary<Vector3I, int> colorResults, 
            Vector3[] colorPalette)
        {
            if (!EnableSkins)
            {
                // Apply colors only (legacy behavior)
                ApplyColorsToGrid(grid, colorResults, colorPalette);
            }
            else
            {
                // Apply skins first
                ApplySkins(grid);
                
                // Then apply both colors and skins together
                _skinManager.ApplySkinsAndColors(grid, colorResults, colorPalette);
            }
        }
        
        /// <summary>
        /// Applies skins to the grid using skin painters
        /// </summary>
        protected virtual void ApplySkins(MyCubeGrid grid)
        {
            if (!EnableSkins || _skinPainters == null || _skinPainters.Count == 0)
                return;
            
            var skinResults = new Dictionary<Vector3I, MyStringHash>();
            var palette = _skinManager.CurrentPalette;
            
            // Apply each skin painter in priority order
            var sortedPainters = new List<ISkinPainter>(_skinPainters);
            sortedPainters.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            foreach (var painter in sortedPainters)
            {
                painter.ApplySkins(grid, skinResults, palette, null);
            }
            
            // Store results in skin manager
            foreach (var kvp in skinResults)
            {
                _skinManager.SetBlockSkin(kvp.Key, kvp.Value);
            }
        }
        
        /// <summary>
        /// Legacy method for applying colors only
        /// </summary>
        protected virtual void ApplyColorsToGrid(
            MyCubeGrid grid, 
            Dictionary<Vector3I, int> colorResults, 
            Vector3[] colorPalette)
        {
            var blocks = grid.GetBlocks();
            foreach (var block in blocks)
            {
                if (colorResults.TryGetValue(block.Position, out var colorIndex))
                {
                    if (colorIndex >= 0 && colorIndex < colorPalette.Length)
                    {
                        grid.ColorBlocks(block.Min, block.Max, colorPalette[colorIndex], false);
                    }
                }
            }
        }
        
        public override void Clean()
        {
            _skinManager?.Clear();
        }
    }
}