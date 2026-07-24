using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    private Vector2 inicio;
    private Vector2 fim;
    public float arrastoMinimo = 50f;
    public GameObject[] lanes;
    int laneAtual;

    // Start is called before the first frame update
    void Start()
    {
        laneAtual = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            inicio = Input.mousePosition;
        else if (Input.GetMouseButtonUp(0))
        {
            fim = Input.mousePosition;
            checarArrasto();
        }

        /*if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                inicio = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                fim = touch.position;
                checarArrasto();
            }
        }*/
    }

    void checarArrasto()
    {
        Vector2 swipeVector = fim - inicio;
        float swipeDistance = swipeVector.magnitude;

        if (swipeDistance > arrastoMinimo)
        {
            Vector2 direction = swipeVector.normalized;
            if (direction.y > 0)
            {
                if (laneAtual != 0)
                {
                    Vector3 posAtual = gameObject.transform.position;
                    posAtual.y = lanes[laneAtual - 1].transform.position.y;
                    transform.position = posAtual;
                    laneAtual -= 1;
                }
            }
            else
            {
                if (laneAtual != 2)
                {
                    Vector3 posAtual = gameObject.transform.position;
                    posAtual.y = lanes[laneAtual + 1].transform.position.y;
                    transform.position = posAtual;
                    laneAtual += 1;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7)
        {
            Debug.Log("Colisão com cavalo");
            Destroy(collision.gameObject);
        }
    }
}
