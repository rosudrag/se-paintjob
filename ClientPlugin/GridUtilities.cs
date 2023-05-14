using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace ClientPlugin
{
    public static class GridUtilities
    {
        public static IMyCubeGrid GetGridInFrontOfPlayer(double distance = 10)
        {
            // Get the local player's controlled entity
            var controlledEntity = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;

            // Calculate the target position in front of the controlled entity
            var forwardVector = controlledEntity.WorldMatrix.Forward;
            var targetPosition = controlledEntity.GetPosition() + forwardVector * distance;

            // Get the grid at the target position
            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, entity => entity is IMyCubeGrid && entity.WorldAABB.Contains(targetPosition) != ContainmentType.Disjoint);

            return entities.FirstOrDefault() as IMyCubeGrid;
        }
    }
}