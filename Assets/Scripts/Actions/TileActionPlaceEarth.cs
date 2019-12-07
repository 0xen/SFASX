using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionPlaceEarth : TileAction
{

    public float placeTime;

    public EnvironmentTile replacmentTile;

    public TileActionPlaceEarth() : base("Place Earth")
    {

    }


    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;
        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoPlace(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndPlant(entity, route, environmentTile));
        }
    }

    private IEnumerator DoWalkAndPlant(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoPlace(entity, tile);
    }

    public IEnumerator DoPlace(Entity entity, EnvironmentTile tile)
    {


        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);

        yield return new WaitForSeconds(placeTime);
        Debug.Log("Time To Plant");

        Environment.instance.ReplaceEnviromentTile(tile, replacmentTile);
        Environment.instance.mWaterMap[environmentTile.PositionTile.x, environmentTile.PositionTile.y] = false;

        {
            int xPadding = 1;
            int yPadding = 1;
            int xMin = (tile.PositionTile.x - xPadding < 0) ? 0 : tile.PositionTile.x - xPadding;
            int yMin = (tile.PositionTile.y - yPadding < 0) ? 0 : tile.PositionTile.y - yPadding;
            int xMax = (tile.PositionTile.x + xPadding < Environment.instance.mMapGenerationPayload.size.x) ? (tile.PositionTile.x + xPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);
            int yMax = (tile.PositionTile.y + yPadding < Environment.instance.mMapGenerationPayload.size.y) ? (tile.PositionTile.y + yPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (Environment.instance.mWaterMap[x, y])
                    {
                        Vector3 position = Environment.instance.GetRawPosition(x, y);
                        EnvironmentTile newTile = null;
                        Environment.instance.AddWaterTile(position, ref newTile, Environment.instance.mMapGenerationPayload.size, x, y);
                        Environment.instance.SetupConnections(x, y);
                    }
                }
            }
            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    Environment.instance.SetupConnections(x, y);
                }
            }
        }

        {
            int xPadding = Environment.instance.seaDeapth;
            int yPadding = Environment.instance.seaDeapth;
            int xMin = (tile.PositionTile.x - xPadding < 0) ? 0 : tile.PositionTile.x - xPadding;
            int yMin = (tile.PositionTile.y - yPadding < 0) ? 0 : tile.PositionTile.y - yPadding;
            int xMax = (tile.PositionTile.x + xPadding < Environment.instance.mMapGenerationPayload.size.x) ? (tile.PositionTile.x + xPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);
            int yMax = (tile.PositionTile.y + yPadding < Environment.instance.mMapGenerationPayload.size.y) ? (tile.PositionTile.y + yPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {

                    if (Environment.instance.mWaterMap[x, y] && Environment.instance.mMap[x][y]==null)
                    {
                        Vector3 position = Environment.instance.GetRawPosition(x, y);
                        EnvironmentTile newTile = null;
                        Environment.instance.AddWaterTile(position, ref newTile, Environment.instance.mMapGenerationPayload.size, x, y);
                    }

                }
            }
        }



        PostRun(entity);
    }
    public override bool CanPreformAction(Entity entity)
    {
        return Environment.instance.mWaterMap[environmentTile.PositionTile.x, environmentTile.PositionTile.y];
    }
}