using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cartas : MonoBehaviour
{
    public string Derrota;
    void OnMouseDown()
    {
        SceneManager.LoadScene(Derrota, LoadSceneMode.Single);
    }
}
