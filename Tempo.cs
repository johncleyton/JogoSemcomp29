using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tempo : MonoBehaviour
{
    public float initialTimer = 7f;
    public bool ActiveTimer = true;
    public string Derrota;

    private void Update()
    {
        if (ActiveTimer)
        {
            if(initialTimer > 0)
            {
                initialTimer -= Time.deltaTime;
            }
            else
            {
                SceneManager.LoadScene(Derrota);
                initialTimer = 0;
                ActiveTimer = false;
            }
        }
    }
}
