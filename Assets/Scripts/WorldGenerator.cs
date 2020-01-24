using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{
    // Size of the preview image
    [SerializeField] private Vector2Int PreviewSize;
    // Reference to the preview image
    [SerializeField] private RawImage Image = null;

    // All the generation variables needed for a new map
    public class GenerationPayload
    {
        // Loading the map from a file?
        [SerializeField] public bool loadFromFile;
        // If we are, whats its path
        [SerializeField] public string loadPath;
        // Size of the map (X,Y)
        [SerializeField] public Vector2Int size;
        // Frequency of the perlin noise
        [SerializeField] public float frequancy;
        // Amplitude of the perlin noise
        [SerializeField] public float amplitude;
        // Perlin noise generated values from 0-1, at what value dose water start? 0-X(Water), X-1(Land)
        [SerializeField] public float waterHeight;
    }

    // Define various random generation settings for the noise generator
    [SerializeField] private Slider IslandFrequancy = null;
    [SerializeField] private Slider IslandAmplitude = null;
    [SerializeField] private Slider WaterHeight = null;
    [SerializeField] private Slider MapSize = null;

    // Label on the ui that represents the maps size
    [SerializeField] private Text MapSizeLabel = null;

    // What are the increments that the map is scaled by?
    [SerializeField] private int MapSizeScalar = 0;

    // Local generation payload reference, when we start the game, this is passed to the Environment for generation
    private GenerationPayload MapGenerationPayload = null;
    // Array that represents what tiles are water and what tiles are land (Water=true)
    private bool[,] mMapData = null;

    // Local map texture instance
    private Texture2D mMapVisulisation = null;

    // Start is called before the first frame update
    void Start()
    {
        mMapVisulisation = new Texture2D(PreviewSize.x, PreviewSize.y);
        MapGenerationPayload = new GenerationPayload();
        MapGenerationPayload.size = PreviewSize;
        RebuildImageMap();
    }

    // When the UI has been changed, we rebuild the UI elements and map generation payload
    public void RebuildGenerationPayloadData()
    {
        int mapSize = MapSizeScalar * (int)MapSize.value;
        MapSizeLabel.text = "(" + mapSize + "x" + mapSize + ")";
        MapGenerationPayload.size.x = MapGenerationPayload.size.y = mapSize;
        MapGenerationPayload.frequancy = IslandFrequancy.value;
        MapGenerationPayload.amplitude = IslandAmplitude.value;
        MapGenerationPayload.waterHeight = WaterHeight.value;
    }

    // Using the generation settings, rebuild the UI preview
    public void RebuildImageMap()
    {
        // Make sure we have the latest UI settings
        RebuildGenerationPayloadData();

        // Get the image preview size and temporarily pass it to the map generator
        Vector2Int tempSize = MapGenerationPayload.size;
        MapGenerationPayload.size = PreviewSize;

        // Load the water map using the current settings
        mMapData = GenerateWaterMap(MapGenerationPayload);

        // Pass the original payload size back to the map payload
        MapGenerationPayload.size = tempSize;
        
        // Loop through and update each pixel on the image map with the new water map
        for (int i = 0; i < PreviewSize.x; i++)
        {
            for (int j = 0; j < PreviewSize.y; j++)
            {
                mMapVisulisation.SetPixel(i, j, mMapData[i, j] ? Color.blue : Color.green);
            }
        }

        // Apply the new image texture
        mMapVisulisation.Apply();
        Image.texture = mMapVisulisation;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public static bool[,] GenerateWaterMap(GenerationPayload MapGenerationPayload)
    {
        // Create the return array
        bool[,] mapData = new bool[MapGenerationPayload.size.x,MapGenerationPayload.size.y];

        Vector2Int Size = MapGenerationPayload.size;


        // Generate water map
        float perTileRadX = 3.14159f / Size.x;
        float perTileRadY = 3.14159f / Size.y;
        float[,] heightMap = new float[Size.x, Size.y];

        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                // Calculate x and y coordinates based on the maps size / frequency
                float xCoord = ((float)x / (float)Size.x) * MapGenerationPayload.frequancy;
                float yCoord = ((float)y / (float)Size.y) * MapGenerationPayload.frequancy;
                // Generate the height map for the tile using the perlin noise function
                heightMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord) + ((Mathf.Sin(perTileRadX * x) + Mathf.Sin(perTileRadY * y)) * MapGenerationPayload.amplitude);
            }
        }
        // calculate the average tile height
        float average = 0.0f;
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                average += heightMap[x, y];
            }
        }
        // Normalize the average to a range of 0-1
        average /= (Size.x * Size.y);
        average *= MapGenerationPayload.waterHeight;

        // Generate the final water map, checking to see if the height map is less then the calculated average
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                mapData[x, y] = heightMap[x, y] < average;
            }
        }

        return mapData;
    }


    public void CreateWorld()
    {
        Game.MapGenerationPayload = MapGenerationPayload;
        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene("Main", LoadSceneMode.Additive);
    }
}
