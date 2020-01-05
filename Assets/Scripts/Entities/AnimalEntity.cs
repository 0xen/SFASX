using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalEntity : Entity
{
    
    [SerializeField] private AnimalActions[] actions;

    [SerializeField] private int walkRange;

    [SerializeField] private Item item;
    [SerializeField] private uint maxItems;
    [SerializeField] private float itemSpawnTime;

    [SerializeField] private ObjectNotification Notification;

    public enum AnimalActions
    {
        Walk
    }

    public float minTimeBeforeAction;

    public float maxTimeBeforeAction;

    private float itemSpawnDelta;

    private float actionDelta;

    private bool mPreformingAction;

    const int AnimalInventorySize = 1;


    public AnimalEntity() : base(AnimalInventorySize)
    {
        mPreformingAction = false;
        itemSpawnDelta = itemSpawnTime;
    } 

    public void Start()
    {
        actionDelta = Random.Range(minTimeBeforeAction, maxTimeBeforeAction);
    }


    private void Update()
    {
        if (CanPreformAction())
        {
            if (actions.Length > 0)
            {
                switch (actions[Random.Range(0, actions.Length)])
                {
                    case AnimalActions.Walk:
                        Walk(walkRange);
                        break;

                }
            }
        }
        itemSpawnDelta -= Time.deltaTime;
        if (itemSpawnDelta < 0)
        {
            // Pre Inventory insertion
            if (!HasItem(item, maxItems))
            {
                AddToInventory(item, 1);
            }
            // Post inventory insertion
            Notification.DisplayNotification(HasItem(item, maxItems));
            itemSpawnDelta = itemSpawnTime;
        }
    }
    
    public override Item GetHandItem()
    {
        return inventory[0];
    }

    public override void InventoryChange(Item item, uint count, InventoryChangeEvent eve)
    {

    }

    public void Walk(int range)
    {
        Vector2Int mapSize = Environment.instance.mMapGenerationPayload.size;
        // Calculate the mins and maxes for the possible touching tile coordinates
        int xMin = CurrentPosition.PositionTile.x - range < 0 ? 0 : CurrentPosition.PositionTile.x - range;
        int yMin = CurrentPosition.PositionTile.y - range < 0 ? 0 : CurrentPosition.PositionTile.y - range;
        int xMax = CurrentPosition.PositionTile.x + range < mapSize.x ? CurrentPosition.PositionTile.x + range : mapSize.x - 1;
        int yMax = CurrentPosition.PositionTile.y + range < mapSize.y ? CurrentPosition.PositionTile.y + range : mapSize.y - 1;


        int randomX = Random.Range(xMin, xMax);
        int randomY = Random.Range(yMin, yMax);

        EnvironmentTile destination = Environment.instance.GetTile(randomX, randomY);

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(CurrentPosition, destination);

        // If the animal is already there, break
        if (route == null)
        {
            mPreformingAction = false;
            return;
        }
        if (route.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(DoWalk(route, destination));
        }

    }

    private IEnumerator DoWalk(List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(this, GetMovmentSpeed(), route);
        mPreformingAction = false;
    }

    public bool CanPreformAction()
    {
        if (mPreformingAction) return false;
        actionDelta -= Time.deltaTime;
        if (actionDelta<0)
        {
            actionDelta = Random.Range(minTimeBeforeAction, maxTimeBeforeAction);
            mPreformingAction = true;
            return true;
        }
        return false;
    }

}
