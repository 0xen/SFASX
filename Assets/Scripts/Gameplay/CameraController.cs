using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Character Character;
    private bool mFollowCharacter;

    public Vector2 ScreenBounds;
    public Vector2 CameraScrollSpeed;

    // Start is called before the first frame update
    void Start()
    {
        mFollowCharacter = false;
    }

    public void SetFollowing(bool following)
    {
        mFollowCharacter = following;
    }

    // Update is called once per frame
    void Update()
    {
        if (mFollowCharacter)
        {

            Vector3 rawScreenPos = Camera.main.WorldToScreenPoint(Character.transform.position);
            if(rawScreenPos.x > Screen.width || rawScreenPos.z > Screen.height || rawScreenPos.x < 0 || rawScreenPos.z < 0)
            {
                this.transform.position = Character.transform.position;
            }

            Vector3 screenPos = rawScreenPos;
            screenPos.x -= Screen.width / 2;
            screenPos.y -= Screen.height / 2;

            if (screenPos.x > (Screen.width / 2) * ScreenBounds.x)
            {
                this.transform.position += new Vector3(CameraScrollSpeed.x, 0, 0);
            }
            else
            if (screenPos.x < -((Screen.width / 2) * ScreenBounds.x))
            {
                this.transform.position += new Vector3(-CameraScrollSpeed.x, 0, 0);
            }

            if (screenPos.y > (Screen.height / 2) * ScreenBounds.y)
            {
                this.transform.position += new Vector3(0, 0, CameraScrollSpeed.y);
            }
            else
            if (screenPos.y < -((Screen.height / 2) * ScreenBounds.y))
            {
                this.transform.position += new Vector3(0, 0, -CameraScrollSpeed.y);
            }


        }
    }
}
