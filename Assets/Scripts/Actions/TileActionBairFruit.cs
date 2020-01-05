using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileActionBairFruit : TileAction
{

    [SerializeField] private Renderer[] spawnLocations;
    [SerializeField] private GameObject furitPrefab;


    [SerializeField] private float bairFruitTimeMin;
    [SerializeField] private float bairFruitTimeMax;

    [SerializeField] private float fruitApearTime;

    [SerializeField] private int maxFruit;
    [SerializeField] private float collectionTime;

    [System.Serializable]
    public struct Pickup
    {
        public Item item;
        public uint count;
    }

    public Pickup[] pickups;

    private int m_nextSpawnLocation;
    private float m_timeBeforeSpawn;
    private List<GameObject> m_aliveFruit;

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
    }

    private IEnumerator DoWalkAndCollect(Entity entity, List<EnvironmentTile> route, EnvironmentTile tile)
    {
        yield return TileActionWalk.DoGoTo(entity, entity.GetMovmentSpeed(), route);
        yield return DoCollect(entity, tile);
    }

    public IEnumerator DoCollect(Entity entity, EnvironmentTile tile)
    {
        for(int i = 0; i < m_aliveFruit.Count; i++)
        {
            foreach (Pickup pickup in pickups)
            {
                if (!entity.AddToInventory(pickup.item, pickup.count))
                {
                    // Drop item on ground
                }
            }
            Destroy(m_aliveFruit[0]);
            m_aliveFruit.RemoveAt(0);
            yield return new WaitForSeconds(collectionTime);
        }
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
            body.AddForce(new Vector3(Random.Range(0, 001f), Random.Range(0, 001f), Random.Range(0, 001f)), ForceMode.Impulse);
            m_aliveFruit.Add(fruit);

        }
        if(m_aliveFruit.Count > maxFruit)
        {
            Destroy(m_aliveFruit[0]);
            m_aliveFruit.RemoveAt(0);
        }
    }

}
