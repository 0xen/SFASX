using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InGameMenuHandler : MonoBehaviour
{

    [SerializeField] private Game GameInstance;
    [SerializeField] private GameObject MenuPanel;


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

    public void ToggleMenu()
    {
        MenuPanel.SetActive(!MenuPanel.activeSelf);
    }

    public void Save()
    {
        GameInstance.Save();
        ToggleMenu();
    }
}
