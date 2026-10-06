using UnityEngine;

public class PixeManager : MinigameBase
{
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

        if(currentPercentage < 0.7)
        {
            Vencer();
        }
    }
}
