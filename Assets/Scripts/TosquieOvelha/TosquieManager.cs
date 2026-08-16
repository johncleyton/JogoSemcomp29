using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public enum AcaoTosa
{
    NaoTosar,
    TosarParcial,
    TosarNormal
}

public class TosquieManager : MonoBehaviour
{
    public static TosquieManager Instance;

    public GameObject ovelhaPrefab;
    public Transform centroTosaPoint;
    public Transform filaPoint; // onde a proxima ovelha aguarda
    public OvelhaController ovelhaAtual;
    public OvelhaController proxOvelha;

    public Text placarText;
    public Slider timerSlider;


    public int totalOvelhas = 6;
    public float tempoOvelha = 5f;


    private int ovelhasAtendidas = 0;
    private float tempoRestante;

    private bool gameOver = false;

    void Awake()
    {
      if(Instance == null) Instance = this;  
    }


    // Start is called before the first frame update
    void Start()
    {
        PrepararProxOvelha();
        AvancarFila();
        
    }

    // Update is called once per frame
    void Update()
    {
        if(gameOver || ovelhaAtual == null) return;


        tempoRestante -= Time.deltaTime;

        if(timerSlider != null)
        {
            timerSlider.value = tempoRestante / tempoOvelha;
        }


        if(tempoRestante <= 0)
        {
            FinalizarJogo(false, "Demorou muito para tosar!");
        }
    }

    private void PrepararProxOvelha()
    {
        GameObject novaOvelha = Instantiate(ovelhaPrefab, filaPoint.position, Quaternion.identity);

        proxOvelha = novaOvelha.GetComponent<OvelhaController>();


        // estado sortedo
        EstadoOvelha estadoSorteado = (EstadoOvelha)Random.Range(0,3);

        proxOvelha.Inicializar(estadoSorteado);
    }



    private void AvancarFila()
    {
        if(proxOvelha != null)
        {
            ovelhaAtual = proxOvelha;

            ovelhaAtual.transform.position = centroTosaPoint.position;

            PrepararProxOvelha();

            tempoRestante = tempoOvelha;

            AtualizarUI();
        }
    }

    public void ReceberAcaoJogador(int acaoIndex)
    {
        if(gameOver || ovelhaAtual == null) return;

        AcaoTosa acaoEscolhida = (AcaoTosa)acaoIndex;

        ValidarAcao(acaoEscolhida);
    }


private void ValidarAcao(AcaoTosa acaoEscolhida)
    {
        bool acaoCorreta = false;

        // Lógica de validação baseada no estado da ovelha
        switch (ovelhaAtual.estadoAtual)
        {
            case EstadoOvelha.Nova:
                acaoCorreta = (acaoEscolhida == AcaoTosa.NaoTosar);
                break;
            case EstadoOvelha.Velha:
                acaoCorreta = (acaoEscolhida == AcaoTosa.TosarParcial);
                break;
            case EstadoOvelha.Perfeita:
                acaoCorreta = (acaoEscolhida == AcaoTosa.TosarNormal);
                break;
        }

        if (acaoCorreta)
        {
            ovelhasAtendidas++;
            ovelhaAtual.SairDaTela();
            ovelhaAtual = null;

            if (ovelhasAtendidas >= totalOvelhas)
            {
                FinalizarJogo(true, "Todas as ovelhas foram tosadas!");
            }
            else
            {
                AvancarFila();
            }
        }
        else
        {
            FinalizarJogo(false, "Você errou a mão na tosa!");
        }
    }

    private void AtualizarUI()
    {
        if(placarText != null)
        {
            placarText.text = $"{ovelhasAtendidas}/{totalOvelhas}";
        }
    }

    private void FinalizarJogo(bool ganhou, string msg)
    {
        gameOver = true;
        Debug.Log(msg);

        // vitoria e derrota
    } 
}
