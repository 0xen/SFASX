using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : MonoBehaviour
{
    public EnvironmentTile environmentTile;

    public uint amountNeeded = 0;

    public Item item;

    public Sprite actionImage = null;

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

    // Is the tile action in a valid enough state that it can run
    public virtual bool Valid(Entity entity)
    {
        return entity.HasItem(item, amountNeeded);
    }

    public virtual void PostRun(Entity entity)
    {

        if (item != null)
        {
            entity.RemoveFromInventory(item, amountNeeded);
        }
    }
}
