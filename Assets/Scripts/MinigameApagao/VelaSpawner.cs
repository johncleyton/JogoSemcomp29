using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class VelaSpawner : MonoBehaviour
{
    public GameObject vela;
    public int x, y, qtde;
    public float tamX, tamY;
    // Start is called before the first frame update
    void Start()
    {
        tamX = vela.GetComponent<SpriteRenderer>().bounds.size.x;
        tamY = vela.GetComponent<SpriteRenderer>().bounds.size.y;
        spawnVelas();
    }

    void spawnVelas()
    {
        for (int i = 0; i < qtde; i++)
        {
            float posicaoX = Random.Range(tamX*2, Camera.main.pixelWidth-tamX*2);
            float posicaoY = Random.Range(tamY*2, Camera.main.pixelHeight-tamY*2);
            Instantiate(vela, Camera.main.ScreenToWorldPoint(new Vector3(posicaoX, posicaoY, 10f)), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            spawnVelas();
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(0, 0, 0), new Vector3(x, y, 0));
    }
}
