using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class comida : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    private Rigidbody2D rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //quando tempoatual eh 3, a gravidade sera 3.37
        //quando tempoatual eh 7, eh 1.3554^0=1
        rb.gravityScale = Mathf.Pow(1.3554f,(7 - GameManagerRework.Instance.tempoDoMinigameAtual));
        //Debug.Log("gravidade: "+rb.gravityScale);

        //escolhe o sprite da comida aleatoriamente
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
    }

}
