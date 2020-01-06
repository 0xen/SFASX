using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityActionGather : TileAction
{
    public float GatherTime = 0.0f;
    
    [SerializeField] private uint maxItems;
    [SerializeField] private float itemSpawnTime;

    [SerializeField] private Entity GatherEntity;

    [SerializeField] private ObjectNotification Notification;

    // Do they spawn with items?
    [SerializeField] private uint ItemCount;

    [SerializeField] private bool KillOnGather;

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

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, GatherEntity.CurrentPosition);
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoGather(entity, GatherEntity.CurrentPosition));
        }
        else if (route.Count > 0)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndGather(entity, route, GatherEntity.CurrentPosition));
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
        entity.transform.rotation = Quaternion.LookRotation(GatherEntity.CurrentPosition.Position - entity.CurrentPosition.Position, Vector3.up);


        
        for(int i = 0; i < ItemCount; i++)
        {
            if (!entity.AddToInventory(item, 1))
            {
                // Drop item on ground
            }
            Notification.DisplayNotification(false);
            yield return new WaitForSeconds(GatherTime);
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
        
        entity.ResetAction();
    }

    public override bool Valid(Entity entity)
    {
        return ItemCount > 0;
    }

}
