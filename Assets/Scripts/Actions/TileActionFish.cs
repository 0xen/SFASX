using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionFish : TileAction
{

    public TileActionFish() : base("Fish")
    {

    }

    public override void Run()
    {
        EnvironmentTile tile = this.GetComponent<EnvironmentTile>();
        if (tile == null) return;



    }
}