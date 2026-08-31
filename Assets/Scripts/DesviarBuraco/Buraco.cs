using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buraco : MonoBehaviour
{
    public float speed = 5f;
    public float downLimit; 

    void Start()
    {
        Camera cam = GetComponentInParent<Camera>(); 
        
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam != null)
        {
            float screenHeight = cam.orthographicSize;
            downLimit = cam.transform.position.y - screenHeight - 2f; 
        }
        else
        {
            downLimit = -18f; 
        }    

        downLimit = -18f;
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        //destruir buraco quando sai da tela
        if (transform.position.y < downLimit)
        {
            Destroy(gameObject);
        }
    }
}
