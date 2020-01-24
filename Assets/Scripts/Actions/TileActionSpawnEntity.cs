using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionSpawnEntity : TileActionCollection
{
    // The entity that will be spawned
    [SerializeField] private Entity entityPrefab = null;

    // Override the "TileActionCollection" DoCollection function
    // Pause, Remove x items from the entity and spawn the new entity at the tile position
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        yield return base.DoCollection(entity, tile);


        // Instantiate the new entity and move it to its new position
        Entity ent = GameObject.Instantiate(entityPrefab, Environment.instance.transform);
        Environment.instance.RegisterEntity(ent);

        ent.transform.position = tile.Position;
        ent.transform.rotation = Quaternion.identity;
        ent.CurrentPosition = tile;
    }

    // Valid if the tile is empty and the base classes valid states are true
    public override bool Valid(Entity entity)
    {
        return environmentTile.Type == EnvironmentTile.TileType.Accessible && base.Valid(entity);
    }

}