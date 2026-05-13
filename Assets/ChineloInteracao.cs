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

    void Start()
    {
        minhaImagem = GetComponent<Image>();
        meuBotao = GetComponent<Button>();
        meuBotao.onClick.AddListener(AoClicarNoChinelo);
    }

    private void AoClicarNoChinelo()
    {
        if (estaVirado)
        {
            estaVirado = false;
            minhaImagem.sprite = spriteNormal;
        }
        else
        {
            estaVirado = true;
            minhaImagem.sprite = spriteVirado;
        }
    }
}