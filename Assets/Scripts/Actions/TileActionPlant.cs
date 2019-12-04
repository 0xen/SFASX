using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionPlant : TileAction
{
    public float PlantTime = 0.0f;

    public EnvironmentTile[] replacmentTile;

    public TileActionPlant() : base("Plant")
    {

    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;
        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoPlant(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndPlant(entity, route, environmentTile));
        }
    }

    private IEnumerator DoWalkAndPlant(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, 0.5f, route);
        yield return DoPlant(entity, tile);
    }

    public IEnumerator DoPlant(Entity entity, EnvironmentTile tile)
    {


        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);

        yield return new WaitForSeconds(PlantTime);
        Debug.Log("Time To Plant");
        Environment.instance.ReplaceEnviromentTile(tile, replacmentTile[Random.Range(0, replacmentTile.Length)]);

    }



}
