using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int PreviewSize;
    [SerializeField] private RawImage Image = null;
    public class GenerationPayload
    {
        [SerializeField] public bool loadFromFile;
        [SerializeField] public string loadPath;
        [SerializeField] public Vector2Int size;
        [SerializeField] public float frequancy;
        [SerializeField] public float amplitude;
        [SerializeField] public float waterHeight;
    }

    // Define various random generation settings for the noise generator
    [SerializeField] private Slider IslandFrequancy = null;
    [SerializeField] private Slider IslandAmplitude = null;
    [SerializeField] private Slider WaterHeight = null;
    [SerializeField] private Slider MapSize = null;

    [SerializeField] private Text MapSizeLabel = null;

    [SerializeField] private int MapSizeScalar = 0;

    private GenerationPayload MapGenerationPayload = null;
    private bool[,] mMapData = null;

    private Texture2D mMapVisulisation = null;
    // Start is called before the first frame update
    void Start()
    {
        mMapVisulisation = new Texture2D(PreviewSize.x, PreviewSize.y);
        MapGenerationPayload = new GenerationPayload();
        MapGenerationPayload.size = PreviewSize;
        RebuildImageMap();
    }

    public void RebuildGenerationPayloadData()
    {
        int mapSize = MapSizeScalar * (int)MapSize.value;
        MapSizeLabel.text = "(" + mapSize + "x" + mapSize + ")";
        MapGenerationPayload.size.x = MapGenerationPayload.size.y = mapSize;
        MapGenerationPayload.frequancy = IslandFrequancy.value;
        MapGenerationPayload.amplitude = IslandAmplitude.value;
        MapGenerationPayload.waterHeight = WaterHeight.value;
    }

    public void RebuildImageMap()
    {
        RebuildGenerationPayloadData();

        Vector2Int tempSize = MapGenerationPayload.size;
        MapGenerationPayload.size = PreviewSize;
        mMapData = GenerateWaterMap(MapGenerationPayload);
        MapGenerationPayload.size = tempSize;
        

        for (int i = 0; i < PreviewSize.x; i++)
        {
            for (int j = 0; j < PreviewSize.y; j++)
            {
                mMapVisulisation.SetPixel(i, j, mMapData[i, j] ? Color.blue : Color.green);
            }
        }
        mMapVisulisation.Apply();
        Image.texture = mMapVisulisation;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public static bool[,] GenerateWaterMap(GenerationPayload MapGenerationPayload)
    {
        // 
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
                float xCoord = /*islandWaveformOffset.x + */((float)x / (float)Size.x) * MapGenerationPayload.frequancy;
                float yCoord = /*islandWaveformOffset.y + */((float)y / (float)Size.y) * MapGenerationPayload.frequancy;
                // Generate the height map for the tile using the perlin noise function
                heightMap[x, y] = Mathf.PerlinNoise(xCoord, yCoord) + ((Mathf.Sin(perTileRadX * x) + Mathf.Sin(perTileRadY * y)) * MapGenerationPayload.amplitude);
            }
        }
        float average = 0.0f;
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                average += heightMap[x, y];
            }
        }
        average /= (Size.x * Size.y);
        average *= MapGenerationPayload.waterHeight;
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
