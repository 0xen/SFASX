using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : MonoBehaviour
{
    public EnvironmentTile environmentTile;

    // Made public so it is accessible by the game controller
    public string actionName;

    public TileAction()
    {

    }
    public TileAction(string _name)
    {
        actionName = _name;
    }
    public abstract void Run(Entity entity);
    public virtual bool CanPreformAction(Entity entity)
    {
        return true;
    }
}
