using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static WorldGenerator;

public class MainMenu : MonoBehaviour
{
    // Used for cross Script Communication to define if the menu should be opened or closed
    public static bool MenuEnabled = true;

    [SerializeField] private Character Character = null;
    [SerializeField] private Transform CharacterStart = null;
    [SerializeField] private Canvas MainMenuInterface = null;
    [SerializeField] private Environment BackgroundEnviroment = null;

    [SerializeField] private float IslandStartRotation = 0.0f;
    [SerializeField] private float IslandRotationSpeed = 0.0f;

    [SerializeField] private float IslandAmplitude = 0.0f;
    [SerializeField] private float IslandFrequancy = 0.0f;
    [SerializeField] private float IslandWaterHeight = 0.0f;
    [SerializeField] private Vector2Int IslandSize = new Vector2Int();

    private GenerationPayload BackgroundGenerationPayload;

    private Character mCharacter;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Music-SFX", LoadSceneMode.Additive);

        mCharacter = Instantiate(Character, transform);
        mCharacter.transform.position = CharacterStart.position;
        mCharacter.transform.rotation = CharacterStart.rotation;
        MenuEnabled = true;
        BackgroundGenerationPayload = new GenerationPayload();
        BackgroundGenerationPayload.amplitude = IslandAmplitude;
        BackgroundGenerationPayload.frequancy = IslandFrequancy;
        BackgroundGenerationPayload.waterHeight = IslandWaterHeight;
        BackgroundGenerationPayload.size = IslandSize;

        BackgroundEnviroment.GenerateWorld(mCharacter, BackgroundGenerationPayload);
        BackgroundEnviroment.transform.RotateAround(BackgroundEnviroment.transform.position, transform.up, IslandStartRotation);
    }

    // Update is called once per frame
    void Update()
    {
        MainMenuInterface.enabled = MenuEnabled;
        BackgroundEnviroment.transform.RotateAround(BackgroundEnviroment.transform.position, transform.up, Time.deltaTime * IslandRotationSpeed);
    }

    public void NewGame()
    {
        SceneManager.LoadScene("WorldCreator", LoadSceneMode.Additive);
        MenuEnabled = false;
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
