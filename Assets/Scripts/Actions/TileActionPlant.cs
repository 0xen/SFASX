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
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;
        Run(entity, tile);
    }

    public override void Run(Entity entity, EnvironmentTile tile)
    {

    }
}
