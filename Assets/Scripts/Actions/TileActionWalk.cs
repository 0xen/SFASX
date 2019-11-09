using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionWalk : TileAction
{
    [SerializeField] private float SingleNodeMoveTime = 0.5f;

    public TileActionWalk() : base("Walk")
    {

    }

    public override void Run(Entity entity)
    {
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;
        List<EnvironmentTile> route = Map.Solve(entity.CurrentPosition, tile);
        entity.StopAllCoroutines();
        entity.StartCoroutine(DoGoTo(entity, SingleNodeMoveTime, route));
    }

    private static IEnumerator DoMove(Entity entity, float NodeMoveTime, Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            entity.transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = entity.transform.position;
            float t = 0.0f;

            while (t < NodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / NodeMoveTime);
                entity.transform.position = p;
                yield return null;
            }
        }
    }

    public static IEnumerator DoGoTo(Entity entity, float NodeMoveTime, List<EnvironmentTile> route)
    {
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
    }

}
