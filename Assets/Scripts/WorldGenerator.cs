using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int PreviewSize;
    [SerializeField] private RawImage Image;
    public class GenerationPayload
    {
        [SerializeField] public Vector2Int size;
        [SerializeField] public float frequancy;
        [SerializeField] public float amplitude;
        [SerializeField] public float waterHeight;
    }

    [SerializeField] private Slider IslandFrequancy;
    [SerializeField] private Slider IslandAmplitude;
    [SerializeField] private Slider WaterHeight;
    [SerializeField] private Slider MapSize;
    [SerializeField] private Text MapSizeLabel;

    [SerializeField] private int MapSizeScalar;

    private GenerationPayload MapGenerationPayload;
    private bool[,] mMapData;

    private Texture2D mMapVisulisation;
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
                float xCoord = /*islandWaveformOffset.x + */((float)x / (float)Size.x) * MapGenerationPayload.frequancy;
                float yCoord = /*islandWaveformOffset.y + */((float)y / (float)Size.y) * MapGenerationPayload.frequancy;
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

    void ReturnHome()
    {
        MainMenu.MenuEnabled = true;
        SceneManager.UnloadSceneAsync("WorldCreator");
    }

    public void CreateWorld()
    {
        Game.MapGenerationPayload = MapGenerationPayload;
        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.UnloadSceneAsync("WorldCreator");
        SceneManager.LoadScene("Main");
    }
}
