using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionGather : TileAction
{

    public TileActionGather() : base("Gather")
    {

    }

    public override void Run(Entity entity)
    {
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;

        // Sort the connections array so that the closest one to the player will be first
        tile.Connections.Sort(((x, y) => (x.Position - entity.transform.position).magnitude.CompareTo((y.Position - entity.transform.position).magnitude))); 

        // Loop through the connections, if we find one that we can path to, go to it
        foreach (EnvironmentTile childTile in tile.Connections)
        {
            List<EnvironmentTile> route = Map.Solve(entity.CurrentPosition, childTile);
            if(route.Count>0)
            {
                entity.StopAllCoroutines();
                entity.StartCoroutine(DoWalkAndGather(entity, route));
                break;
            }
        }

    }



    private IEnumerator DoWalkAndGather(Entity entity, List<EnvironmentTile> route)
    {
        yield return TileActionWalk.DoGoTo(entity, 0.5f, route);
        Debug.Log("Time To Gather");
    }

}
