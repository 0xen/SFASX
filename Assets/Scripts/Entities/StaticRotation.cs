using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticRotation : MonoBehaviour
{
    [SerializeField] private float RotationSpeed = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, 0.0f, RotationSpeed * Time.deltaTime), Space.Self);
    }
}
