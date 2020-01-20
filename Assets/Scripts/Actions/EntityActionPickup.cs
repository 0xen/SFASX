using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityActionPickup : TileActionCollection
{
    [SerializeField] private Entity GatherEntity = null;
    
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        yield return base.DoCollection(entity, tile);
        Environment.instance.RemoveEntity(GatherEntity);
        Destroy(GatherEntity.gameObject);
    }

    public override bool Valid(Entity entity)
    {
        return true;
    }

}