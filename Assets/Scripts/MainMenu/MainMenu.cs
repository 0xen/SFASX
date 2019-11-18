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

    private GenerationPayload BackgroundGenerationPayload;

    private Character mCharacter;

    // Start is called before the first frame update
    void Start()
    {
        mCharacter = Instantiate(Character, transform);
        mCharacter.transform.position = CharacterStart.position;
        mCharacter.transform.rotation = CharacterStart.rotation;
        MenuEnabled = true;
        BackgroundGenerationPayload = new GenerationPayload();
        BackgroundGenerationPayload.amplitude = 4.142858f;
        BackgroundGenerationPayload.frequancy = 10.0f;
        BackgroundGenerationPayload.waterHeight = 1.435714f;
        BackgroundGenerationPayload.size = new Vector2Int(50, 50);
        BackgroundEnviroment.GenerateWorld(mCharacter, BackgroundGenerationPayload);
    }

    // Update is called once per frame
    void Update()
    {
        MainMenuInterface.enabled = MenuEnabled;
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
