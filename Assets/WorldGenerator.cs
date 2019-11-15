using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{

    [SerializeField] private Vector2Int PreviewSize;
    [SerializeField] private RawImage Image;

    private Texture2D mMapVisulisation;
    // Start is called before the first frame update
    void Start()
    {
        mMapVisulisation = new Texture2D(PreviewSize.x, PreviewSize.y);
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < 250; i++)
        {
            mMapVisulisation.SetPixel(i, i, Color.red);
        }
        Image.texture = mMapVisulisation;
    }
}
