using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityActionPickup : TileActionCollection
{
    [SerializeField] private Entity GatherEntity = null;

    // Override the "TileActionCollection" DoCollection function
    // Remove the entity that this script is attached to and preform all the lower level inventory adding code
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        yield return base.DoCollection(entity, tile);
        Environment.instance.RemoveEntity(GatherEntity);
        Destroy(GatherEntity.gameObject);
    }

    // If this is on a tile, it can be preformed
    public override bool Valid(Entity entity)
    {
        return true;
    }

}