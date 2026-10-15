using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeradorNuvens : MonoBehaviour
{

    public GameObject NuvemPrefab; 
    public GameObject NuvemPrefab2; 
    public float tempoMaximo = 5f;
    public float cronometro = 0f;
    public float alturaVariavel = 3f;

    private int flagNuvem;

    // Start is called before the first frame update
    void Start()
    {
        GameObject novaNuvem = Instantiate(NuvemPrefab, gameObject.transform);
        novaNuvem.transform.position = transform.position;
        flagNuvem = 0;
    }

    void Update()
    {
        cronometro += Time.deltaTime;
        GameObject NuvemEscolhida = null;

        if (flagNuvem == 0)
        {
            NuvemEscolhida = NuvemPrefab;
            flagNuvem = 1;
        }
        else
        {
            NuvemEscolhida = NuvemPrefab2;
            flagNuvem = 0;
        }

        if (cronometro > tempoMaximo)
        {
            // Instancia o poste em uma altura aleatória
            GameObject novaNuvem = Instantiate(NuvemEscolhida, gameObject.transform);
            novaNuvem.transform.position = transform.position + new Vector3(0, Random.Range(-alturaVariavel, alturaVariavel), 0);
            
            cronometro = 0;
        }

        
    }
}
