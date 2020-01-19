using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionCollection : TileAction
{
    [System.Serializable]
    protected enum ItemMode
    {
        Give, // Gives the player the item
        Take, // If the item is set to be consumed on use, use it
        HandItem // Should the entity be holding this item
    }

    [System.Serializable]
    protected struct ItemInstance
    {
        public ItemMode mode;
        public Item item;
        public uint count;
        public float chance; // What are the odds from 0-1 of the item being used
    }

    [System.Serializable]
    // This is the group if valid "take" options that the user can do
    protected struct ItemGroup
    {
        public ItemInstance[] Items;
    }


    [SerializeField] protected AnimationStates AnimationState;

    // This is the group if valid "take" options that the user can do
    [SerializeField] protected ItemGroup[] ItemGroups;

    // After the action is complete, what tile should be in the place of the current one
    // If no tile is present, do not replace
    [SerializeField] protected EnvironmentTile[] ReplacmentTiles;

    // How long it takes for the action to complete
    [SerializeField] protected float ActionTime;

    // Once this action is complete, if there is a valid TileActionCollection on the next tile, should we execute it
    [SerializeField] protected bool Recursive;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private ItemInstance[] FindValidItemGroup(Entity entity)
    {
        foreach (ItemGroup group in ItemGroups)
        {
            bool valid = true;
            foreach (ItemInstance itemInstance in group.Items)
            {
                if (itemInstance.mode == ItemMode.Take && !entity.HasItem(itemInstance.item, itemInstance.count))
                {
                    valid = false;
                    continue;
                }
                else if (itemInstance.mode == ItemMode.HandItem && (entity.GetHandItem() == null || entity.GetHandItem().itemName != itemInstance.item.itemName))
                {
                    valid = false;
                    continue;
                }
            }
            if (valid && group.Items.Length>0)
            {
                return group.Items;
            }
        }
        return null;
    }

    public override bool Valid(Entity entity)
    {
        if (environmentTile == null) return false;
        ItemInstance[] items = FindValidItemGroup(entity);
        return items != null;
    }

    public override void Run(Entity entity)
    {
        entity.StopAllCoroutines();
        // We check to see if the action is still valid
        if (!Valid(entity))
        {
            entity.StartCoroutine(PostRun(entity));
            return;
        }

        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        if (route == null)
        {
            entity.StartCoroutine(DoCollection(entity, environmentTile));
        }
        else if (route.Count > 0)
        {
            entity.StartCoroutine(DoWalkAndCollection(entity, route, environmentTile));
        }
        else
        {
            entity.StartCoroutine(PostRun(entity));
        }
    }

    // Preform the walk animation and move onto 'DoCollectionWithDelay'
    private IEnumerator DoWalkAndCollection(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoCollection(entity, tile);
    }

    public IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
        // Turn towards the tile
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);
        entity.ChangeAnimation(AnimationState);


        yield return new WaitForSeconds(ActionTime);

        ItemInstance[] items = FindValidItemGroup(entity);

        // Handle all item removals first as we may be in a case where a item gets removed only for just enough space to be made
        // for the newly inserted items
        foreach (ItemInstance itemInstance in items)
        {
            switch (itemInstance.mode)
            {
                case ItemMode.Take:
                case ItemMode.HandItem:
                    // If the item is marked to not be consumed on use, continue
                    if (!itemInstance.item.consumeOnUse) continue;

                    float chanceRandom = Random.Range(0.0f, 1.0f);
                    // Check to see if the random chance of the item being used comes true
                    // Set to less then or equals as the random.range is set to min inclusive and it could be 0.0f
                    if (chanceRandom <= itemInstance.chance)
                    {
                        entity.RemoveFromInventory(itemInstance.item, itemInstance.count);
                    }

                    break;
            }
        }
        // Now preform all item insertions
        foreach (ItemInstance itemInstance in items)
        {
            switch (itemInstance.mode)
            {
                case ItemMode.Give:

                    float chanceRandom = Random.Range(0.0f, 1.0f);
                    // Check to see if the random chance of the item being dropped comes true
                    // Set to less then or equals as the random.range is set to min inclusive and it could be 0.0f
                    if (chanceRandom <= itemInstance.chance)
                    {
                        if (!entity.AddToInventory(itemInstance.item, itemInstance.count))
                        {
                            // Drop on the floor?
                        }
                    }
                    break;
            }
        }

        // Do we have a tile to replace the current one with?
        if (ReplacmentTiles.Length > 0)
        {
            // Replace the tile, choosing a random new tile to go in its place
            EnvironmentTile newTile = Environment.instance.ReplaceEnviromentTile(tile, ReplacmentTiles[Random.Range(0, ReplacmentTiles.Length)]);

            // If the tile is set to be recursive then call the tile action collection on the new instance
            if (Recursive)
            {
                TileActionCollection newAction = newTile.GetComponent<TileActionCollection>();
                newAction.environmentTile = newTile;
                //newAction.environmentTile = tile;
                // Verify that there is a new action is valid
                if (newAction != null && newAction.Valid(entity))
                {
                    entity.SetCurrentAction(newAction);

                    yield return newAction.DoCollection(entity, newTile);
                }
            }
        }
        yield return PostRun(entity);

    }
}
