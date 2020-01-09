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
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndPlant(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoPlace(entity, tile);
    }

    public IEnumerator DoPlace(Entity entity, EnvironmentTile tile)
    {
        entity.ChangeAnimation(AnimationStates.Gathering);

        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);

        yield return new WaitForSeconds(placeTime);

        Environment.instance.ReplaceEnviromentTile(tile, replacmentTile);
        Environment.instance.mWaterMap[tile.PositionTile.x, tile.PositionTile.y] = false;

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


       // Environment.instance.notificationHandler.AddNotification(ref LandmarkNotification.ReclaimingLand, "Resources can be sold to the shop to buy more usefull items such as animals or seeds");

        entity.ResetAction();
        entity.ChangeAnimation(AnimationStates.Idle);
        PostRun(entity);
    }

    // Make sure that at least one of the touching tiles is land
    public override bool Valid(Entity entity)
    {
        EnvironmentTile tile = environmentTile;
        // Is the current tile water
        if (!Environment.instance.mWaterMap[tile.PositionTile.x, tile.PositionTile.y] || !base.Valid(entity)) return false;

        int xMin = (tile.PositionTile.x - 1 < 0) ? 0 : tile.PositionTile.x - 1;
        int yMin = (tile.PositionTile.y - 1 < 0) ? 0 : tile.PositionTile.y - 1;
        int xMax = (tile.PositionTile.x + 1 < Environment.instance.mMapGenerationPayload.size.x) ? (tile.PositionTile.x + 1) : (Environment.instance.mMapGenerationPayload.size.x - 1);
        int yMax = (tile.PositionTile.y + 1 < Environment.instance.mMapGenerationPayload.size.y) ? (tile.PositionTile.y + 1) : (Environment.instance.mMapGenerationPayload.size.x - 1);

        // Check the 4 edges of the water tile for land
        if (!Environment.instance.mWaterMap[xMin, tile.PositionTile.y] ||
            !Environment.instance.mWaterMap[xMax, tile.PositionTile.y] ||
            !Environment.instance.mWaterMap[tile.PositionTile.x, yMin] ||
            !Environment.instance.mWaterMap[tile.PositionTile.x, yMax])
        {
            return true;
        }

        return false;
    }
}