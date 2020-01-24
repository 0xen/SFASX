using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // The character the camera is attached too
    public Character Character;
    // Is the camera to follow the character
    private bool mFollowCharacter;

    // How far can the character get of screen before the camera scrolls
    public Vector2 ScreenBounds;
    // Scroll speed of the camera
    public Vector2 CameraScrollSpeed;

    // Start is called before the first frame update
    void Start()
    {
        mFollowCharacter = false;
    }

    // Set the camera following the character
    public void SetFollowing(bool following)
    {
        mFollowCharacter = following;
    }

    // Update is called once per frame
    void Update()
    {
        // Are we following the player
        if (mFollowCharacter)
        {
            // Where the position is on the screen
            Vector3 rawScreenPos = Camera.main.WorldToScreenPoint(Character.transform.position);
            // Move the camera to the player if it is fully out of bounds
            if(rawScreenPos.x > Screen.width || rawScreenPos.z > Screen.height || rawScreenPos.x < 0 || rawScreenPos.z < 0)
            {
                this.transform.position = Character.transform.position;
            }

            // Get the screen halfway point
            Vector3 screenPos = rawScreenPos;
            screenPos.x -= Screen.width / 2;
            screenPos.y -= Screen.height / 2;

            // If the camera is slightly off center from the player on the x axis, move it back
            if (screenPos.x > (Screen.width / 2) * ScreenBounds.x)
            {
                this.transform.position += new Vector3(CameraScrollSpeed.x, 0, 0);
            }
            else
            if (screenPos.x < -((Screen.width / 2) * ScreenBounds.x))
            {
                this.transform.position += new Vector3(-CameraScrollSpeed.x, 0, 0);
            }

            // If the camera is slightly off center from the player on the y axis, move it back
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
