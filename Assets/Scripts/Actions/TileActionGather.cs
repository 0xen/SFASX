using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileActionGather : TileActionCollection
{
    // Max items that can be on one tile
    [SerializeField] private int maxItems = 0;
    // Spawn rate of the items
    [SerializeField] private float itemSpawnRate = 0.0f;
    // Notification icon instance that exists on the tile
    [SerializeField] private ObjectNotification Notification = null;

    // Time until the next item spawn
    private float mItemSpawnRateDelta = 0.0f;
    private uint currentItemCount = 0;

    public void Start()
    {
        // Init the member variables
        mItemSpawnRateDelta = itemSpawnRate;
    }

    
    public void Update()
    {
        mItemSpawnRateDelta -= Time.deltaTime;
        // If the delta time has elapsed
        if (mItemSpawnRateDelta<0.0f)
        {
            // Re-init the delta time
            mItemSpawnRateDelta = itemSpawnRate;
            // If we have not reached the max items then increment the items
            if(currentItemCount < maxItems)
            {
                currentItemCount++;
            }
            else
                // Update the notification system that the entity is full
                Notification.DisplayNotification(true);
        }
    }

    // Override the "TileActionCollection" DoCollection function
    // Update the item group for this script with the member item count and preform the collection
    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        // Loop through all item groups, if we find a Give action, then change its count to the new count
        for (int i = 0; i < ItemGroups.Length; i++)
        {
            for (int j = 0; j < ItemGroups[i].Items.Length; j++)
            {
                if (ItemGroups[i].Items[j].mode == ItemMode.Give)
                {
                    ItemGroups[i].Items[j].count = currentItemCount;
                }
            }
        }
        // Preform the collection
        yield return base.DoCollection(entity, tile);
        Notification.DisplayNotification(false);
        currentItemCount = 0;
    }

    // Valid if we have some items to collect and the base class returns valid
    public override bool Valid(Entity entity)
    {
        return currentItemCount > 0 && base.Valid(entity);
    }
}