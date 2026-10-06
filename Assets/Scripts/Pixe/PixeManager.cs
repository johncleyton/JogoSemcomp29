using System;
using UnityEngine;

public class PixeManager : MinigameBase
{

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return Mathf.Min(15f - (faseAtual/10), 10f);
    }

    private void OnEnable()
    {
        PaintableCanvas.paintPercentageUpdated += HandlePaintUpdate;
    }

    private void OnDisable()
    {
        PaintableCanvas.paintPercentageUpdated -= HandlePaintUpdate;
    }

    // Chamada toda vez que atualiza a parte pintada
    private void HandlePaintUpdate(float currentPercentage)
    {
        Debug.Log($"Porcentagem: {currentPercentage * 100f}%");

        if(currentPercentage >= 0.5)
        {
            Vencer();
        }
    }
}
