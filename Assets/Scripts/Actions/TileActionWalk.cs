using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionWalk : TileActions
{
    public TileActionWalk(EnvironmentTile tile) : base("Walk", tile)
    {

    }
    public override void Run()
    {
        Debug.Log("#Walking");
    }
}
