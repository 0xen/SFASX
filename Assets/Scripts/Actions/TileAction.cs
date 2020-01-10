using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileAction : MonoBehaviour
{
    public EnvironmentTile environmentTile;

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
    // Force the child to override
    public abstract bool Valid(Entity entity);

    public virtual IEnumerator PostRun(Entity entity)
    {
        entity.ChangeAnimation(AnimationStates.Idle);
        entity.ResetAction();
        yield return null;
    }
}
