using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionSpawnEntity : TileActionCollection
{

    [SerializeField] private Entity entityPrefab = null;
    
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        yield return base.DoCollection(entity, tile);

        Entity ent = GameObject.Instantiate(entityPrefab, Environment.instance.transform);
        Environment.instance.RegisterEntity(ent);

        ent.transform.position = tile.Position;
        ent.transform.rotation = Quaternion.identity;
        ent.CurrentPosition = tile;
    }

    public override bool Valid(Entity entity)
    {
        return environmentTile.Type == EnvironmentTile.TileType.Accessible && base.Valid(entity);
    }

}