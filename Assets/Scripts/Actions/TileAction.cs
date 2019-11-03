using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileActions
{

    public string name;
    EnvironmentTile tile;
    public TileActions(string _name, EnvironmentTile tile)
    {
        name = _name;
    }

    public abstract void Run();
}
