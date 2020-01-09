using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityActionPickup : TileAction
{
    public float GatherTime = 0.0f;

    [SerializeField] private Entity GatherEntity = null;

    [SerializeField] private Item EntityItem = null;

    public EntityActionPickup() : base("Pick Up")
    {

    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoPickup(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndPickup(entity, route, environmentTile));
        }
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndPickup(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoPickup(entity, tile);
    }

    public IEnumerator DoPickup(Entity entity, EnvironmentTile tile)
    {
        // Turn towards the tile
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);


        yield return new WaitForSeconds(GatherTime);


        if (!entity.AddToInventory(EntityItem, 1))
        {
            // Drop item on ground
        }

        Environment.instance.RemoveEntity(GatherEntity);
        Destroy(GatherEntity.gameObject);

        entity.ResetAction();
    }

    public override bool Valid(Entity entity)
    {
        return true;
    }

}