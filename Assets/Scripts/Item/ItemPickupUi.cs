using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPickupUi : MonoBehaviour
{
    // Text for the item that has just been picked up
    [SerializeField] private TextMeshProUGUI textMesh = null;
    // Image for the item that has just been picked up
    [SerializeField] private Image image = null;

    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    // Add the text and sprite to the ui
    public void SetupUI(string text, Sprite sprite)
    {
        textMesh.text = text;
        image.sprite = sprite;
    }
}
