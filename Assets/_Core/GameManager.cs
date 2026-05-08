using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float timer = 0;
    int sceneCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        sceneCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log(sceneCount);
        timer = GlobalVariables.timer;
        Debug.Log(timer);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            //Debug.Log(timer);
        }
        else
        {
            int random = Random.Range(0, sceneCount);
            GlobalVariables.timer = Mathf.Max((float)(GlobalVariables.timer - 0.1), (float)1.0);
            SceneManager.LoadScene(random);
        }
    }
}
