using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PixeManager : MinigameBase
{
    public TMP_Text textoPorcentagem;
    [SerializeField] private float porcentagemVitoria;
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
        textoPorcentagem.text = $"{Mathf.Floor(currentPercentage * 100f)}% / {porcentagemVitoria * 100f}%";

        if(currentPercentage >= porcentagemVitoria)
        {
            Vencer();
        }
    }
}
