using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionSpawnEntity : TileAction
{

    [SerializeField] private Entity entityPrefab = null;
    [SerializeField] private float spawnTime = 0.0f;

    
    public override void Run(Entity entity)
    {
        // We check to see if the action is still valid
        if (environmentTile == null || !Valid(entity))
        {
            entity.ResetAction();
            return;
        }

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoSpawn(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndSpawn(entity, route, environmentTile));
        }
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndSpawn(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoSpawn(entity, tile);
    }

    public IEnumerator DoSpawn(Entity entity, EnvironmentTile tile)
    {
        // Turn towards the tile
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);
        
        yield return new WaitForSeconds(spawnTime);


        /*
        if (item != null)
        {
            // We make sure we can remove the item before we replace the tile to stop people accessing the shop and selling the item mid interaction
            if (entity.RemoveFromInventory(item, amountNeeded))
            {

                Entity ent = GameObject.Instantiate(entityPrefab, Environment.instance.transform);

                Environment.instance.RegisterEntity(ent);

                ent.transform.position = tile.Position;
                ent.transform.rotation = Quaternion.identity;
                ent.CurrentPosition = tile;
            }
        }*/


        entity.ChangeAnimation(AnimationStates.Idle);
        entity.ResetAction();
    }
    

    public override bool Valid(Entity entity)
    {
        return environmentTile.Type == EnvironmentTile.TileType.Accessible;
    }

}
