using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionRotate : TileAction
{

    public float rotateTime = 0.0f;

    public TileActionRotate() : base()
    {
        actionName = "Rotate";
    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;
        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        // We are at the location
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoReplace(entity, environmentTile));
        }
        else if (route.Count > 0) // We need to path to the location
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndReplace(entity, route, environmentTile));
        }
    }

    private IEnumerator DoWalkAndReplace(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoReplace(entity, tile);
    }

    public IEnumerator DoReplace(Entity entity, EnvironmentTile tile)
    {

        // Turn the player towards the object
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);

        // Rotate the tile 90 degrees clockwise
        Environment.instance.SetTileRotation(ref tile, (tile.Rotation + 1) % 4);
        
        yield return new WaitForSeconds(rotateTime);
    }
    public override bool Valid(Entity entity)
    {
        return true;
    }


}
