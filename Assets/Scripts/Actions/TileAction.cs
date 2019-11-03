using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileActions
{

    public string name;

    protected EnvironmentTile mTile;
    protected Environment mMap;

    public TileActions(string _name, EnvironmentTile tile, Environment map)
    {
        name = _name;
        mTile = tile;
        mMap = map;
    }

    public abstract void Run();
}
