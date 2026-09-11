using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurupiraManager : MinigameBase
{

    public static CurupiraManager Instance;

    [Header("Referencias das Cena")]
    public List<CasaScript> casasCena;

    public CasaScript casaCorreta;


    void Awake()
    {
      if(Instance == null) Instance = this;  
    }

    // Start is called before the first frame update
    void Start()
    {
        SortearCasaCurupira();
    }

    public override float ConfigurarDificuldade(int faseAtual, float tempoGlobalSugerido)
    {
        return tempoGlobalSugerido;
    }

    private void SortearCasaCurupira()
    {
        if(casasCena == null || casasCena.Count == 0)
        {
            Debug.LogError("Nenhuma casa adicionada na cena");
            return;
        }

        int indexSorteado = Random.Range(0, casasCena.Count);
        casaCorreta = casasCena[indexSorteado];

    }


    public void VerificarCasa(CasaScript casaClicada)
    {
        if(jogoFinalizado) return;

        if(casaClicada == casaCorreta)
        {
            Debug.Log("Encontrou a casa do curupira! Vitória!");
            Vencer();
        }
        else
        {
            Debug.Log("Casa errada clicada! Derrota!");
            Perder();
        }
    }

    public override void TempoEsgotado()
    {
        if(jogoFinalizado) return;

        Debug.Log("Tempo esgotado!");
        // "o curupira fugiu? "
        base.TempoEsgotado();
    }
}
