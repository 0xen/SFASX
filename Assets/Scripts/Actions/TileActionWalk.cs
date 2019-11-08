using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionWalk : TileAction
{

    public TileActionWalk() : base("Walk")
    {

    }

    public override void Run()
    {
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;
        List<EnvironmentTile> route = Map.Solve(Character.CurrentPosition, tile);
        Character.GoTo(route);
    }
}
