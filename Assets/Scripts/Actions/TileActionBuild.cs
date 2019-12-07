using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionBuild : TileAction
{

    public TileActionBuild() : base("Build")
    {

    }
    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoBuild(entity));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndBuild(entity, route));
        }
    }

    private IEnumerator DoWalkAndBuild(Entity entity, List<EnvironmentTile> route)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoBuild(entity);
    }

    private IEnumerator DoBuild(Entity entity)
    {

        yield return null;
    }
}