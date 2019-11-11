using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionGather : TileAction
{
    public float GatherTime = 0.0f;

    public TileActionGather() : base("Gather")
    {

    }

    public override void Run(Entity entity)
    {
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;



        List<EnvironmentTile> route = Map.SolveNeighbour(entity.CurrentPosition, tile);
        if(route==null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoGather(entity));
        }
        else if(route.Count>0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndGather(entity, route));
        }
    }

    private IEnumerator DoWalkAndGather(Entity entity, List<EnvironmentTile> route)
    {
        yield return TileActionWalk.DoGoTo(entity, 0.5f, route);
        yield return DoGather(entity);
    }

    private IEnumerator DoGather(Entity entity)
    {
        yield return new WaitForSeconds(GatherTime);
        Debug.Log("Time To Gather");
        if (!entity.AddToInventory(new Item("Test")))
        {
            // Drop item on ground
        }
        yield return null;
    }

}
