using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : Action
{

    // Made public so it is accessible by the game controller
    public Environment Map;
    public Character Character;

    public TileAction(string name) : base(name)
    {

    }
}
