using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavaloManager : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    private void OnBecameInvisible()
    {
        Debug.Log("Perdeu Otario");
        Destroy(gameObject);
    }

}
