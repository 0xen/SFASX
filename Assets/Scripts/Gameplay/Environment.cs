using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static WorldGenerator;

public class Environment : MonoBehaviour
{
    public static Environment instance;

    // Used as a reference when loading a map file, what tiles belong to what name
    [SerializeField] private EnvironmentTile[] WorldEnviromentTiles = null;

    // Used for the loading and unloading of game entities
    [SerializeField] private Entity[] EntityInstances = null;

    [System.Serializable]
    public struct TileInstance
    {
        // This is used to define how frequantly a object should spawn in the world.
        // Range 1-100, 1 being rare and 100 being common
        public float spawnChance;
        public EnvironmentTile tile;
    }
    [System.Serializable]
    public struct WorldTiles
    {
        public EnvironmentTile.TileType type;
        // Spawn chance of it choosing this tile group
        public float spawnChance;
        public List<TileInstance> tiles;
    }
    [Header("World Tiles")]
    [SerializeField] private WorldTiles[] WorldTileGroups = null;

    private Dictionary<EnvironmentTile.TileType, WorldTiles> mTiles; 

    [System.Serializable]
    public struct WaterTile
    {
        public EnvironmentTile tile;

        // A array of 8 elements, starting from the North position of the model. it represents if at that position the tile
        // touches water and rotates in a clock wise order
        public bool[] Connectors;
    }
    [Header("Water Tile Configuration")]
    [SerializeField] private List<WaterTile> WaterTiles = null;

    public struct WaterTileSearchResult
    {
        public EnvironmentTile tile;
        public int rotation;
    }

    [Header("Sea Settings")]
    [SerializeField] public int seaDeapth;

    [Header("Shading")]
    [SerializeField] private Shader TintShader = null;

    [Header("UI")]
    [SerializeField] private GameObject ItemPickupUIParent = null;
    [SerializeField] private ItemPickupUi ItemPickupUIInstance = null;


    // Offset as the animator applies a position offset
    [SerializeField] private float CharacterYOffset = 3.0f;

    public NotificationHandler notificationHandler = null;


    public EnvironmentTile[][] mMap;
    public bool[,] mWaterMap;

    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    private List<Entity> mEntities;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    private Character mCharacter;

    public GenerationPayload mMapGenerationPayload;


    public EnvironmentTile Start { get; private set; }

    public void AddItemToPickupUI(string name, uint count, Sprite sprite)
    {
        ItemPickupUi ui = GameObject.Instantiate(ItemPickupUIInstance);

        ui.transform.SetParent(ItemPickupUIParent.transform);
        ui.SetupUI("+" + count + " " + name, sprite);

        RectTransform recTransform = ui.GetComponent<RectTransform>();
        recTransform.localScale = new Vector3(1, 1, 1);
        recTransform.localEulerAngles = new Vector3(0, 0, 0);
        recTransform.localPosition = new Vector3(recTransform.position.x, recTransform.position.y, 0);
    }

    private void Awake()
    {
        instance = this;
        mAll = new List<EnvironmentTile>();
        mEntities = new List<Entity>();
        mToBeTested = new List<EnvironmentTile>();
        mTiles = new Dictionary<EnvironmentTile.TileType, WorldTiles>();
        SetupTileGroups();
    }

    public Character GetCharacter()
    {
        return mCharacter;
    }

    public void RegisterEntity(Entity entity)
    {
        mEntities.Add(entity);
    }

    public Entity[] GetEntitiesAt(Vector2Int position)
    {
        List<Entity> entities = new List<Entity>();
        foreach(Entity e in mEntities)
        {
            if(e.CurrentPosition.PositionTile == position)
            {
                entities.Add(e);
            }
        }

        return entities.ToArray();
    }

    public Entity[] GetEntities()
    {
        return mEntities.ToArray();
    }

    public void RemoveEntity(Entity entity)
    {
        mEntities.Remove(entity);
    }

    // Organise the tile map groups into a faster to search format and resolve odds of choosing tiles/groups
    private void SetupTileGroups()
    {
        //Used to store the normalised odds of choosing x group
        float groupSpawnRate = 0.0f;
        for (int i = 0; i < WorldTileGroups.Length; ++i)
        {
            // Add the groups odds to the total odds
            groupSpawnRate += WorldTileGroups[i].spawnChance;

            // Used to store the normalised odds of choosing y tile in x group
            float prefabSpawnRate = 0.0f;
            // Loop through and add the spawn chances to the spawn rate var
            foreach(TileInstance tile in WorldTileGroups[i].tiles)
            {
                prefabSpawnRate += tile.spawnChance;
            }
            // Calculate the spawn rate
            prefabSpawnRate = 100.0f / prefabSpawnRate;
            for( int j = 0; j < WorldTileGroups[i].tiles.Count; ++j)
            {
                // Recalculate the provided spawn rate with the new normalisation
                TileInstance tileInstance = WorldTileGroups[i].tiles[j];
                tileInstance.spawnChance = prefabSpawnRate * tileInstance.spawnChance;
                WorldTileGroups[i].tiles[j] = tileInstance;
            }
        }
        // Calculate the spawn rate
        groupSpawnRate = 100.0f / groupSpawnRate;

        for (int i = 0; i < WorldTileGroups.Length; ++i)
        {
            // Recalculate the provided spawn rate with the new normalisation
            WorldTileGroups[i].spawnChance *= groupSpawnRate;
            mTiles[WorldTileGroups[i].type] = WorldTileGroups[i];
        }
    }



    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < mMapGenerationPayload.size.x; ++x)
            { 
                for (int y = 0; y < mMapGenerationPayload.size.y; ++y)
                {
                    if (mMap[x][y] == null)
                        continue;
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( mMap[x][y].Type == EnvironmentTile.TileType.Inaccessible)
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    private void GenerateWaterMap()
    {
        // Generate a water map that represents what tiles of the map should be water and what ones wont be
        mWaterMap = WorldGenerator.GenerateWaterMap(mMapGenerationPayload);
    }

    private void GenerateWaterMap(Game.SaveDataPacket saveData)
    {
        // Generate a water map that represents what tiles of the map should be water and what ones wont be
        mWaterMap = new bool[saveData.WorldWidth, saveData.WorldHeight];

        for(int x = 0; x < saveData.WorldWidth; x++)
        {
            for (int y = 0; y < saveData.WorldHeight; y++)
            {
                mWaterMap[x, y] = saveData.WaterMap[x + (y * saveData.WorldWidth)];
            }
        }

    }

    // check to see if we can find any tiles that match the current dataset and append them to the search results
    private void FindWaterTileMatch(ref List<WaterTileSearchResult> results, bool[] dataset, int currentRotation)
    {
        // Loop through all avaliable tiles
        foreach (WaterTile waterTilePrefab in WaterTiles)
        {
            // check to see if the dataset matches the water connectors
            if (MatchingDataset(dataset, waterTilePrefab.Connectors))
            {
                // Add the tile to the results
                WaterTileSearchResult result = new WaterTileSearchResult();
                result.tile = waterTilePrefab.tile;
                result.rotation = currentRotation;
                results.Add(result);
            }
        }
    }
    // rotate the datasets 2 places to the right

    private void RotateWaterDataset(ref bool[] dataset)
    {
        // Loop through twise
        for(int i = 0; i < 2; i ++)
        {
            bool last = dataset[0];
            // Rotate the data one place
            for (int j = 0; j < 7; j++)
            {
                dataset[j] = dataset[j + 1];
            }
            dataset[dataset.Length - 1] = last;
        }
    }

    // Check to see if the two bool arrays match
    private bool MatchingDataset(bool[] d1, bool[] d2)
    {
        for (int i = 0; i < (d1.Length >= d2.Length ? d2.Length : d1.Length); i++) 
        {
            if (d1[i] != d2[i]) return false;
        }
        return true;
    }

    // Return a single water tile that matches the enviroment requirments
    private bool GetWaterTile(int x, int y, ref EnvironmentTile tile, ref int rotation)
    {
        // By default, set all the values to true. This is incase any of the tiles are of the map, they should be by default water
        bool[] dataset = new bool[8] { true, true, true, true, true, true, true, true };
        // Check each axis to see if they are in range or not
        bool xMin = x > 0;
        bool yMin = y > 0;
        bool xMax = x < mMapGenerationPayload.size.x - 1;
        bool yMax = y < mMapGenerationPayload.size.y - 1;

        // If each NWEW tile is in the map, check to see if they are water
        if (yMax) dataset[0] = mWaterMap[x,y + 1]; // N
        if (xMax) dataset[2] = mWaterMap[x + 1,y]; // E
        if (yMin) dataset[4] = mWaterMap[x,y - 1]; // S
        if (xMin) dataset[6] = mWaterMap[x - 1,y]; // W

        // Check to see if the corner blocks ar ein the scene, if thye are load there data
        if (xMax && yMax) dataset[1] = mWaterMap[x + 1,y + 1]; // NE
        if (xMax && yMin) dataset[3] = mWaterMap[x + 1,y - 1]; // SE
        if (xMin && yMin) dataset[5] = mWaterMap[x - 1,y - 1]; // SW
        if (xMin && yMax) dataset[7] = mWaterMap[x - 1,y + 1]; // NW

        // We loop through 4 times for each corner block, if the side blocks surrounding the corner blocks are not water, then the corner cant be water
        for(int i = 0; i < 8; i+=2)
        {
            if (!dataset[i] || !dataset[(i + 2) % 8]) dataset[(i + 1) % 8] = false;
        }

        // Create a results array to sample from
        List<WaterTileSearchResult> results = new List<WaterTileSearchResult>();

        // Check to see if we can find some tiles that match the dataset/rotation
        FindWaterTileMatch(ref results, dataset, 0);

        for(int i = 0; i < 3; i ++)
        {
            // Rotate the map 90 degrees
            RotateWaterDataset(ref dataset);
            // Check to see if we can find some tiles that match the dataset/rotation
            FindWaterTileMatch(ref results, dataset, i + 1);
        }

        // If we had no results, return
        if (results.Count == 0) return false;
        
        WaterTileSearchResult responce = results[Random.Range(0, results.Count)];
        tile = responce.tile;
        rotation = responce.rotation;

        return true;
    }



    public void GenerateWaterTile()
    {

        
    }

    public Vector3 GetRawPosition(int x, int z)
    {
        int halfWidth = mMapGenerationPayload.size.x / 2;
        int halfHeight = mMapGenerationPayload.size.y / 2;
        Vector3 position = new Vector3(-(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize));
        position.x += x * TileSize;
        position.z += z * TileSize;
        return position;
    }


    public void AddWaterTile(Vector3 position,ref EnvironmentTile tile, Vector2Int mapSize, int x, int y)
    {

        bool foundLand = false;
        for (int xa = x - seaDeapth; !foundLand && xa < x + seaDeapth + 1; xa++)
        {
            if (xa < 0)
                continue;
            if (xa > mapSize.x - 1)
                break;

            for (int ya = y - seaDeapth; !foundLand && ya < y + seaDeapth + 1; ya++)
            {
                if (ya < 0)
                    continue;
                if (ya > mapSize.y - 1)
                    break;

                if (!mWaterMap[xa, ya])
                {
                    foundLand = true;
                    break;
                }
            }
        }
        if (!foundLand)
        {
            position.z += TileSize;
            return;
        }



        int rotation = 0;
        EnvironmentTile prefab = null;
        // Return what water tile fits within the provided world requirment
        if (GetWaterTile(x, y, ref prefab, ref rotation))
        {
            // Calculate the posiiton and rotation of the water tile in the world
            Quaternion q = Quaternion.Euler(0, 90 * rotation, 0);
            Vector3 positionOffset = new Vector3();

            positionOffset.x += (rotation > 1 ? 10.0f : 0);
            positionOffset.z += (rotation >= 1 && rotation < 3 ? 10.0f : 0);

            tile = Instantiate(prefab, position + positionOffset, q, transform);
            tile.Type = EnvironmentTile.TileType.Inaccessible;
        }
        else // Output a error if we cant find the required tile
        {
            Debug.LogError("Could not find water tile to fit senario");
        }


        FinalizeTile(ref tile, x, y, position);

    }

    public void AddLandTile(Vector3 position, ref EnvironmentTile tile, Vector2Int mapSize, int x, int y, string name, int rotation)
    {

        for(int i = 0; i < WorldEnviromentTiles.Length;i++)
        {
            if(WorldEnviromentTiles[i].TileName==name)
            {
                tile = Instantiate(WorldEnviromentTiles[i], position, Quaternion.identity, transform);
                break; ;
            }
        }

        if (tile == null) return;

        FinalizeTile(ref tile, x, y, position);

        // Apply defined rotation
        SetTileRotation(ref tile, rotation);
    }

    public void AddLandTile(Vector3 position, ref EnvironmentTile tile, Vector2Int mapSize, int x, int y)
    {

        // Calculate the random chance of choosing x tile group
        float randomChoice = Random.Range(0.0f, 100.0f);
        float runningTotal = 0.0f;
        // Provide a default tile group incase it falls through
        WorldTiles worldTilesChoice = mTiles[EnvironmentTile.TileType.Accessible];

        foreach (WorldTiles worldTiles in mTiles.Values)
        {
            // If the current tile group falls within the bounds of the random number, select the tile group
            if (runningTotal + worldTiles.spawnChance > randomChoice)
            {
                worldTilesChoice = worldTiles;
                break;
            }
            runningTotal += worldTiles.spawnChance;
        }

        EnvironmentTile prefab = null;
        // Calculate the random chance of choosing x tile
        randomChoice = Random.Range(0.0f, 100.0f);
        runningTotal = 0;
        foreach (TileInstance tileInstance in worldTilesChoice.tiles)
        {
            // If the current tile group falls within the bounds of the random number, select the tile group
            if (runningTotal + tileInstance.spawnChance > randomChoice)
            {
                prefab = tileInstance.tile;
                break;
            }
            runningTotal += tileInstance.spawnChance;
        }

        
        tile = Instantiate(prefab, position, Quaternion.identity, transform);
        tile.Type = worldTilesChoice.type;
        
        FinalizeTile(ref tile, x, y, position);

        // Rotate the tile a random amount
        int rotationRand = Random.Range(0, 4);
        SetTileRotation(ref tile, rotationRand);
    }

    public void SetTileRotation(ref EnvironmentTile tile, int newRotation)
    {

        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(0.0f, 90.0f * newRotation, 0.0f);
        
        Vector3 Posiiton0ffset = new Vector3(
            newRotation < 2 ? 0.0f : 10.0f,   // Rotation 0-1 Needs a offset of 0 and rotation 2-3 needs a offset of 10
            0, // Y axis is unchanged
            newRotation < 1 || newRotation > 2 ? 0.0f : 10.0f // Rotation 0 and 3 needs a offset of 0 and rotation 1 and 2 need a offset of 10
            );

        Vector3 position = GetRawPosition(tile.PositionTile.x, tile.PositionTile.y) + Posiiton0ffset;
        tile.transform.localPosition = position;
        tile.transform.localRotation = rotation;
        // Set the environment tile so it knows what rotation the tile is at
        tile.Rotation = newRotation;
    }

    private void FinalizeTile(ref EnvironmentTile tile, int x, int y, Vector3 position)
    {
        // Attach the tint shader to all the tile blocks
        foreach (Material m in tile.GetComponent<MeshRenderer>().materials)
        {
            m.shader = TintShader;
        }


        tile.PositionTile = new Vector2Int(x, y);

        tile.Position = new Vector3(position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));

        tile.gameObject.name = string.Format("Tile({0},{1})", x, y);

        if (mMap[x][y] != null) Destroy(mMap[x][y].gameObject);

        mMap[x][y] = tile;
        mAll.Add(tile);

    }

    private void Generate(Game.SaveDataPacket saveData)
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage

        Vector2Int Size = mMapGenerationPayload.size;

        mMap = new EnvironmentTile[Size.x][];
         
        for (int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for (int y = 0; y < Size.y; ++y)
            {
                bool isWater = mWaterMap[x, y];

                EnvironmentTile tile = null;

                Vector3 position = GetRawPosition(x, y);

                if (isWater)
                {
                    AddWaterTile(position, ref tile, mMapGenerationPayload.size, x, y);
                }
                else
                {
                    Game.TileSaveData saveInstance = saveData.TileData[x + (y * Size.x)];

                    AddLandTile(position, ref tile, mMapGenerationPayload.size, x, y, saveInstance.N, saveInstance.R);
                }
            }
        }
    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage

        Vector2Int Size = mMapGenerationPayload.size;

        mMap = new EnvironmentTile[Size.x][];

        bool hasStart = true;

        for (int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for (int y = 0; y < Size.y; ++y)
            {
                bool isWater = mWaterMap[x, y];

                EnvironmentTile tile = null;

                Vector3 position = GetRawPosition(x, y);

                if (isWater)
                {
                    AddWaterTile(position, ref tile, mMapGenerationPayload.size, x, y);
                }
                else
                {

                    AddLandTile(position, ref tile, mMapGenerationPayload.size, x, y);


                }
                if (tile == null) continue;


                // Choose the first, most south available tile that is accessible to the user to walk on
                if (hasStart && tile.Type == EnvironmentTile.TileType.Accessible)
                {
                    Start = tile;
                    hasStart = false;
                }


            }
        }
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < mMapGenerationPayload.size.x; ++x)
        {
            for (int y = 0; y < mMapGenerationPayload.size.y; ++y)
            {
                SetupConnections(x, y);
            }
        }
    }

    public void SetupConnections(int x, int y)
    {
        if (mMap[x][y] == null) return;
        EnvironmentTile tile = mMap[x][y];
        tile.Connections = new List<EnvironmentTile>();

        // Calculate the mins and maxes for the possible touching tile coordinates
        int xMin = x - 1 < 0 ? 0 : x - 1;
        int yMin = y - 1 < 0 ? 0 : y - 1;
        int xMax = x + 1 < mMapGenerationPayload.size.x ? x + 1 : mMapGenerationPayload.size.x - 1;
        int yMax = y + 1 < mMapGenerationPayload.size.y ? y + 1 : mMapGenerationPayload.size.y - 1;
        
        for (int xa = xMin; xa <= xMax; xa++)
        {
            for (int ya = yMin; ya <= yMax; ya++)
            {
                // Stop tiles having connections to themselves
                if (ya == y && xa == x) continue;
                // Check all touching tile instances
                if ((xa == x || ya == y) && mMap[xa][ya] != null) 
                {
                    if (mMap[xa][ya].Type == EnvironmentTile.TileType.Accessible)
                        tile.Connections.Add(mMap[xa][ya]);
                }
                /*else// Check all diagonal tile instances
                {
                    // Make sure the diagonal tile is considered accessible
                    if (mMap[xa][ya] != null && mMap[xa][ya].Type == EnvironmentTile.TileType.Accessible)
                    {
                        // Calculate the offset from the tile we are checking for to the diagonal tiles
                        int xDif = x - xa; // Can be -1 or 1
                        int yDif = y - ya; // Can be -1 or 1
                        // Check the two touching tiles that are both touching the diagonal tile and the tile that we are checking
                        if ((mMap[xa + xDif][ya] != null && mMap[xa + xDif][ya].Type == EnvironmentTile.TileType.Accessible) ||
                            (mMap[xa][ya + yDif] != null && mMap[xa][ya + yDif].Type == EnvironmentTile.TileType.Accessible))
                        {
                            tile.Connections.Add(mMap[xa][ya]);
                        }
                    }
                }*/
            }
        }
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    private float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld(Character Character, GenerationPayload generationPayload, Game.SaveDataPacket saveData)
    {
        mMapGenerationPayload = generationPayload;
        GenerateWaterMap(saveData);
        Generate(saveData);
        SetupConnections();


        MovePlayerToStart(Character, mMap[saveData.PlayerX][saveData.PlayerY]);
        mCharacter = Character;

        // Load all entities
        for (int i = 0; i < saveData.Entities.Length; i++)
        {
            foreach (Entity e in EntityInstances)
            {
                if (saveData.Entities[i].N == e.entityName)
                {
                    EnvironmentTile tile = mMap[saveData.Entities[i].X][saveData.Entities[i].Y];
                    Entity ent = GameObject.Instantiate(e, Environment.instance.transform);
                    RegisterEntity(ent);
                    ent.transform.position = tile.Position;
                    ent.transform.rotation = Quaternion.identity;
                    ent.CurrentPosition = tile;
                    break;
                }
            }

        }
    }

    public void GenerateWorld(Character Character, GenerationPayload generationPayload)
    {
        mMapGenerationPayload = generationPayload;
        GenerateWaterMap();
        Generate();
        SetupConnections();
        MovePlayerToStart(Character, Start);
        mCharacter = Character;
    }

    public void Save(ref Game.SaveDataPacket saveData)
    {

        Vector2Int mapSize = mMapGenerationPayload.size;
        int mapTileCount = mapSize.x * mapSize.y;

        // World dimensions
        saveData.WorldWidth = mapSize.x;
        saveData.WorldHeight = mapSize.y;

        saveData.WaterMap = new bool[mapTileCount];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                saveData.WaterMap[x + (y * mapSize.x)] = mWaterMap[x, y];
            }
        }
        saveData.TileData = new Game.TileSaveData[mapTileCount];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                if (!mWaterMap[x, y] && mMap[x][y]!=null)
                {
                    saveData.TileData[x + (y * mapSize.x)] = new Game.TileSaveData(mMap[x][y].TileName, mMap[x][y].Rotation);
                }
                else
                {
                    saveData.TileData[x + (y * mapSize.x)] = new Game.TileSaveData("", 0);
                }
            }
        }
    }

    private void MovePlayerToStart(Character Character,EnvironmentTile tile)
    {
        Character.transform.position = tile.Position + new Vector3(0.0f, CharacterYOffset, 0.0f);
        Character.CurrentPosition = tile;
        Character.transform.rotation = Quaternion.identity;
        Character.transform.parent = this.transform;
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < mMapGenerationPayload.size.x; ++x)
            {
                for (int y = 0; y < mMapGenerationPayload.size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public EnvironmentTile GetTile(int x, int y)
    {
        return mMap[x][y];
    }

    public List<EnvironmentTile> SolveNeighbour(EnvironmentTile begin, EnvironmentTile destination)
    {
        foreach (EnvironmentTile childTile in destination.Connections)
        {
            if(childTile.PositionTile == begin.PositionTile)
            {
                return null;
            }
        }
        // Sort the connections array so that the closest one to the player will be first
        destination.Connections.Sort(((x, y) => (x.PositionTile - begin.PositionTile).magnitude.CompareTo((y.PositionTile - begin.PositionTile).magnitude)));

        // Loop through the connections, if we find one that we can path to, go to it
        foreach (EnvironmentTile childTile in destination.Connections)
        {
            List<EnvironmentTile> route = Solve(begin, childTile);
            // If there are nodes within the route, then we need to move to the node
            if (route!=null && route.Count > 0)
            {
                return route;
            }
        }
        return new List<EnvironmentTile>();
    }
    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination)
    {
        List<EnvironmentTile> result = null;
        if (begin != null && destination != null)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for( int count = 0; count < mAll.Count; ++count )
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.Type == EnvironmentTile.TileType.Accessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();
                    // Remove the start node as we are already there
                    result.RemoveAt(0);
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                //result.Add(begin);
                result.Add(destination);
            }
        }

        mLastSolution = result;

        return result;
    }

    public EnvironmentTile ReplaceEnviromentTile(EnvironmentTile current, EnvironmentTile replacment)
    {
        Vector3 newPosition = GetRawPosition(current.PositionTile.x,current.PositionTile.y);


        EnvironmentTile tile = Instantiate(replacment, new Vector3(), Quaternion.identity, transform);
        

        tile.Position = current.Position;
        tile.PositionTile = current.PositionTile;

        SetTileRotation(ref tile, current.Rotation);

        if (mCharacter.CurrentPosition == current)
        {
            mCharacter.CurrentPosition = tile;
        }

        // Attach the tint shader to all the tile blocks
        foreach (Material m in tile.GetComponent<MeshRenderer>().materials)
        {
            m.shader = TintShader;
        }

        // Add the tile to the global map
        mMap[current.PositionTile.x][current.PositionTile.y] = tile;

        // Setup connections between the new tile and its local tiles
        SetupConnections(current.PositionTile.x, current.PositionTile.y);
        if (current.PositionTile.x > 0)
            SetupConnections(current.PositionTile.x - 1, current.PositionTile.y);
        if (current.PositionTile.x < mMapGenerationPayload.size.x)
            SetupConnections(current.PositionTile.x + 1, current.PositionTile.y);
        if (current.PositionTile.y > 0)
            SetupConnections(current.PositionTile.x, current.PositionTile.y - 1);
        if (current.PositionTile.y < mMapGenerationPayload.size.y)
            SetupConnections(current.PositionTile.x, current.PositionTile.y + 1);


        // Remove the old tile and add the new one
        mAll.Remove(current);
        mAll.Add(tile);


        Destroy(current.gameObject);

        return tile;
    }
}
