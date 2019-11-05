using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : Action
{

    protected EnvironmentTile mTile;
    protected Environment mMap;

    public TileAction(string name, EnvironmentTile tile, Environment map) : base(name)
    {
        mTile = tile;
        mMap = map;
    }
}
