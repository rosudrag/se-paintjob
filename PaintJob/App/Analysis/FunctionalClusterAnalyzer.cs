using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace PaintJob.App.Analysis
{
    public class FunctionalClusterAnalyzer
    {
        public class FunctionalCluster
        {
            public int Id { get; set; }
            public FunctionalSystemType SystemType { get; set; }
            public HashSet<MySlimBlock> Blocks { get; set; } = new HashSet<MySlimBlock>();
            public Vector3 Center { get; set; }
            public float Importance { get; set; } // 0-1 scale
            public bool IsPrimary { get; set; }
            public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        }

        public enum FunctionalSystemType
        {
            Unknown,
            // Power Systems
            PowerGeneration,    // Reactors, solar panels, wind turbines
            PowerStorage,       // Batteries
            
            // Propulsion
            MainThrusters,      // Primary movement thrusters
            ManeuveringThrusters, // RCS/maneuvering thrusters
            AtmosphericThrusters,
            HydrogenThrusters,
            IonThrusters,
            Gyroscopes,
            
            // Weapons
            OffensiveWeapons,   // Turrets, missile launchers, fixed weapons
            DefensiveWeapons,   // Point defense turrets
            
            // Storage
            CargoStorage,       // Cargo containers
            FluidStorage,       // Tanks (oxygen, hydrogen)
            
            // Production
            Manufacturing,      // Assemblers, refineries
            Medical,           // Medical rooms, survival kits
            
            // Control
            CommandAndControl,  // Cockpits, control seats, remote control
            Communication,      // Antennas, beacons
            Sensors,           // Sensors, cameras
            
            // Utility
            Conveyor,          // Conveyor system
            Airlock,           // Doors, airlocks
            Lighting,          // Lights
            LandingGear,       // Landing gear, connectors, merge blocks
            Armor,             // Heavy armor concentrations
            Decorative         // LCD panels, decorative blocks
        }

        public class ClusterAnalysis
        {
            public List<FunctionalCluster> Clusters { get; set; } = new List<FunctionalCluster>();
            public Dictionary<FunctionalSystemType, List<FunctionalCluster>> SystemClusters { get; set; } = new Dictionary<FunctionalSystemType, List<FunctionalCluster>>();
            public Dictionary<MySlimBlock, int> BlockToCluster { get; set; } = new Dictionary<MySlimBlock, int>();
        }

        private const float CLUSTER_DISTANCE_THRESHOLD = 10f; // Grid units

        public ClusterAnalysis AnalyzeGrid(MyCubeGrid grid)
        {
            var analysis = new ClusterAnalysis();
            var blocks = grid.GetBlocks();
            
            // Categorize blocks by function
            var categorizedBlocks = CategorizeBlocks(blocks);
            
            // Create clusters for each system type
            foreach (var category in categorizedBlocks)
            {
                var systemType = category.Key;
                var systemBlocks = category.Value;
                
                if (!systemBlocks.Any())
                    continue;
                
                var clusters = CreateClustersForSystem(grid, systemType, systemBlocks);
                
                foreach (var cluster in clusters)
                {
                    analysis.Clusters.Add(cluster);
                    
                    // Map blocks to clusters
                    foreach (var block in cluster.Blocks)
                    {
                        analysis.BlockToCluster[block] = cluster.Id;
                    }
                }
                
                analysis.SystemClusters[systemType] = clusters;
            }
            
            // Analyze cluster importance
            AnalyzeClusterImportance(analysis);
            
            return analysis;
        }

        private Dictionary<FunctionalSystemType, List<MySlimBlock>> CategorizeBlocks(HashSet<MySlimBlock> blocks)
        {
            var categories = new Dictionary<FunctionalSystemType, List<MySlimBlock>>();
            
            // Initialize all categories
            foreach (FunctionalSystemType systemType in Enum.GetValues(typeof(FunctionalSystemType)))
            {
                categories[systemType] = new List<MySlimBlock>();
            }
            
            foreach (var block in blocks)
            {
                var systemType = DetermineBlockSystem(block);
                categories[systemType].Add(block);
            }
            
            return categories;
        }

        private FunctionalSystemType DetermineBlockSystem(MySlimBlock block)
        {
            if (block.FatBlock == null)
            {
                // Non-functional blocks are likely armor
                if (block.BlockDefinition.Id.SubtypeName.Contains("Armor"))
                {
                    return FunctionalSystemType.Armor;
                }
                return FunctionalSystemType.Decorative;
            }
            
            var fatBlock = block.FatBlock;
            var typeName = block.BlockDefinition.Id.TypeId.ToString();
            var subtypeName = block.BlockDefinition.Id.SubtypeName;
            
            // Power Systems
            if (typeName.Contains("Reactor") || subtypeName.Contains("Reactor"))
                return FunctionalSystemType.PowerGeneration;
            if (typeName.Contains("SolarPanel") || subtypeName.Contains("Solar"))
                return FunctionalSystemType.PowerGeneration;
            if (typeName.Contains("WindTurbine"))
                return FunctionalSystemType.PowerGeneration;
            if (typeName.Contains("Battery"))
                return FunctionalSystemType.PowerStorage;
            
            // Propulsion
            if (typeName.Contains("Thrust"))
            {
                if (subtypeName.Contains("Atmospheric"))
                    return FunctionalSystemType.AtmosphericThrusters;
                else if (subtypeName.Contains("Hydrogen"))
                    return FunctionalSystemType.HydrogenThrusters;
                else if (subtypeName.Contains("Ion"))
                    return FunctionalSystemType.IonThrusters;
                else if (subtypeName.Contains("Small") || subtypeName.Contains("RCS"))
                    return FunctionalSystemType.ManeuveringThrusters;
                else
                    return FunctionalSystemType.MainThrusters;
            }
            if (typeName.Contains("Gyro"))
                return FunctionalSystemType.Gyroscopes;
            
            // Weapons
            if (typeName.Contains("Turret") || typeName.Contains("GatlingGun") || 
                typeName.Contains("MissileLauncher") || typeName.Contains("RocketLauncher"))
            {
                if (subtypeName.Contains("Interior") || subtypeName.Contains("Point"))
                    return FunctionalSystemType.DefensiveWeapons;
                else
                    return FunctionalSystemType.OffensiveWeapons;
            }
            
            // Storage
            if (typeName.Contains("CargoContainer"))
                return FunctionalSystemType.CargoStorage;
            if (typeName.Contains("OxygenTank") || typeName.Contains("HydrogenTank"))
                return FunctionalSystemType.FluidStorage;
            
            // Production
            if (typeName.Contains("Assembler") || typeName.Contains("Refinery") || typeName.Contains("Arc"))
                return FunctionalSystemType.Manufacturing;
            if (typeName.Contains("MedicalRoom") || typeName.Contains("SurvivalKit") || typeName.Contains("CryoChamber"))
                return FunctionalSystemType.Medical;
            
            // Control
            if (typeName.Contains("Cockpit") || typeName.Contains("RemoteControl") || 
                typeName.Contains("ControlPanel") || typeName.Contains("ButtonPanel"))
                return FunctionalSystemType.CommandAndControl;
            if (typeName.Contains("Antenna") || typeName.Contains("LaserAntenna") || typeName.Contains("Beacon"))
                return FunctionalSystemType.Communication;
            if (typeName.Contains("Sensor") || typeName.Contains("Camera"))
                return FunctionalSystemType.Sensors;
            
            // Utility
            if (typeName.Contains("Conveyor") || typeName.Contains("Connector") || typeName.Contains("Ejector"))
                return FunctionalSystemType.Conveyor;
            if (typeName.Contains("Door") || typeName.Contains("AirtightHangarDoor"))
                return FunctionalSystemType.Airlock;
            if (typeName.Contains("Light") || subtypeName.Contains("Light"))
                return FunctionalSystemType.Lighting;
            if (typeName.Contains("LandingGear") || typeName.Contains("Connector") || typeName.Contains("MergeBlock"))
                return FunctionalSystemType.LandingGear;
            if (typeName.Contains("TextPanel") || typeName.Contains("LCD"))
                return FunctionalSystemType.Decorative;
            
            return FunctionalSystemType.Unknown;
        }

        private List<FunctionalCluster> CreateClustersForSystem(
            MyCubeGrid grid,
            FunctionalSystemType systemType,
            List<MySlimBlock> blocks)
        {
            var clusters = new List<FunctionalCluster>();
            var unassigned = new HashSet<MySlimBlock>(blocks);
            var clusterId = 0;
            
            while (unassigned.Any())
            {
                var cluster = new FunctionalCluster
                {
                    Id = clusterId++,
                    SystemType = systemType
                };
                
                // Start with first unassigned block
                var seedBlock = unassigned.First();
                var queue = new Queue<MySlimBlock>();
                queue.Enqueue(seedBlock);
                unassigned.Remove(seedBlock);
                
                // Grow cluster by proximity
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    cluster.Blocks.Add(current);
                    
                    var currentCenter = GetBlockCenter(grid, current);
                    
                    // Find nearby blocks of same type
                    var nearbyBlocks = unassigned.Where(b =>
                    {
                        var blockCenter = GetBlockCenter(grid, b);
                        var distance = Vector3.Distance(currentCenter, blockCenter);
                        return distance <= CLUSTER_DISTANCE_THRESHOLD * grid.GridSize;
                    }).ToList();
                    
                    foreach (var nearby in nearbyBlocks)
                    {
                        queue.Enqueue(nearby);
                        unassigned.Remove(nearby);
                    }
                }
                
                if (cluster.Blocks.Count > 0)
                {
                    // Calculate cluster center
                    var centerSum = Vector3.Zero;
                    foreach (var block in cluster.Blocks)
                    {
                        centerSum += GetBlockCenter(grid, block);
                    }
                    cluster.Center = centerSum / cluster.Blocks.Count;
                    
                    clusters.Add(cluster);
                }
            }
            
            return clusters;
        }

        private Vector3 GetBlockCenter(MyCubeGrid grid, MySlimBlock block)
        {
            return (grid.GridIntegerToWorld(block.Min) + grid.GridIntegerToWorld(block.Max)) / 2f;
        }

        private void AnalyzeClusterImportance(ClusterAnalysis analysis)
        {
            // Determine primary clusters for each system
            foreach (var systemClusters in analysis.SystemClusters)
            {
                var clusters = systemClusters.Value;
                if (!clusters.Any())
                    continue;
                
                // Find largest cluster as primary
                var primaryCluster = clusters.OrderByDescending(c => c.Blocks.Count).First();
                primaryCluster.IsPrimary = true;
                
                // Calculate importance based on size and system type
                foreach (var cluster in clusters)
                {
                    var sizeRatio = (float)cluster.Blocks.Count / primaryCluster.Blocks.Count;
                    var baseImportance = GetSystemBaseImportance(cluster.SystemType);
                    
                    cluster.Importance = baseImportance * (cluster.IsPrimary ? 1.0f : sizeRatio * 0.7f);
                    
                    // Add metadata
                    cluster.Metadata["BlockCount"] = cluster.Blocks.Count;
                    cluster.Metadata["TotalPower"] = CalculateClusterPower(cluster);
                    
                    if (cluster.SystemType == FunctionalSystemType.MainThrusters ||
                        cluster.SystemType == FunctionalSystemType.ManeuveringThrusters)
                    {
                        cluster.Metadata["ThrustDirection"] = CalculateThrustDirection(cluster);
                    }
                }
            }
        }

        private float GetSystemBaseImportance(FunctionalSystemType systemType)
        {
            switch (systemType)
            {
                case FunctionalSystemType.CommandAndControl:
                case FunctionalSystemType.PowerGeneration:
                    return 1.0f;
                    
                case FunctionalSystemType.MainThrusters:
                case FunctionalSystemType.OffensiveWeapons:
                case FunctionalSystemType.PowerStorage:
                    return 0.9f;
                    
                case FunctionalSystemType.Manufacturing:
                case FunctionalSystemType.Medical:
                case FunctionalSystemType.Communication:
                    return 0.8f;
                    
                case FunctionalSystemType.ManeuveringThrusters:
                case FunctionalSystemType.Gyroscopes:
                case FunctionalSystemType.DefensiveWeapons:
                    return 0.7f;
                    
                case FunctionalSystemType.CargoStorage:
                case FunctionalSystemType.FluidStorage:
                case FunctionalSystemType.Sensors:
                    return 0.6f;
                    
                case FunctionalSystemType.Conveyor:
                case FunctionalSystemType.Airlock:
                    return 0.5f;
                    
                case FunctionalSystemType.LandingGear:
                case FunctionalSystemType.Armor:
                    return 0.4f;
                    
                case FunctionalSystemType.Lighting:
                case FunctionalSystemType.Decorative:
                    return 0.3f;
                    
                default:
                    return 0.5f;
            }
        }

        private float CalculateClusterPower(FunctionalCluster cluster)
        {
            var totalPower = 0f;
            
            foreach (var block in cluster.Blocks)
            {
                if (block.FatBlock == null)
                    continue;
                
                // This is simplified - in reality you'd check actual power consumption/generation
                var mass = block.BlockDefinition.Mass;
                totalPower += mass; // Using mass as proxy for power
            }
            
            return totalPower;
        }

        private Vector3 CalculateThrustDirection(FunctionalCluster cluster)
        {
            var avgDirection = Vector3.Zero;
            var count = 0;
            
            foreach (var block in cluster.Blocks)
            {
                if (block.FatBlock == null)
                    continue;
                
                // Get thruster orientation
                var forward = block.FatBlock.WorldMatrix.Forward;
                avgDirection += forward;
                count++;
            }
            
            if (count > 0)
            {
                avgDirection /= count;
                avgDirection.Normalize();
            }
            
            return avgDirection;
        }

        public List<FunctionalCluster> GetClustersByImportance(ClusterAnalysis analysis, float minImportance = 0.5f)
        {
            return analysis.Clusters
                .Where(c => c.Importance >= minImportance)
                .OrderByDescending(c => c.Importance)
                .ToList();
        }

        public Dictionary<FunctionalSystemType, Vector3> GetSystemCenters(ClusterAnalysis analysis)
        {
            var centers = new Dictionary<FunctionalSystemType, Vector3>();
            
            foreach (var systemClusters in analysis.SystemClusters)
            {
                var primaryCluster = systemClusters.Value.FirstOrDefault(c => c.IsPrimary);
                if (primaryCluster != null)
                {
                    centers[systemClusters.Key] = primaryCluster.Center;
                }
            }
            
            return centers;
        }
    }
}