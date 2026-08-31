using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public List<GameObject> prefabs = new List<GameObject>();

    private void Awake()
    {
        int index = Random.Range(0, prefabs.Count);
        Instantiate(prefabs[index]);
    }

}
