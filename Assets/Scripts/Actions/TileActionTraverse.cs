using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionTraverse : TileActionCollection
{

    // Override the "TileActionCollection" DoCollection function
    // Pause, Remove x items from the entity and spawn the new entity at the tile position
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        yield return base.DoCollection(entity, tile);

        Vector2Int targetPosition = tile.PositionTile + (tile.PositionTile - entity.CurrentPosition.PositionTile);
        EnvironmentTile target = Environment.instance.GetTile(targetPosition.x, targetPosition.y);

        if(target!=null && target.Type == EnvironmentTile.TileType.Accessible)
        {
            Vector3 position = target.Position;
            position.y = entity.transform.position.y;
            entity.transform.position = position;
            entity.CurrentPosition = target;
        }
    }
    
    // Make sure there is a pair of opposing tiles that we can jump between
    public override bool Valid(Entity entity)
    {
        Vector2Int targetPosition = environmentTile.PositionTile;
        EnvironmentTile tile1 = Environment.instance.GetTile(targetPosition.x, targetPosition.y + 1);
        EnvironmentTile tile2 = Environment.instance.GetTile(targetPosition.x + 1, targetPosition.y);
        EnvironmentTile tile3 = Environment.instance.GetTile(targetPosition.x, targetPosition.y - 1);
        EnvironmentTile tile4 = Environment.instance.GetTile(targetPosition.x - 1, targetPosition.y);

        if((tile1!=null && tile3!= null && tile1.Type == EnvironmentTile.TileType.Accessible && tile3.Type == EnvironmentTile.TileType.Accessible) ||
            (tile2 != null && tile4 != null && tile2.Type == EnvironmentTile.TileType.Accessible && tile4.Type == EnvironmentTile.TileType.Accessible))
        {
            return true;
        }
        return false;
    }

}