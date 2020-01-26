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


        int xMin = (targetPosition.x - 1 < 0) ? 0 : targetPosition.x - 1;
        int yMin = (targetPosition.y - 1 < 0) ? 0 : targetPosition.y - 1;
        int xMax = (targetPosition.x + 1 < Environment.instance.mMapGenerationPayload.size.x) ? (targetPosition.x + 1) : (Environment.instance.mMapGenerationPayload.size.x - 1);
        int yMax = (targetPosition.y + 1 < Environment.instance.mMapGenerationPayload.size.y) ? (targetPosition.y + 1) : (Environment.instance.mMapGenerationPayload.size.x - 1);


        // Make sure we are not going out of the map bounds
        if (xMin != targetPosition.x && xMax != targetPosition.x)
        {
            EnvironmentTile tile2 = Environment.instance.GetTile(targetPosition.x + 1, targetPosition.y);
            EnvironmentTile tile4 = Environment.instance.GetTile(targetPosition.x - 1, targetPosition.y);
            if (tile2 != null && tile2.Type == EnvironmentTile.TileType.Accessible && tile4 != null && tile4.Type == EnvironmentTile.TileType.Accessible)
                return true;
        }
        else if (xMin != targetPosition.x && xMax != targetPosition.x)// Make sure we are not going out of the map bounds
        {
            EnvironmentTile tile1 = Environment.instance.GetTile(targetPosition.x, targetPosition.y + 1);
            EnvironmentTile tile3 = Environment.instance.GetTile(targetPosition.x, targetPosition.y - 1);
            if (tile1 != null && tile1.Type == EnvironmentTile.TileType.Accessible && tile3 != null && tile3.Type == EnvironmentTile.TileType.Accessible)
                return true;
        }

        return false;
    }

}