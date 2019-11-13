using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionGather : TileAction
{
    public float GatherTime = 0.0f;

    public EnvironmentTile[] replacmentTile;

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
            entity.StartCoroutine(DoGather(entity, tile));
        }
        else if(route.Count>0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndGather(entity, route, tile));
        }
    }

    private IEnumerator DoWalkAndGather(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, 0.5f, route);
        yield return DoGather(entity, tile);
    }

    public IEnumerator DoGather(Entity entity, EnvironmentTile tile)
    {
        yield return new WaitForSeconds(GatherTime);
        Debug.Log("Time To Gather");
        GameObject newObject = Map.ReplaceEnviromentTile(tile, replacmentTile[Random.Range(0, replacmentTile.Length)]);

        if (!entity.AddToInventory(new Item("Test")))
        {
            // Drop item on ground
        }

        if(newObject.GetComponent<TileActionGather>())
        {
            TileActionGather newGatherAction = newObject.GetComponent<TileActionGather>();
            yield return newGatherAction.DoGather(entity, newObject.GetComponent<EnvironmentTile>());
        }

    }

}
