using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityActionGather : TileAction
{
    public float GatherTime = 0.0f;
    
    [SerializeField] private uint maxItems = 0;
    [SerializeField] private float itemSpawnTime = 0.0f;

    [SerializeField] private Entity GatherEntity = null;

    [SerializeField] private ObjectNotification Notification = null;

    // Do they spawn with items?
    [SerializeField] private uint ItemCount = 0;

    [SerializeField] private bool KillOnGather = false;
    [SerializeField] private bool GatherAllAtOnce = false;

    private float itemSpawnDelta;

    public EntityActionGather() : base("Gather")
    {
        itemSpawnDelta = itemSpawnTime;
    }

    public void Update()
    {
        itemSpawnDelta -= Time.deltaTime;
        if (itemSpawnDelta < 0)
        {

            if(ItemCount < maxItems)
            {
                ItemCount++;
            }
            // Post inventory insertion
            itemSpawnDelta = itemSpawnTime;
        }
        Notification.DisplayNotification(ItemCount == maxItems);
    }

    public override void Run(Entity entity)
    {
        if (entity.CurrentPosition == null) return;

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoGather(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndGather(entity, route, environmentTile));
        }
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndGather(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoGather(entity, tile);
    }

    public IEnumerator DoGather(Entity entity, EnvironmentTile tile)
    {
        // Turn towards the tile
        entity.transform.rotation = Quaternion.LookRotation(environmentTile.Position - entity.CurrentPosition.Position, Vector3.up);

        entity.ChangeAnimation(AnimationStates.Gathering);

        if (GatherAllAtOnce)
        {
            yield return new WaitForSeconds(GatherTime);
            if (!entity.AddToInventory(item, ItemCount))
            {
                // Drop item on ground
            }
        }
        else
        {
            for (int i = 0; i < ItemCount; i++)
            {
                if (!entity.AddToInventory(item, 1))
                {
                    // Drop item on ground
                }
                Notification.DisplayNotification(false);
                yield return new WaitForSeconds(GatherTime);
            }
        }

        ItemCount = 0;
        if(KillOnGather)
        {
            Environment.instance.RemoveEntity(GatherEntity);
            Destroy(GatherEntity.gameObject);
            Environment.instance.notificationHandler.AddNotification(ref LandmarkNotification.FirstKill, "Your first kill! You can sell the items you have gathered from the animal to the shop");
        }
        else
        {
            Environment.instance.notificationHandler.AddNotification(ref LandmarkNotification.FirstAnimalGather, "Animals are a great renewable resource, make sure to keep a eye on them and don't let them wander too far");
        }

        entity.ChangeAnimation(AnimationStates.Idle);
        entity.ResetAction();
    }

    public override bool Valid(Entity entity)
    {
        return ItemCount > 0;
    }

}
