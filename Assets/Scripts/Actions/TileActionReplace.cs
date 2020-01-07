using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionReplace : TileAction
{

    public float ReplaceTime = 0.0f;

    public bool Rotatable = false;

    public EnvironmentTile[] replacmentTile;

    public TileActionReplace() : base()
    {

    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null || !base.Valid(entity))
        {
            entity.ResetAction();
            return;
        }
        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoReplace(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndReplace(entity, route, environmentTile));
        }
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndReplace(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoReplace(entity, tile);
    }

    public IEnumerator DoReplace(Entity entity, EnvironmentTile tile)
    {
        entity.ChangeAnimation(AnimationStates.Gathering);
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);

        yield return new WaitForSeconds(ReplaceTime);

        if (item != null)
        {
            // We make sure we can remove the item before we replace the tile to stop people accessing the shop and selling the item mid interaction
            if(entity.RemoveFromInventory(item, amountNeeded))
            {
                GameObject newTile = Environment.instance.ReplaceEnviromentTile(tile, replacmentTile[Random.Range(0, replacmentTile.Length)]);
                // Add rotation here rather then in the prefabs so only placed items can be rotated rather then them all all the time
                if(Rotatable)
                {
                    newTile.AddComponent<TileActionRotate>();
                }
            }
        }

        entity.ChangeAnimation(AnimationStates.Idle);
        entity.ResetAction();
    }
    public override bool Valid(Entity entity)
    {
        return environmentTile.Type == EnvironmentTile.TileType.Accessible && base.Valid(entity);
    }



}
