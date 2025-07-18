using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class SpatialOrientationAnalyzer
    {
        public class OrientationAnalysis
        {
            public Vector3 ForwardDirection { get; set; }
            public Vector3 UpDirection { get; set; }
            public Vector3 RightDirection { get; set; }
            
            public Vector3 Bow { get; set; }          // Front point
            public Vector3 Stern { get; set; }        // Rear point
            public Vector3 Port { get; set; }         // Left side center
            public Vector3 Starboard { get; set; }    // Right side center
            public Vector3 Dorsal { get; set; }       // Top center
            public Vector3 Ventral { get; set; }      // Bottom center
            
            public float Length { get; set; }         // Forward/Aft dimension
            public float Beam { get; set; }           // Port/Starboard dimension  
            public float Height { get; set; }         // Dorsal/Ventral dimension
            
            public Dictionary<Vector3I, RelativePosition> BlockPositions { get; set; } = new Dictionary<Vector3I, RelativePosition>();
        }

        public class RelativePosition
        {
            public float ForwardDistance { get; set; }    // + is forward, - is aft
            public float LateralDistance { get; set; }    // + is starboard, - is port
            public float VerticalDistance { get; set; }   // + is dorsal, - is ventral
            public ShipRegion Region { get; set; }
            public float RelativeForward { get; set; }    // 0-1, 0 is stern, 1 is bow
            public float RelativeLateral { get; set; }    // 0-1, 0 is port, 1 is starboard
            public float RelativeVertical { get; set; }   // 0-1, 0 is ventral, 1 is dorsal
        }

        public enum ShipRegion
        {
            // Primary regions
            Bow,            // Front
            Stern,          // Rear
            Amidships,      // Middle
            
            // Vertical regions
            Dorsal,         // Top
            Ventral,        // Bottom
            Midline,        // Middle height
            
            // Lateral regions
            Port,           // Left
            Starboard,      // Right
            Centerline,     // Center
            
            // Combined regions
            BowPort,
            BowStarboard,
            BowDorsal,
            BowVentral,
            SternPort,
            SternStarboard,
            SternDorsal,
            SternVentral,
            AmidshipsPort,
            AmidshipsStarboard,
            AmidshipsDorsal,
            AmidshipsVentral
        }

        private readonly FunctionalClusterAnalyzer _functionalAnalyzer = new FunctionalClusterAnalyzer();

        public OrientationAnalysis AnalyzeGrid(MyCubeGrid grid)
        {
            var analysis = new OrientationAnalysis();
            
            // Get functional clusters to help determine orientation
            var functionalAnalysis = _functionalAnalyzer.AnalyzeGrid(grid);
            
            // Determine ship orientation based on functional blocks
            DetermineOrientation(grid, functionalAnalysis, analysis);
            
            // Calculate ship dimensions and key points
            CalculateShipDimensions(grid, analysis);
            
            // Analyze all block positions relative to orientation
            AnalyzeBlockPositions(grid, analysis);
            
            return analysis;
        }

        private void DetermineOrientation(
            MyCubeGrid grid, 
            FunctionalClusterAnalyzer.ClusterAnalysis functionalAnalysis,
            OrientationAnalysis analysis)
        {
            // Try to determine forward direction from cockpits/control seats
            var commandClusters = functionalAnalysis.SystemClusters
                .Where(kvp => kvp.Key == FunctionalClusterAnalyzer.FunctionalSystemType.CommandAndControl)
                .SelectMany(kvp => kvp.Value)
                .ToList();
            
            if (commandClusters.Any())
            {
                // Use primary command cluster orientation
                var primaryCommand = commandClusters.FirstOrDefault(c => c.IsPrimary) ?? commandClusters.First();
                var cockpitBlock = primaryCommand.Blocks.FirstOrDefault(b => 
                    b.BlockDefinition.Id.TypeId.ToString().Contains("Cockpit"));
                
                if (cockpitBlock?.FatBlock != null)
                {
                    // Cockpit forward is ship forward
                    analysis.ForwardDirection = cockpitBlock.FatBlock.WorldMatrix.Forward;
                    analysis.UpDirection = cockpitBlock.FatBlock.WorldMatrix.Up;
                    analysis.RightDirection = cockpitBlock.FatBlock.WorldMatrix.Right;
                    return;
                }
            }
            
            // Fallback: Determine from main thrusters
            var thrusterClusters = functionalAnalysis.SystemClusters
                .Where(kvp => kvp.Key == FunctionalClusterAnalyzer.FunctionalSystemType.MainThrusters)
                .SelectMany(kvp => kvp.Value)
                .ToList();
            
            if (thrusterClusters.Any())
            {
                // Main thrusters usually point backward
                var mainThruster = thrusterClusters.OrderByDescending(c => c.Blocks.Count).First();
                if (mainThruster.Metadata.TryGetValue("ThrustDirection", out var thrustDir) && thrustDir is Vector3 thrust)
                {
                    analysis.ForwardDirection = -thrust; // Opposite of thrust
                    
                    // Guess up direction (perpendicular to forward)
                    if (Math.Abs(Vector3.Dot(analysis.ForwardDirection, Vector3.Up)) < 0.9f)
                    {
                        analysis.UpDirection = Vector3.Up;
                    }
                    else
                    {
                        analysis.UpDirection = Vector3.Forward;
                    }
                    
                    analysis.RightDirection = Vector3.Cross(analysis.ForwardDirection, analysis.UpDirection);
                    analysis.UpDirection = Vector3.Cross(analysis.RightDirection, analysis.ForwardDirection);
                    return;
                }
            }
            
            // Final fallback: Use grid's longest axis as forward
            var gridSize = new Vector3(
                grid.Max.X - grid.Min.X,
                grid.Max.Y - grid.Min.Y,
                grid.Max.Z - grid.Min.Z
            );
            var size = gridSize;
            
            if (size.X >= size.Y && size.X >= size.Z)
            {
                analysis.ForwardDirection = Vector3.Right;
                analysis.UpDirection = Vector3.Up;
            }
            else if (size.Y >= size.X && size.Y >= size.Z)
            {
                analysis.ForwardDirection = Vector3.Up;
                analysis.UpDirection = Vector3.Forward;
            }
            else
            {
                analysis.ForwardDirection = Vector3.Forward;
                analysis.UpDirection = Vector3.Up;
            }
            
            analysis.RightDirection = Vector3.Cross(analysis.ForwardDirection, analysis.UpDirection);
        }

        private void CalculateShipDimensions(MyCubeGrid grid, OrientationAnalysis analysis)
        {
            var blocks = grid.GetBlocks();
            if (!blocks.Any())
                return;
            
            // Project all block positions onto ship axes
            var minForward = float.MaxValue;
            var maxForward = float.MinValue;
            var minLateral = float.MaxValue;
            var maxLateral = float.MinValue;
            var minVertical = float.MaxValue;
            var maxVertical = float.MinValue;
            
            foreach (var block in blocks)
            {
                // Get all positions occupied by this block
                for (var x = block.Min.X; x <= block.Max.X; x++)
                {
                    for (var y = block.Min.Y; y <= block.Max.Y; y++)
                    {
                        for (var z = block.Min.Z; z <= block.Max.Z; z++)
                        {
                            var worldPos = grid.GridIntegerToWorld(new Vector3I(x, y, z));
                            
                            var forward = Vector3.Dot(worldPos, analysis.ForwardDirection);
                            var lateral = Vector3.Dot(worldPos, analysis.RightDirection);
                            var vertical = Vector3.Dot(worldPos, analysis.UpDirection);
                            
                            minForward = Math.Min(minForward, forward);
                            maxForward = Math.Max(maxForward, forward);
                            minLateral = Math.Min(minLateral, lateral);
                            maxLateral = Math.Max(maxLateral, lateral);
                            minVertical = Math.Min(minVertical, vertical);
                            maxVertical = Math.Max(maxVertical, vertical);
                        }
                    }
                }
            }
            
            // Calculate dimensions
            analysis.Length = maxForward - minForward;
            analysis.Beam = maxLateral - minLateral;
            analysis.Height = maxVertical - minVertical;
            
            // Calculate key points
            var center = (minForward + maxForward) / 2f * analysis.ForwardDirection +
                        (minLateral + maxLateral) / 2f * analysis.RightDirection +
                        (minVertical + maxVertical) / 2f * analysis.UpDirection;
            
            analysis.Bow = center + (maxForward - (minForward + maxForward) / 2f) * analysis.ForwardDirection;
            analysis.Stern = center + (minForward - (minForward + maxForward) / 2f) * analysis.ForwardDirection;
            analysis.Starboard = center + (maxLateral - (minLateral + maxLateral) / 2f) * analysis.RightDirection;
            analysis.Port = center + (minLateral - (minLateral + maxLateral) / 2f) * analysis.RightDirection;
            analysis.Dorsal = center + (maxVertical - (minVertical + maxVertical) / 2f) * analysis.UpDirection;
            analysis.Ventral = center + (minVertical - (minVertical + maxVertical) / 2f) * analysis.UpDirection;
        }

        private void AnalyzeBlockPositions(MyCubeGrid grid, OrientationAnalysis analysis)
        {
            var blocks = grid.GetBlocks();
            
            foreach (var block in blocks)
            {
                var blockCenter = (grid.GridIntegerToWorld(block.Min) + grid.GridIntegerToWorld(block.Max)) / 2f;
                
                // Calculate distances along each axis
                var forwardDist = Vector3.Dot(blockCenter - analysis.Stern, analysis.ForwardDirection);
                var lateralDist = Vector3.Dot(blockCenter - analysis.Port, analysis.RightDirection);
                var verticalDist = Vector3.Dot(blockCenter - analysis.Ventral, analysis.UpDirection);
                
                // Calculate relative positions (0-1)
                var relForward = analysis.Length > 0 ? forwardDist / analysis.Length : 0.5f;
                var relLateral = analysis.Beam > 0 ? lateralDist / analysis.Beam : 0.5f;
                var relVertical = analysis.Height > 0 ? verticalDist / analysis.Height : 0.5f;
                
                // Determine region
                var region = DetermineRegion(relForward, relLateral, relVertical);
                
                var relPos = new RelativePosition
                {
                    ForwardDistance = forwardDist - analysis.Length / 2f,
                    LateralDistance = lateralDist - analysis.Beam / 2f,
                    VerticalDistance = verticalDist - analysis.Height / 2f,
                    RelativeForward = relForward,
                    RelativeLateral = relLateral,
                    RelativeVertical = relVertical,
                    Region = region
                };
                
                // Store for the primary position of the block
                analysis.BlockPositions[block.Position] = relPos;
            }
        }

        private ShipRegion DetermineRegion(float relForward, float relLateral, float relVertical)
        {
            // Determine primary forward/aft region
            string forwardRegion;
            if (relForward < 0.33f)
                forwardRegion = "Stern";
            else if (relForward > 0.67f)
                forwardRegion = "Bow";
            else
                forwardRegion = "Amidships";
            
            // Determine lateral region
            string lateralRegion;
            if (relLateral < 0.33f)
                lateralRegion = "Port";
            else if (relLateral > 0.67f)
                lateralRegion = "Starboard";
            else
                lateralRegion = "";
            
            // Determine vertical region
            string verticalRegion;
            if (relVertical < 0.33f)
                verticalRegion = "Ventral";
            else if (relVertical > 0.67f)
                verticalRegion = "Dorsal";
            else
                verticalRegion = "";
            
            // Combine regions
            var combinedName = forwardRegion + lateralRegion + verticalRegion;
            
            // Try to parse combined name
            if (Enum.TryParse<ShipRegion>(combinedName, out var region))
                return region;
            
            // Fallback to primary region
            if (Enum.TryParse<ShipRegion>(forwardRegion, out region))
                return region;
            
            return ShipRegion.Amidships;
        }

        public Dictionary<ShipRegion, List<MySlimBlock>> GetBlocksByRegion(
            MyCubeGrid grid, 
            OrientationAnalysis analysis)
        {
            var regionBlocks = new Dictionary<ShipRegion, List<MySlimBlock>>();
            
            // Initialize all regions
            foreach (ShipRegion region in Enum.GetValues(typeof(ShipRegion)))
            {
                regionBlocks[region] = new List<MySlimBlock>();
            }
            
            var blocks = grid.GetBlocks();
            foreach (var block in blocks)
            {
                if (analysis.BlockPositions.TryGetValue(block.Position, out var relPos))
                {
                    regionBlocks[relPos.Region].Add(block);
                }
            }
            
            return regionBlocks;
        }

        public bool IsPortSide(MySlimBlock block, OrientationAnalysis analysis)
        {
            if (analysis.BlockPositions.TryGetValue(block.Position, out var relPos))
            {
                return relPos.LateralDistance < 0;
            }
            return false;
        }

        public bool IsStarboardSide(MySlimBlock block, OrientationAnalysis analysis)
        {
            if (analysis.BlockPositions.TryGetValue(block.Position, out var relPos))
            {
                return relPos.LateralDistance > 0;
            }
            return false;
        }

        public bool IsForward(MySlimBlock block, OrientationAnalysis analysis, float threshold = 0.5f)
        {
            if (analysis.BlockPositions.TryGetValue(block.Position, out var relPos))
            {
                return relPos.RelativeForward > threshold;
            }
            return false;
        }

        public bool IsAft(MySlimBlock block, OrientationAnalysis analysis, float threshold = 0.5f)
        {
            if (analysis.BlockPositions.TryGetValue(block.Position, out var relPos))
            {
                return relPos.RelativeForward < (1f - threshold);
            }
            return false;
        }
    }
}