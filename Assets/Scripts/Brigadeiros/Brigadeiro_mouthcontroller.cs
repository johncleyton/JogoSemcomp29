using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brigadeiro_mouthcontroller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject Mouth;

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Layer 1 eh "brigadeiro"
        if (collision.gameObject.layer == 1)
        {
            Mouth.gameObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Debug.Log("Saiu");
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        //int count = Physics2D.OverlapCircleAll(transform.position,4).Length;
        //print(count);
        //Layer 1 eh "brigadeiro"
        if (collision.gameObject.layer == 1)
        {
            Mouth.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
