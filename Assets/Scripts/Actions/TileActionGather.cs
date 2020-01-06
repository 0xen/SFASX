using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionGather : TileAction
{
    public float GatherTime = 0.0f;

    public EnvironmentTile[] replacmentTile;

    [System.Serializable]
    public struct Pickup
    {
        public Item item;
        public uint count;
    }

    public Pickup[] pickups;

    public TileActionGather() : base("Gather")
    {

    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoGather(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndGather(entity, route, environmentTile));
        }
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndGather(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoGather(entity, tile);
    }

    public IEnumerator DoGather(Entity entity, EnvironmentTile tile)
    {
        // Turn towards the tile
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);


        yield return new WaitForSeconds(GatherTime);
        GameObject newObject = Environment.instance.ReplaceEnviromentTile(tile, replacmentTile[Random.Range(0, replacmentTile.Length)]);


        foreach(Pickup pickup in pickups)
        {
            if (!entity.AddToInventory(pickup.item, pickup.count))
            {
                // Drop item on ground
            }
        }
        
        if(newObject.GetComponent<TileActionGather>())
        {
            TileActionGather newGatherAction = newObject.GetComponent<TileActionGather>();
            entity.SetCurrentAction(newGatherAction);
            yield return newGatherAction.DoGather(entity, newObject.GetComponent<EnvironmentTile>());
        }
        else
        {
            entity.ResetAction();
        }

    }

    public override bool Valid(Entity entity)
    {
        return true;
    }

}
