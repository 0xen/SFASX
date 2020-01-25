using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

using static WorldGenerator;

public class MainMenu : MonoBehaviour
{
    // the character that is on the main menu screen
    [SerializeField] private Character Character = null;
    // Start position on the main menu screen
    [SerializeField] private Transform CharacterStart = null;
    // Environment instance
    [SerializeField] private Environment BackgroundEnviroment = null;

    // The rotation that the island starts at
    [SerializeField] private float IslandStartRotation = 0.0f;
    // How fast should the island rotate in degrees
    [SerializeField] private float IslandRotationSpeed = 0.0f;

    // Preview island generation settings
    [SerializeField] private float IslandAmplitude = 0.0f;
    [SerializeField] private float IslandFrequancy = 0.0f;
    [SerializeField] private float IslandWaterHeight = 0.0f;
    [SerializeField] private Vector2Int IslandSize = new Vector2Int();


    // UI panel for selecting game settings
    [SerializeField] private GameObject NewGamePanel = null;

    // Settings
    [SerializeField] private GameObject SettingsPanel = null;
    [SerializeField] private TMP_Dropdown GraphicsQualityDropdown = null;
    [SerializeField] private Slider MainMusicSlider = null;
    [SerializeField] private Slider UISlider = null;
    // Music mixer settings
    [SerializeField] private AudioMixer MusicMixer = null;
    [SerializeField] private AudioMixer UIMixer = null;


    // Generation payload for the example map
    private GenerationPayload BackgroundGenerationPayload;

    private Character mCharacter;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Music-SFX", LoadSceneMode.Additive);

        // Init the music levels
        {
            MainMusicSlider.value = PlayerPrefs.GetFloat("MainVol", -20);
            MusicMixer.SetFloat("MainVol", MainMusicSlider.value);

            UISlider.value = PlayerPrefs.GetFloat("UIVol", -20);
            UIMixer.SetFloat("UIVol", UISlider.value);

            MusicMixer.SetFloat("NightTimeMusicVol", -80);
            MusicMixer.SetFloat("MusicVol", 0);
        }

        // Generate a example game scene
        {
            mCharacter = Instantiate(Character, transform);
            mCharacter.transform.position = CharacterStart.position;
            mCharacter.transform.rotation = CharacterStart.rotation;
            BackgroundGenerationPayload = new GenerationPayload();
            // Define the generation settings
            BackgroundGenerationPayload.amplitude = IslandAmplitude;
            BackgroundGenerationPayload.frequancy = IslandFrequancy;
            BackgroundGenerationPayload.waterHeight = IslandWaterHeight;
            BackgroundGenerationPayload.size = IslandSize;

            // Finally generate the world
            BackgroundEnviroment.GenerateWorld(mCharacter, BackgroundGenerationPayload);
            BackgroundEnviroment.transform.RotateAround(BackgroundEnviroment.transform.position, transform.up, IslandStartRotation);
        }

        // Setup settings
        {
            // Loop through all the graphics settings and add them to the settings panel
            string[] names = QualitySettings.names;
            GraphicsQualityDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> qualityOptions = new List<TMP_Dropdown.OptionData>();
            int currentQuality = QualitySettings.GetQualityLevel();
            foreach (string qualityName in names)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(qualityName);
                qualityOptions.Add(optionData);
            }
            GraphicsQualityDropdown.AddOptions(qualityOptions);
            GraphicsQualityDropdown.value = currentQuality;
        }
    }

    // Call on music slider change
    public void MusicSettingSliderchanged()
    {
        MusicMixer.SetFloat("MainVol", MainMusicSlider.value);
        PlayerPrefs.SetFloat("MainVol", MainMusicSlider.value);
    }

    // Call on ui vol slider change
    public void UISettingSliderchanged()
    {
        UIMixer.SetFloat("UIVol", UISlider.value);
        PlayerPrefs.SetFloat("UIVol", UISlider.value);
    }

    // Call on graphics quality change
    public void GraphicsQualityChanged()
    {
        QualitySettings.SetQualityLevel(GraphicsQualityDropdown.value);
    }

    // Update is called once per frame
    void Update()
    {
        BackgroundEnviroment.transform.RotateAround(BackgroundEnviroment.transform.position, transform.up, Time.deltaTime * IslandRotationSpeed);
    }

    // Toggle the new game generation panel open and closed
    public void ToggleNewGame()
    {
        SettingsPanel.SetActive(false);
        NewGamePanel.SetActive(!NewGamePanel.activeSelf);
    }

    // Toggle the settings menu open and closed
    public void SettingsToggle()
    {
        NewGamePanel.SetActive(false);
        SettingsPanel.SetActive(!SettingsPanel.activeSelf);
    }

    // Exit the game
    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
