using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirarCarne : MinigameBase
{
    public RectTransform bar;
    public RectTransform indicator;
    public RectTransform hitzone;

    public float speed = 100f;
    private bool movingUp = true;

    void Update()
    {
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

        pos.y += speed * dir * Time.deltaTime;
        
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
            Vencer();
        }
        else
        {
            Perder();
        }
    }
}
