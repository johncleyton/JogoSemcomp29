using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableObject : MonoBehaviour
{

    public bool isCapivara;
    private bool AlreadyClicked = false;

    void OnMouseDown()
    {
        if(AlreadyClicked) return;

        AlreadyClicked = true;

        if (isCapivara)
        {
            GameManagerCapivara.Instance.CapivaraEncontrada();


            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            GameManagerCapivara.Instance.GameOver();
            GetComponent<SpriteRenderer>().color = Color.red;
        
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
