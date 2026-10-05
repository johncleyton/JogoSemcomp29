using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirarCarne : MinigameBase
{
    public RectTransform bar;
    public RectTransform indicator;
    public RectTransform hitzone;

    public SpriteRenderer carneRenderer; 
    public Sprite spriteCrua;
    public Sprite spriteAssada;
    public Sprite spriteQueimada;

    public float baseSpeed = 80f;
    private float currentSpeed;
    private bool movingUp = true;

    void Start()
    {
        if (currentSpeed == 0)
        {
            currentSpeed = baseSpeed; // Usa a velocidade base padrão
        }
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        currentSpeed = baseSpeed + faseAtual * 5;
        return tempoGlobalSugerido;
    }

    void Update()
    {
        if (jogoFinalizado) return;

        MoveIndicator();

        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }

     void MoveIndicator()
    {
        float topLimit = bar.rect.height/2;
        float bottomLimit = -bar.rect.height/2;

        Vector2 pos = indicator.anchoredPosition;
        float dir = movingUp ? 1f : -1f;

        pos.y += currentSpeed * dir * Time.deltaTime;
        
        if (pos.y >= topLimit)
        {
            pos.y = topLimit;
            movingUp = false;
        }
        else if (pos.y <= bottomLimit)
        {
            pos.y = bottomLimit;
            movingUp = true;
        }

        indicator.anchoredPosition = pos;
    }

    void CheckTiming()
    {
        float yIndicator = indicator.anchoredPosition.y;

        float min = hitzone.anchoredPosition.y - (hitzone.rect.height / 2);
        float max = hitzone.anchoredPosition.y + (hitzone.rect.height / 2);

        if (yIndicator >= min && yIndicator <= max)
        {
            if (carneRenderer != null && spriteAssada != null)
            {
                carneRenderer.sprite = spriteAssada;
            }
            VencerComAtraso(2f);
        }
        else
        {
            if (carneRenderer != null && spriteQueimada != null)
            {
                carneRenderer.sprite = spriteQueimada;
            }
            PerderComAtraso(2f);
        }
    }
}
