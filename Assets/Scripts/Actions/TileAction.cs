using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : MonoBehaviour
{

    // Made public so it is accessible by the game controller
    public Environment Map;
    public string actionName;
     
    public TileAction(string _name)
    {
        actionName = _name;
    }
    public abstract void Run(Entity entity);
}
