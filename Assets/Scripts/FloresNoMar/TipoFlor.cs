using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipoFlor : MonoBehaviour
{
    public Sprite[] florSprite;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.GetComponent<SpriteRenderer>().sprite = florSprite[Random.Range(0, florSprite.Length)];
    }
}
