using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPickupUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh = null;
    [SerializeField] private Image image = null;

    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void SetupUI(string text, Sprite sprite)
    {
        textMesh.text = text;
        image.sprite = sprite;
    }
}
