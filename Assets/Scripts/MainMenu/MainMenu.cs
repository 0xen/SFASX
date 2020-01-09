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

    [SerializeField] private GameObject NewGamePanel = null;

    // Settings
    [SerializeField] private GameObject SettingsPanel = null;
    [SerializeField] private TMP_Dropdown GraphicsQualityDropdown = null;
    [SerializeField] private Slider MainMusicSlider = null;
    [SerializeField] private Slider UISlider = null;

    [SerializeField] private AudioMixer MusicMixer = null;
    [SerializeField] private AudioMixer UIMixer = null;



    private GenerationPayload BackgroundGenerationPayload;

    private Character mCharacter;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Music-SFX", LoadSceneMode.Additive);

        mCharacter = Instantiate(Character, transform);
        mCharacter.transform.position = CharacterStart.position;
        mCharacter.transform.rotation = CharacterStart.rotation;
        BackgroundGenerationPayload = new GenerationPayload();
        BackgroundGenerationPayload.amplitude = IslandAmplitude;
        BackgroundGenerationPayload.frequancy = IslandFrequancy;
        BackgroundGenerationPayload.waterHeight = IslandWaterHeight;
        BackgroundGenerationPayload.size = IslandSize;

        BackgroundEnviroment.GenerateWorld(mCharacter, BackgroundGenerationPayload);
        BackgroundEnviroment.transform.RotateAround(BackgroundEnviroment.transform.position, transform.up, IslandStartRotation);

        // setup settings
        {

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

    public void MusicSettingSliderchanged()
    {
        MusicMixer.SetFloat("MusicVol", MainMusicSlider.value);
    }

    public void UISettingSliderchanged()
    {
        UIMixer.SetFloat("UIVol", UISlider.value);
    }

    public void GraphicsQualityChanged()
    {
        QualitySettings.SetQualityLevel(GraphicsQualityDropdown.value);
    }

    // Update is called once per frame
    void Update()
    {
        BackgroundEnviroment.transform.RotateAround(BackgroundEnviroment.transform.position, transform.up, Time.deltaTime * IslandRotationSpeed);
    }

    public void ToggleNewGame()
    {
        SettingsPanel.SetActive(false);
        NewGamePanel.SetActive(!NewGamePanel.activeSelf);
    }

    public void SettingsToggle()
    {
        NewGamePanel.SetActive(false);
        SettingsPanel.SetActive(!SettingsPanel.activeSelf);
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
