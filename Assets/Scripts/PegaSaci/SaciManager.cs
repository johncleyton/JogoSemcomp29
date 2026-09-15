using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaciBoss: MinigameBase
{

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer peneira;
    private bool janelaDeParry = false;
    private bool jogandoPeneira = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(SaciCoroutine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && janelaDeParry)
        {
            Debug.Log("acertou!");
            jogandoPeneira = true;
            VencerComAtraso(1.5f);
        }

        if (jogandoPeneira)
        {
            peneira.transform.position = Vector3.MoveTowards(peneira.transform.position, spriteRenderer.transform.position, 15f * Time.deltaTime);
        }
    }

    IEnumerator SaciCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        spriteRenderer.color = Color.gray;
        yield return new WaitForSeconds(1f);
        spriteRenderer.color = Color.red;
        janelaDeParry = true;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = new Color(176f/255f, 82f/255f, 0f);
        janelaDeParry = false;
    }
}