using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InGameMenuHandler : MonoBehaviour
{
    // The current game instance
    [SerializeField] private Game GameInstance = null;
    [SerializeField] private GameObject MenuPanel = null;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If the mouse has been clicked, close the menu and not clicked on ui element
        if(Input.GetMouseButtonDown(0) && MenuPanel.activeSelf && !EventSystem.current.IsPointerOverGameObject())
        {
            ToggleMenu();
        }
    }

    // Toggle the in-game menu open and closed
    public void ToggleMenu()
    {
        MenuPanel.SetActive(!MenuPanel.activeSelf);
    }

    // Save button. Save the game and close the menu
    public void Save()
    {
        GameInstance.Save();
        ToggleMenu();
    }

    // Close the game and return to the main menu
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
