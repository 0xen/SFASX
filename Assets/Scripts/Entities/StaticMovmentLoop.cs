using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMovmentLoop : MonoBehaviour
{
    // Movement speed of the object
    [SerializeField] private float speed = 0.0f;
    // Max distance that can me moved
    [SerializeField] private float flyDistance = 0.0f;
    [SerializeField] private Vector3 startPosition = new Vector3();
    [SerializeField] private Vector3 startEulerRotation = new Vector3();

    // Delta moment time
    private float currentFlyTime = 0.0f;
    // Current object speed
    private float currentSpeed = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        currentFlyTime = (flyDistance / speed) / 60.0f;
        this.transform.localPosition = startPosition;
        this.transform.localEulerAngles = startEulerRotation;
        currentSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        currentFlyTime -= Time.deltaTime;

        this.transform.localPosition += new Vector3(0, 0, currentSpeed);

        if(currentFlyTime<0)
        {
            this.transform.Rotate(new Vector3(0, 180.0f, 0), Space.Self);
            currentFlyTime = (flyDistance / speed) / 60.0f;
            currentSpeed *= -1.0f;
        }
    }
}
