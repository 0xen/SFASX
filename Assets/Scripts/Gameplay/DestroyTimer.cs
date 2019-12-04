using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    [SerializeField] private float TimeBeforeDestroy;

    private float runningTime;

    // Start is called before the first frame update
    void Start()
    {
        runningTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        runningTime += Time.deltaTime;
        if(TimeBeforeDestroy < runningTime)
        {
            Destroy(this.gameObject);
        }
    }
}
