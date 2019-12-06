using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionReplace : TileAction
{
    public float ReplaceTime = 0.0f;

    public EnvironmentTile[] replacmentTile;

    public TileActionReplace() : base()
    {

    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;
        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoReplace(entity, environmentTile));
        }
        else if (route.Count > 0)
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


        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);

        yield return new WaitForSeconds(ReplaceTime);
        Environment.instance.ReplaceEnviromentTile(tile, replacmentTile[Random.Range(0, replacmentTile.Length)]);

    }
    public override bool CanPreformAction(Entity entity)
    {
        return environmentTile.Type == EnvironmentTile.TileType.Accessible;
    }



}
