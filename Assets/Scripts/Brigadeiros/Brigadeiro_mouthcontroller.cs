using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brigadeiro_mouthcontroller : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Layer 1 eh "brigadeiro"
        if (collision.gameObject.layer == 1)
        {
            animator.SetBool("eating", false);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //int count = Physics2D.OverlapCircleAll(transform.position,4).Length;
        //print(count);
        //Layer 1 eh "brigadeiro"
        if (collision.gameObject.layer == 1)
        {
            animator.SetBool("eating", true);
        }
    }
}
