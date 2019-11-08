using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{

    [Header("World Tiles")]
    [SerializeField] private List<EnvironmentTile> AccessibleTiles;
    [SerializeField] private List<EnvironmentTile> InaccessibleTiles;
    [Header("World Generation")]
    [SerializeField] private Vector2Int Size;
    [SerializeField] private float AccessiblePercentage;
    [System.Serializable]
    public struct WaterTile
    {
        public EnvironmentTile tile;

        // A array of 8 elements, starting from the North position of the model. it represents if at that position the tile
        // touches water and rotates in a clock wise order
        public bool[] Connectors;
    }
    [Header("Water Tile Configuration")]
    [SerializeField] private List<WaterTile> WaterTiles;

    public struct WaterTileSearchResult
    {
        public EnvironmentTile tile;
        public int rotation;
    }
    
    [Header("Sea Settings")]
    // How far can each inlet be between each other
    [SerializeField] private int SeaInletDistanceBetween;
    // How big can each sea inlet be
    [SerializeField] private int SeaInletMaxInletSize;

    private EnvironmentTile[][] mMap;
    private bool[][] mWaterMap;
    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;


    public EnvironmentTile Start { get; private set; }

    private void Awake()
    {
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
    }



    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
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
                    if ( !mMap[x][y].IsAccessible )
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
        mWaterMap = new bool[Size.x][];
        for (int x = 0; x < Size.x; ++x)
        {
            mWaterMap[x] = new bool[Size.y];

            for (int y = 0; y < Size.y; ++y)
            {
                mWaterMap[x][y] = false;
            }
        }

        // Generate a basic Sea
        {
            // Create the base layer that is the lower portion of the map and that should always be water, after that, there can be between 1-3 extra layers of sea
            // 1 + (1 -> 3 extra layers)
            int seaDepth = Random.Range(2, 5);
            for (int y = 0; y < seaDepth; y++)
            {
                for (int x = 0; x < Size.x; x++)
                {
                    mWaterMap[x][y] = true; // Set tile to be a part of the sea
                }
            }
            // Generate sea inlet
            {
                int r = 0;
                for (int xa = Random.Range(0, SeaInletDistanceBetween); xa < Size.x; xa += Random.Range(2, SeaInletDistanceBetween))
                {
                    r = Random.Range(2, SeaInletMaxInletSize);
                    for (int xb = xa; ((xb < xa + r) && (xb < Size.x)); xb++)
                        mWaterMap[xb][seaDepth] = true; // Set tile to be a part of the sea
                    xa += r;
                }
            }
        }

        /*int lastX = Size.y / 2;
        int lastChange = 0;
        // Generate a basic winding river
        for (int y = 0; y < Size.y - 1; ++y)
        {
            int newChange = Random.Range(-1, 2);
            int newX = lastX;
            if (newChange + lastChange == 0) 
            {
                lastChange = 0;
            }
            else
            {
                lastChange = newChange;
                newX += newChange;
            }
            if (lastX != newX)
            {
                mWaterMap[lastX][y] = true;
            }
            mWaterMap[newX][y] = true;
             
            lastX = newX;
        }*/




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
        bool xMax = x < Size.x - 1;
        bool yMax = y < Size.y - 1;

        // If each NWEW tile is in the map, check to see if they are water
        if (yMax) dataset[0] = mWaterMap[x][y + 1]; // N
        if (xMax) dataset[2] = mWaterMap[x + 1][y]; // E
        if (yMin) dataset[4] = mWaterMap[x][y - 1]; // S
        if (xMin) dataset[6] = mWaterMap[x - 1][y]; // W

        // Check to see if the corner blocks ar ein the scene, if thye are load there data
        if (xMax && yMax) dataset[1] = mWaterMap[x + 1][y + 1]; // NE
        if (xMax && yMin) dataset[3] = mWaterMap[x + 1][y - 1]; // SE
        if (xMin && yMin) dataset[5] = mWaterMap[x - 1][y - 1]; // SW
        if (xMin && yMax) dataset[7] = mWaterMap[x - 1][y + 1]; // NW

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

    private void Generate(Character Character)
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentTile[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3( -(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize) );
        bool start = true;

        for ( int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for ( int y = 0; y < Size.y; ++y)
            {
                bool isWater = mWaterMap[x][y];
                bool isAccessible = (start || Random.value < AccessiblePercentage) && !isWater;
                //List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;

                EnvironmentTile tile = null;
                if(isWater)
                {
                    List<EnvironmentTile> tiles = AccessibleTiles;
                    int rotation = 0;
                    EnvironmentTile prefab = null;
                    if (GetWaterTile(x, y, ref prefab, ref rotation)) 
                    {
                        Quaternion q = Quaternion.Euler(0, 90 * rotation, 0);
                        Vector3 positionOffset = new Vector3();

                        positionOffset.x += (rotation > 1 ? 10.0f : 0);
                        positionOffset.z += (rotation >= 1 && rotation < 3 ? 10.0f : 0);

                        tile = Instantiate(prefab, position + positionOffset, q, transform);
                    }
                    else
                    {
                        Debug.LogError("Could not find water tile to fit senario");
                    }
                }
                else
                {
                    List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles; ;
                    EnvironmentTile prefab = tiles[Random.Range(0, tiles.Count)];
                    tile = Instantiate(prefab, position, Quaternion.identity, transform);
                }

                foreach (var component in tile.GetComponents<TileAction>())
                {
                    component.Character = Character;
                    component.Map = this;
                }

                tile.Position = new Vector3( position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.gameObject.name = string.Format("Tile({0},{1})", x, y);
                mMap[x][y] = tile;
                mAll.Add(tile);

                // Choose the first, most south available tile that is accessible to the user to walk on
                if(start && isAccessible)
                {
                    Start = tile;
                    start = false;
                }

                position.z += TileSize;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                // If the tile is not navigable, then there is no need to record what tiles it can reach
                if (!tile.IsAccessible) continue;
                tile.Connections = new List<EnvironmentTile>();
                if (x > 0)
                {
                    if(mMap[x - 1][y].IsAccessible)
                        tile.Connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    if (mMap[x + 1][y].IsAccessible)
                        tile.Connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    if (mMap[x][y - 1].IsAccessible)
                        tile.Connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    if (mMap[x][y + 1].IsAccessible)
                        tile.Connections.Add(mMap[x][y + 1]);
                }
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

    public void GenerateWorld(Character Character)
    {
        GenerateWaterMap();
        Generate(Character);
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
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

                            if (!neighbour.Visited && neighbour.IsAccessible)
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

                    Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found");
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        mLastSolution = result;

        return result;
    }
}
