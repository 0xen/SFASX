using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMovmentLoop : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float flyDistance;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 startEulerRotation;

    private float currentFlyTime;
    private float currentSpeed;

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
