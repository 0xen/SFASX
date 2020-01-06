using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionWalk : TileAction
{ 
    public TileActionWalk() : base("Walk")
    {

    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;
        List<EnvironmentTile> route = Environment.instance.Solve(entity.CurrentPosition, environmentTile);

        entity.StopAllCoroutines();
        entity.StartCoroutine(DoGoToLocal(entity, entity.GetMovmentSpeed(), route));
    }

    private static IEnumerator DoMove(Entity entity, float entityMovmentPerSeccond, Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            entity.transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = entity.transform.position;
            float distance = (destination - p).magnitude;
            float distanceMoved = 0.0f;

            while (distanceMoved < distance)
            {
                distanceMoved += entityMovmentPerSeccond * Time.deltaTime;
                p = Vector3.Lerp(position, destination, distanceMoved / distance);
                entity.transform.position = p;
                yield return null;
            }
        }
    }

    private IEnumerator DoGoToLocal(Entity entity, float NodeMoveTime, List<EnvironmentTile> route)
    {
        yield return DoGoTo(entity, NodeMoveTime, route);
        entity.ResetAction();
    }

    public static IEnumerator DoGoTo(Entity entity, float NodeMoveTime, List<EnvironmentTile> route)
    {
        entity.ChangeAnimation(AnimationStates.Walking);
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = entity.CurrentPosition.Position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].Position;
                yield return DoMove(entity, NodeMoveTime, position, next);
                entity.CurrentPosition = route[count];
                position = next;
            }
        }
        entity.ChangeAnimation(AnimationStates.Idle);
    }

    public override bool Valid(Entity entity)
    {
        return true;
    }
}
