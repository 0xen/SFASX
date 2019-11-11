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
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;

        List<EnvironmentTile> route = Map.SolveNeighbour(entity.CurrentPosition, tile);
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
        yield return TileActionWalk.DoGoTo(entity, 0.5f, route);
        yield return DoBuild(entity);
    }

    private IEnumerator DoBuild(Entity entity)
    {

        yield return null;
    }
}