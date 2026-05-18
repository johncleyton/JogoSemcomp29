using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ChineloInteracao : MonoBehaviour
{
    [Header("Configuração de Estado")]
    public bool estaVirado;

    [Header("Artes do Chinelo")]
    public Sprite spriteNormal;
    public Sprite spriteVirado;

    private Image minhaImagem;
    private Button meuBotao;

    private UIShake efeitoTremor;

    private bool stunado;

    void Start()
    {
        minhaImagem = GetComponent<Image>();
        meuBotao = GetComponent<Button>();
        meuBotao.onClick.AddListener(AoClicarNoChinelo);


        efeitoTremor = Object.FindFirstObjectByType<UIShake>();

        if (efeitoTremor == null)
        {
            Debug.LogWarning("O script UIShake não foi encontrado na cena!");
        }
    }

    private void AoClicarNoChinelo()
    {
        if (stunado) return;

        if (estaVirado)
        {
            estaVirado = false;
            minhaImagem.sprite = spriteNormal;
        }
        else
        {
            estaVirado = true;
            minhaImagem.sprite = spriteVirado;

            if (efeitoTremor != null)
            {
                StartCoroutine(efeitoTremor.Shake(0.2f, 20f));
                StartCoroutine(StunDoErro(2f));
            }
        }
    }

    private IEnumerator StunDoErro(float tempo)
    {
        stunado = true;
        yield return new WaitForSecondsRealtime(tempo);
        stunado = false;
    }
}