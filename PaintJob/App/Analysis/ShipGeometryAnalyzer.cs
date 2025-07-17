using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class ShipGeometryAnalyzer
    {
        public class GeometryAnalysis
        {
            public BoundingBox BoundingBox { get; set; }
            public Vector3 GeometricCenter { get; set; }
            public Vector3 CenterOfMass { get; set; }
            public Vector3 PrimaryAxis { get; set; }
            public Vector3 SecondaryAxis { get; set; }
            public Vector3 TertiaryAxis { get; set; }
            public float PrimaryAxisLength { get; set; }
            public float SecondaryAxisLength { get; set; }
            public float TertiaryAxisLength { get; set; }
            public ShipProfile Profile { get; set; }
            public Dictionary<SymmetryPlane, float> SymmetryScores { get; set; }
        }

        public enum ShipProfile
        {
            Elongated,     // Long and thin (like a submarine)
            Wedge,         // Triangular profile (Star Destroyer)
            Boxy,          // Roughly cubic
            Flat,          // Wide and flat (like a pancake)
            Cylindrical,   // Tube-like
            Irregular      // No clear pattern
        }

        public enum SymmetryPlane
        {
            XY, // Top-bottom symmetry
            XZ, // Left-right symmetry  
            YZ  // Front-back symmetry
        }

        public GeometryAnalysis AnalyzeGrid(MyCubeGrid grid)
        {
            var analysis = new GeometryAnalysis();
            var blocks = grid.GetBlocks();
            
            if (!blocks.Any())
                return analysis;

            // Calculate bounding box
            analysis.BoundingBox = CalculateBoundingBox(grid, blocks);
            
            // Calculate centers
            analysis.GeometricCenter = analysis.BoundingBox.Center;
            analysis.CenterOfMass = CalculateCenterOfMass(grid, blocks);
            
            // Calculate principal axes using covariance matrix
            CalculatePrincipalAxes(grid, blocks, analysis);
            
            // Determine ship profile based on axis ratios
            analysis.Profile = DetermineShipProfile(analysis);
            
            // Calculate symmetry scores
            analysis.SymmetryScores = CalculateSymmetryScores(grid, blocks, analysis);
            
            return analysis;
        }

        private BoundingBox CalculateBoundingBox(MyCubeGrid grid, HashSet<MySlimBlock> blocks)
        {
            var min = Vector3.MaxValue;
            var max = Vector3.MinValue;
            
            foreach (var block in blocks)
            {
                // Handle multi-block structures by using Min and Max positions
                var minWorld = grid.GridIntegerToWorld(block.Min);
                var maxWorld = grid.GridIntegerToWorld(block.Max);
                
                min = Vector3.Min(min, minWorld);
                max = Vector3.Max(max, maxWorld);
            }
            
            // Expand by half grid size to account for block size
            var halfGrid = grid.GridSize / 2f;
            min -= new Vector3(halfGrid);
            max += new Vector3(halfGrid);
            
            return new BoundingBox(min, max);
        }

        private Vector3 CalculateCenterOfMass(MyCubeGrid grid, HashSet<MySlimBlock> blocks)
        {
            var totalMass = 0f;
            var weightedSum = Vector3.Zero;
            
            foreach (var block in blocks)
            {
                var mass = block.BlockDefinition.Mass;
                if (mass <= 0) mass = 1f; // Default mass for blocks without defined mass
                
                // For multi-blocks, use the center of their occupied space
                var blockCenter = (grid.GridIntegerToWorld(block.Min) + grid.GridIntegerToWorld(block.Max)) / 2f;
                
                // Multi-blocks have more mass proportional to their size
                var blockVolume = (block.Max - block.Min + Vector3I.One);
                var volumeFactor = blockVolume.X * blockVolume.Y * blockVolume.Z;
                var adjustedMass = mass * volumeFactor;
                
                weightedSum += blockCenter * adjustedMass;
                totalMass += adjustedMass;
            }
            
            return totalMass > 0 ? weightedSum / totalMass : Vector3.Zero;
        }

        private void CalculatePrincipalAxes(MyCubeGrid grid, HashSet<MySlimBlock> blocks, GeometryAnalysis analysis)
        {
            // Build covariance matrix
            var covariance = new Matrix3x3();
            var center = analysis.CenterOfMass;
            var totalMass = 0f;
            
            foreach (var block in blocks)
            {
                var mass = block.BlockDefinition.Mass;
                if (mass <= 0) mass = 1f;
                
                var worldPos = grid.GridIntegerToWorld(block.Position);
                var relativePos = worldPos - center;
                
                // Add to covariance matrix
                covariance.M11 += (float)(mass * relativePos.X * relativePos.X);
                covariance.M12 += (float)(mass * relativePos.X * relativePos.Y);
                covariance.M13 += (float)(mass * relativePos.X * relativePos.Z);
                covariance.M22 += (float)(mass * relativePos.Y * relativePos.Y);
                covariance.M23 += (float)(mass * relativePos.Y * relativePos.Z);
                covariance.M33 += (float)(mass * relativePos.Z * relativePos.Z);
                
                totalMass += mass;
            }
            
            // Make symmetric
            covariance.M21 = covariance.M12;
            covariance.M31 = covariance.M13;
            covariance.M32 = covariance.M23;
            
            if (totalMass > 0)
            {
                // Manually divide each element since Matrix3x3 doesn't support /= operator
                covariance.M11 /= totalMass;
                covariance.M12 /= totalMass;
                covariance.M13 /= totalMass;
                covariance.M21 /= totalMass;
                covariance.M22 /= totalMass;
                covariance.M23 /= totalMass;
                covariance.M31 /= totalMass;
                covariance.M32 /= totalMass;
                covariance.M33 /= totalMass;
            }
            
            // For now, use simple axis-aligned approach
            // TODO: Implement proper eigenvalue decomposition for accurate principal axes
            var size = analysis.BoundingBox.Size;
            
            // Find longest axis
            if (size.X >= size.Y && size.X >= size.Z)
            {
                analysis.PrimaryAxis = Vector3.Right;
                analysis.PrimaryAxisLength = size.X;
                if (size.Y >= size.Z)
                {
                    analysis.SecondaryAxis = Vector3.Up;
                    analysis.SecondaryAxisLength = size.Y;
                    analysis.TertiaryAxis = Vector3.Forward;
                    analysis.TertiaryAxisLength = size.Z;
                }
                else
                {
                    analysis.SecondaryAxis = Vector3.Forward;
                    analysis.SecondaryAxisLength = size.Z;
                    analysis.TertiaryAxis = Vector3.Up;
                    analysis.TertiaryAxisLength = size.Y;
                }
            }
            else if (size.Y >= size.X && size.Y >= size.Z)
            {
                analysis.PrimaryAxis = Vector3.Up;
                analysis.PrimaryAxisLength = size.Y;
                if (size.X >= size.Z)
                {
                    analysis.SecondaryAxis = Vector3.Right;
                    analysis.SecondaryAxisLength = size.X;
                    analysis.TertiaryAxis = Vector3.Forward;
                    analysis.TertiaryAxisLength = size.Z;
                }
                else
                {
                    analysis.SecondaryAxis = Vector3.Forward;
                    analysis.SecondaryAxisLength = size.Z;
                    analysis.TertiaryAxis = Vector3.Right;
                    analysis.TertiaryAxisLength = size.X;
                }
            }
            else
            {
                analysis.PrimaryAxis = Vector3.Forward;
                analysis.PrimaryAxisLength = size.Z;
                if (size.X >= size.Y)
                {
                    analysis.SecondaryAxis = Vector3.Right;
                    analysis.SecondaryAxisLength = size.X;
                    analysis.TertiaryAxis = Vector3.Up;
                    analysis.TertiaryAxisLength = size.Y;
                }
                else
                {
                    analysis.SecondaryAxis = Vector3.Up;
                    analysis.SecondaryAxisLength = size.Y;
                    analysis.TertiaryAxis = Vector3.Right;
                    analysis.TertiaryAxisLength = size.X;
                }
            }
        }

        private ShipProfile DetermineShipProfile(GeometryAnalysis analysis)
        {
            var primary = analysis.PrimaryAxisLength;
            var secondary = analysis.SecondaryAxisLength;
            var tertiary = analysis.TertiaryAxisLength;
            
            // Calculate ratios
            var elongationRatio = primary / Math.Max(secondary, tertiary);
            var flatnessRatio = Math.Min(secondary, tertiary) / Math.Max(secondary, tertiary);
            var volumeRatio = (secondary * tertiary) / (primary * primary);
            
            // Determine profile based on ratios
            if (elongationRatio > 3.0f)
            {
                return ShipProfile.Elongated;
            }
            else if (flatnessRatio < 0.3f)
            {
                return ShipProfile.Flat;
            }
            else if (elongationRatio < 1.5f && flatnessRatio > 0.7f)
            {
                return ShipProfile.Boxy;
            }
            else if (volumeRatio < 0.3f && elongationRatio > 2.0f)
            {
                return ShipProfile.Wedge;
            }
            else if (flatnessRatio > 0.8f && elongationRatio > 2.0f)
            {
                return ShipProfile.Cylindrical;
            }
            
            return ShipProfile.Irregular;
        }

        private Dictionary<SymmetryPlane, float> CalculateSymmetryScores(MyCubeGrid grid, HashSet<MySlimBlock> blocks, GeometryAnalysis analysis)
        {
            var scores = new Dictionary<SymmetryPlane, float>
            {
                { SymmetryPlane.XY, 0f },
                { SymmetryPlane.XZ, 0f },
                { SymmetryPlane.YZ, 0f }
            };
            
            var center = analysis.GeometricCenter;
            var blockPositions = new HashSet<Vector3>();
            
            foreach (var block in blocks)
            {
                blockPositions.Add(grid.GridIntegerToWorld(block.Position));
            }
            
            // Check XY plane (top-bottom) symmetry
            scores[SymmetryPlane.XY] = CalculatePlaneSymmetry(blockPositions, center, Vector3.UnitZ);
            
            // Check XZ plane (left-right) symmetry  
            scores[SymmetryPlane.XZ] = CalculatePlaneSymmetry(blockPositions, center, Vector3.UnitY);
            
            // Check YZ plane (front-back) symmetry
            scores[SymmetryPlane.YZ] = CalculatePlaneSymmetry(blockPositions, center, Vector3.UnitX);
            
            return scores;
        }

        private float CalculatePlaneSymmetry(HashSet<Vector3> positions, Vector3 center, Vector3 normal)
        {
            var matchCount = 0;
            var totalCount = 0;
            
            foreach (var pos in positions)
            {
                var relativePos = pos - center;
                var distance = Vector3.Dot(relativePos, normal);
                
                if (Math.Abs(distance) < 0.1f) // On the plane
                    continue;
                    
                // Mirror position across plane
                var mirroredPos = pos - 2 * distance * normal;
                
                // Check if mirrored position has a block (with tolerance)
                var hasMatch = positions.Any(p => Vector3.DistanceSquared(p, mirroredPos) < 1f);
                
                if (hasMatch) matchCount++;
                totalCount++;
            }
            
            return totalCount > 0 ? (float)matchCount / totalCount : 0f;
        }
    }
}