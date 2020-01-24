using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TileActionRotate : TileActionCollection
{

    // Override the "TileActionCollection" DoCollection function
    // Pause and rotate the tile
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        yield return base.DoCollection(entity, tile);

        // Rotate the tile 90 degrees clockwise
        Environment.instance.SetTileRotation(ref tile, (tile.Rotation + 1) % 4);
    }

    // If this is on a tile, it can be rotated
    public override bool Valid(Entity entity)
    {
        return true;
    }
}
