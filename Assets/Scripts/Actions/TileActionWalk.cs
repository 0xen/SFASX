using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionWalk : TileAction
{ 
    public TileActionWalk() : base("Walk")
    {

    }

    // On the action run
    public override void Run(Entity entity)
    {
        // If the environment tile is null, then somehow the state was accessed, break
        if (environmentTile == null) return;

        // Calculate the route to the target tile
        List<EnvironmentTile> route = Environment.instance.Solve(entity.CurrentPosition, environmentTile);

        entity.StopAllCoroutines();
        // Start the entity movement
        entity.StartCoroutine(DoGoToLocal(entity, entity.GetMovmentSpeed(), route));
    }

    // Preform a move action from one position to another
    private static IEnumerator DoMove(Entity entity, float entityMovmentPerSeccond, Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            // Rotate the entity towards the tile
            entity.transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = entity.transform.position;
            float distance = (destination - p).magnitude;
            float distanceMoved = 0.0f;

            // Loop until we have moved to the new tile
            while (distanceMoved < distance)
            {
                distanceMoved += entityMovmentPerSeccond * Time.deltaTime;
                p = Vector3.Lerp(position, destination, distanceMoved / distance);
                entity.transform.position = p;
                yield return null;
            }
        }
    }

    // Called on a local move, move between the tiles
    private IEnumerator DoGoToLocal(Entity entity, float NodeMoveTime, List<EnvironmentTile> route)
    {
        yield return DoGoTo(entity, NodeMoveTime, route);
        entity.ResetAction();
    }

    // Move between the tiles
    public static IEnumerator DoGoTo(Entity entity, float NodeMoveTime, List<EnvironmentTile> route)
    {
        entity.ChangeAnimation(AnimationStates.Walking);
        // Move through each tile in the given route
        if (route != null)
        {
            // Start at the current position of the entity to stop jittering in the moment
            Vector3 position = entity.transform.position;
            for (int count = 0; count < route.Count; ++count)
            {
                // Get the next position
                Vector3 next = route[count].Position;
                next.y = position.y;
                // Preform the move based on the last and next positions
                yield return DoMove(entity, NodeMoveTime, position, next);
                entity.CurrentPosition = route[count];
                position = next;
            }
        }
        // Return the entity to a idle state
        entity.ChangeAnimation(AnimationStates.Idle);
    }

    // Since the walk action con only exist on empty tiles, it will always be a valid action
    public override bool Valid(Entity entity)
    {
        return true;
    }
}
