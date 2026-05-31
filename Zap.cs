using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Zap : MonoBehaviour
{
    public string Vitoria;
    void OnMouseDown()
    {
        SceneManager.LoadScene(Vitoria, LoadSceneMode.Single);
    }
}
