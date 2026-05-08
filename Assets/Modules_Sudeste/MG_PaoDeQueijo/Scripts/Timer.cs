using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{


    private float currentTime = 0f;
    private bool isTimeRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        isTimeRunning = true;
    }


    void Stop()
    {
        isTimeRunning = false;
    }


    void ResetTimer()
    {
        currentTime = 0f;
    }


    public float GetCurrentTimer()
    {
        return currentTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(isTimeRunning)
            currentTime += Time.deltaTime;
    }
}
