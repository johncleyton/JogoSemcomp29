using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScriptCarta : MinigameBase
{
    public string valor = "";
    public int naipe = 0;
    public string nomeNaipe = "";
    public TextMeshProUGUI[] textoCarta;
    public Image[] imagemCarta;
    public Sprite[] sprites;
    public bool ehZap;
    public bool freeze = false;
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
            if (random == 4 && naipe == 0)
                naipe = 1;
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
            for (int i = 0; i < 2; i++)
            {
                imagemCarta[i].sprite = sprites[naipe];
                textoCarta[i].text = valor;
                if ((naipe + 1) % 2 == 0)
                    textoCarta[i].color = Color.red;
            }

        }
    }

    public void cliqueCarta()
    {
        if (!freeze)
        {
            GameObject objeto = GameObject.Find("TextoTruco");
            ScriptCarta[] objetosCarta = FindObjectsOfType<ScriptCarta>();
            for (int i = 0; i < objetosCarta.Length; i++)
                objetosCarta[i].freeze = true;
            if (ehZap)
            {
                objeto.GetComponent<TMP_Text>().text = "TRUCO!!!";
                Vencer();
            }
            else
            {
                objeto.GetComponent<TMP_Text>().text = "Morreu com o zap na mão!";
                Perder();
            }
        }
    }
}
