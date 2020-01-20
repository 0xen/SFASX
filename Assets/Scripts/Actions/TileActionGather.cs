using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileActionGather : TileActionCollection
{
    [SerializeField] private int maxItems = 0;
    [SerializeField] private float itemSpawnRate = 0.0f;
    [SerializeField] private ObjectNotification Notification = null;

    private float mItemSpawnRateDelta = 0.0f;
    private uint currentItemCount = 0;

    public void Start()
    {
        mItemSpawnRateDelta = itemSpawnRate;
    }

    public void Update()
    {
        mItemSpawnRateDelta -= Time.deltaTime;
        if (mItemSpawnRateDelta<0.0f)
        {
            mItemSpawnRateDelta = itemSpawnRate;
            if(currentItemCount < maxItems)
            {
                currentItemCount++;
            }
            else
                Notification.DisplayNotification(true);
        }
    }

    public override IEnumerator DoCollection(Entity entity, EnvironmentTile tile)
    {
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
        yield return base.DoCollection(entity, tile);
        Notification.DisplayNotification(false);
        currentItemCount = 0;
    }

    public override bool Valid(Entity entity)
    {
        return currentItemCount > 0 && base.Valid(entity);
    }
}



/*
public class TileActionGather : TileAction
{

    [SerializeField] private Renderer[] spawnLocations = null;
    [SerializeField] private GameObject furitPrefab = null;


    [SerializeField] private float bairFruitTimeMin = 0.0f;
    [SerializeField] private float bairFruitTimeMax = 0.0f;

    [SerializeField] private float fruitApearTime = 0.0f;

    [SerializeField] private int maxFruit = 0;
    [SerializeField] private float collectionTime = 0.0f;

    [SerializeField] private ObjectNotification Notification = null;

    [System.Serializable]
    public struct Pickup
    {
        public Item item;
        public uint count;
    }

    public Pickup[] pickups = null;

    private int m_nextSpawnLocation = 0;
    private float m_timeBeforeSpawn = 0.0f;
    private List<GameObject> m_aliveFruit = null;

    public TileActionBairFruit() : base()
    {
        m_aliveFruit = new List<GameObject>(); 
    }

    public void Start()
    {
        m_timeBeforeSpawn = Random.Range(bairFruitTimeMin, bairFruitTimeMax);
        m_nextSpawnLocation = Random.Range(0, spawnLocations.Length);
    }

    public override void Run(Entity entity)
    {
        if (environmentTile == null) return;
        List<EnvironmentTile> route = Environment.instance.SolveNeighbour(entity.CurrentPosition, environmentTile);
        // We are at the location
        if (route == null)
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoCollect(entity, environmentTile));
        }
        else if (route.Count > 0) // We need to path to the location
        {
            entity.StopAllCoroutines();
            entity.StartCoroutine(DoWalkAndCollect(entity, route, environmentTile));
        }
        else
        {
            entity.ResetAction();
        }
    }

    private IEnumerator DoWalkAndCollect(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoCollect(entity, tile);
    }

    public IEnumerator DoCollect(Entity entity, EnvironmentTile tile)
    {
        // Turn towards the tile
        entity.transform.rotation = Quaternion.LookRotation(tile.Position - entity.CurrentPosition.Position, Vector3.up);


        for (int i = m_aliveFruit.Count-1; i >=0 ; i--)
        {
            foreach (Pickup pickup in pickups)
            {
                if (!entity.AddToInventory(pickup.item, pickup.count))
                {
                    // Drop item on ground
                }
            }
            Destroy(m_aliveFruit[i]);
            m_aliveFruit.RemoveAt(i);
            Notification.DisplayNotification(false);
            yield return new WaitForSeconds(collectionTime);
        }
        entity.ResetAction();
    }

    public override bool Valid(Entity entity)
    {
        return m_aliveFruit.Count > 0;
    }


    // Update is called once per frame
    void Update()
    {
        m_timeBeforeSpawn -= Time.deltaTime;

        if (m_timeBeforeSpawn < fruitApearTime)
        {
            spawnLocations[m_nextSpawnLocation].enabled = true;
        }

        if (m_timeBeforeSpawn < 0)
        {
            spawnLocations[m_nextSpawnLocation].enabled = false;
            Transform spawnLocation = spawnLocations[m_nextSpawnLocation].transform;

            m_timeBeforeSpawn = Random.Range(bairFruitTimeMin, bairFruitTimeMax);
            m_nextSpawnLocation = Random.Range(0, spawnLocations.Length);


            GameObject fruit = GameObject.Instantiate(furitPrefab, spawnLocation.position, spawnLocation.rotation, transform);
            Rigidbody body = fruit.GetComponent<Rigidbody>();
            body.AddForce(new Vector3(Random.Range(-001f, 001f), Random.Range(-001f, 001f), Random.Range(-001f, 001f)), ForceMode.Impulse);
            m_aliveFruit.Add(fruit);

        }
        if (m_aliveFruit.Count == maxFruit)
        {
            Notification.DisplayNotification(true);
        }
        else if (m_aliveFruit.Count > maxFruit)
        {
            Destroy(m_aliveFruit[0]);
            m_aliveFruit.RemoveAt(0);
        }
    }

}
*/