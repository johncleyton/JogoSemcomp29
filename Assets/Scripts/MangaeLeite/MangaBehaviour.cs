using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangaBehaviour : MonoBehaviour
{
    private MangaManager manager;


    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<MangaManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "CopoLeite")
        {
            if(manager != null)
            {
                manager.Perder();
            }
            Destroy(gameObject);
        }

        else if(collision.gameObject.name == "Bottom")
        {
            Destroy(gameObject);
        }
    }
}
