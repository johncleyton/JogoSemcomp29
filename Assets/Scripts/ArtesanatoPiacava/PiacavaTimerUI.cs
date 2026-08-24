using UnityEngine;
using UnityEngine.UI;

// Círculo de tempo exibido no canto superior direito da tela.
// Espera uma Image (UI) do tipo "Filled" / "Radial 360".
public class PiacavaTimerUI : MonoBehaviour
{
    public Image imagemPreenchimento;
    public Gradient corPorTempo;

    public void DefinirProgresso(float progresso01)
    {
        if (imagemPreenchimento == null) return;

        progresso01 = Mathf.Clamp01(progresso01);
        imagemPreenchimento.fillAmount = progresso01;

        if (corPorTempo != null)
            imagemPreenchimento.color = corPorTempo.Evaluate(1f - progresso01);
    }
}
