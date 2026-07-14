using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScissorsTime : MonoBehaviour
{

    private Transform startPoint;

    private Transform cutPoint;

    private float timeLimit = 10f;

    private ShakeController grapeController;

    private float elapsedTime = 0f;

    private bool active = true;
    // Update is called once per frame
    void Update()
    {
        if(!active) return;

        elapsedTime += Time.deltaTime;

        float t = elapsedTime / timeLimit;


        transform.position = Vector3.Lerp(startPoint.position, cutPoint.position, t);

        if(t > 1.0f)
        {
            active = false;

            grapeController.TriggerLose();
            // dispara animação de corte
        }

    }

    // parece javascript!!1
    public void StopTimer() => active = false;
}
