using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScriptCarta : MonoBehaviour
{
    public string valor = "";
    public int naipe = 0;
    public string nomeNaipe = "";
    public TextMeshProUGUI textoCarta;
    public Image imagemCarta;
    public Sprite[] sprites;
    public bool ehZap;
    // Start is called before the first frame update
    void Start()
    {
        if (!ehZap)
        {
            int random;
            random = Random.Range(1, 11);
            Debug.Log("Random da carta: " + gameObject + " = " + random);
            switch (random)
            {
                case 1:
                    valor = "A";
                    break;
                case 8:
                    valor = "Q";
                    break;
                case 9:
                    valor = "J";
                    break;
                case 10:
                    valor = "K";
                    break;
                default:
                    valor = random.ToString();
                    break;
            }
            naipe = Random.Range(0, sprites.Length);
            switch (naipe)
            {
                case 0:
                    nomeNaipe = "paus";
                    break;
                case 1:
                    nomeNaipe = "ouros";
                    break;
                case 2:
                    nomeNaipe = "espadas";
                    break;
                case 3:
                    nomeNaipe = "copas";
                    break;
            }
            imagemCarta.sprite = sprites[naipe];
            textoCarta.text = valor;
            if ((naipe + 1) % 2 == 0)
                textoCarta.color = Color.red;
        }
    }

    public void cliqueCarta()
    {
        if (ehZap)
            Debug.Log("Vitória ao clicar na carta: " + valor + " de " + nomeNaipe);
        else
            Debug.Log("Derrota ao clicar na carta: " + valor + " de " + nomeNaipe);
    }
}
