using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenuContinueButton : MonoBehaviour
{
    [SerializeField] private string SavePath = "";

    private Button mButton;
    // Start is called before the first frame update
    void Start()
    {
        mButton = GetComponent<Button>();
        mButton.interactable = File.Exists(SavePath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Continue()
    {
        WorldGenerator.GenerationPayload MapGenerationPayload = new WorldGenerator.GenerationPayload();
        MapGenerationPayload.loadFromFile = true;
        MapGenerationPayload.loadPath = SavePath;
        Game.MapGenerationPayload = MapGenerationPayload;

        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene("Main", LoadSceneMode.Additive);
    }
}
