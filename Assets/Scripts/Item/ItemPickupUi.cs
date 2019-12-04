using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPickupUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private Image image;

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
