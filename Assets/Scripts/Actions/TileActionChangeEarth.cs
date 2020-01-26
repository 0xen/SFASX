using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileActionChangeEarth : TileActionCollection
{
    // Are we placing earth or digging it up
    [SerializeField] private bool PlaceEarth = true;


    // Override the "TileActionCollection" DoCollection function
    // Place or dig up the earth based on "PlaceEarth"
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        // Preform the action pause, inventory change and tile replace
        yield return base.DoCollection(entity, tile);


        // Set that the current tile is water/earth
        Environment.instance.mWaterMap[tile.PositionTile.x, tile.PositionTile.y] = !PlaceEarth;


        // Go through the 3x3 tiles around the local one updating there properties
        {
            // Calculate the min and max tile coords that could be affected by the tile change
            int xPadding = 1;
            int yPadding = 1;
            int xMin = (tile.PositionTile.x - xPadding < 0) ? 0 : tile.PositionTile.x - xPadding;
            int yMin = (tile.PositionTile.y - yPadding < 0) ? 0 : tile.PositionTile.y - yPadding;
            int xMax = (tile.PositionTile.x + xPadding < Environment.instance.mMapGenerationPayload.size.x) ? (tile.PositionTile.x + xPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);
            int yMax = (tile.PositionTile.y + yPadding < Environment.instance.mMapGenerationPayload.size.y) ? (tile.PositionTile.y + yPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);

            // Loop through all the potentially effected tile
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    // Check to see if the tile that is changes is a water tile as only the costline will change
                    if (entity.CurrentPosition.PositionTile!=new Vector2Int(x,y) && Environment.instance.mWaterMap[x, y])
                    {
                        Vector3 position = Environment.instance.GetRawPosition(x, y);
                        EnvironmentTile newTile = null;
                        // Update the tile with the its new tile
                        Environment.instance.AddWaterTile(position, ref newTile, Environment.instance.mMapGenerationPayload.size, x, y);
                    }
                    //Environment.instance.SetupConnections(x, y);
                }
            }
            // Finish up by rebuilding the connections between the affected tiles
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    Environment.instance.SetupConnections(x, y);
                }
            }
        }

        // If we were placing a tile, then the sea might need to be moved further away from the coast line to maintain the "seaDeapth"
        if(PlaceEarth)
        {
            // The x and y padding that is out far from land the water line is generated
            int xPadding = Environment.instance.seaDeapth;
            int yPadding = Environment.instance.seaDeapth;
            // Calculate the min and max tile coords that could be affected by the tile change
            int xMin = (tile.PositionTile.x - xPadding < 0) ? 0 : tile.PositionTile.x - xPadding;
            int yMin = (tile.PositionTile.y - yPadding < 0) ? 0 : tile.PositionTile.y - yPadding;
            int xMax = (tile.PositionTile.x + xPadding < Environment.instance.mMapGenerationPayload.size.x) ? (tile.PositionTile.x + xPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);
            int yMax = (tile.PositionTile.y + yPadding < Environment.instance.mMapGenerationPayload.size.y) ? (tile.PositionTile.y + yPadding) : (Environment.instance.mMapGenerationPayload.size.x - 1);

            // Loop through all tiles that should be water or another tile
            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    // If the tile is not currently created, create it
                    if (Environment.instance.mWaterMap[x, y] && Environment.instance.mMap[x][y] == null)
                    {
                        // Add a new water tile
                        Vector3 position = Environment.instance.GetRawPosition(x, y);
                        EnvironmentTile newTile = null;
                        Environment.instance.AddWaterTile(position, ref newTile, Environment.instance.mMapGenerationPayload.size, x, y);
                    }

                }
            }
        }
    }

    // Make sure that at least one of the touching tiles is land
    public override bool Valid(Entity entity)
    {
        EnvironmentTile tile = environmentTile;
        // Is the current tile water
        if (PlaceEarth != Environment.instance.mWaterMap[tile.PositionTile.x, tile.PositionTile.y] || !base.Valid(entity)) return false;
        if (!PlaceEarth && tile.Type != EnvironmentTile.TileType.Accessible) return false;

        int xMin = (tile.PositionTile.x - 1 < 0) ? 0 : tile.PositionTile.x - 1;
        int yMin = (tile.PositionTile.y - 1 < 0) ? 0 : tile.PositionTile.y - 1;
        int xMax = (tile.PositionTile.x + 1 < Environment.instance.mMapGenerationPayload.size.x) ? (tile.PositionTile.x + 1) : (Environment.instance.mMapGenerationPayload.size.x - 1);
        int yMax = (tile.PositionTile.y + 1 < Environment.instance.mMapGenerationPayload.size.y) ? (tile.PositionTile.y + 1) : (Environment.instance.mMapGenerationPayload.size.x - 1);

        // Check the 4 edges of the water tile for land to see if its remotely possible to path to it later on
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
