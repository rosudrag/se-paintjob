using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace PaintJob.App.Extensions
{
    public static class GridUtilities
    {
        public static IMyCubeGrid GetGridInFrontOfPlayer(double distance = 100)
        {
            var controlledEntity = MyAPIGateway.Session.Player.Controller.ControlledEntity.Entity;
            var forwardVector = controlledEntity.WorldMatrix.Forward;
            var startPosition = controlledEntity.GetPosition();
            var endPosition = startPosition + forwardVector * distance;

            var raycastHit = MyAPIGateway.Physics.CastRay(startPosition, endPosition, out var hitInfo);
            if (raycastHit && hitInfo.HitEntity is IMyCubeGrid grid)
            {
                return grid;
            }

            return null;
        }

        public static bool IsExteriorBlock(MySlimBlock block, MyCubeGrid grid)
        {
            var directions = new[]
            {
                Vector3D.Up, Vector3D.Down, Vector3D.Forward, Vector3D.Backward, Vector3D.Right, Vector3D.Left
            };

            var blockWorldPosition = grid.GridIntegerToWorld(block.Position);

            foreach (var direction in directions)
            {
                var endPoint = blockWorldPosition + direction * grid.GridSize * grid.Max;
                var ray = new Ray(blockWorldPosition, direction);

                var raycastHit = MyAPIGateway.Physics.CastRay(blockWorldPosition, endPoint, out var hitInfo);

                if (raycastHit == false || hitInfo.HitEntity != grid)
                {
                    // If the raycast doesn't hit any part of the same grid, it's an exterior block
                    return true;
                }
            }

            return false; // If all raycasts hit a part of the same grid, it's not an exterior block
        }

        public static bool IsEdgeBlock(MySlimBlock block, MyCubeGrid grid)
        {
            Vector3I[] directions =
            {
                Vector3I.Up, Vector3I.Down, Vector3I.Left, Vector3I.Right, Vector3I.Forward, Vector3I.Backward
            };

            var countEmptyFaces = 0;

            foreach (var direction in directions)
            {
                var adjacentPosition = block.Position + direction;
                var adjacentBlock = grid.GetCubeBlock(adjacentPosition);

                if (adjacentBlock == null)
                {
                    countEmptyFaces++;
                }
            }

            return countEmptyFaces >= 2;
        }
    }
}