using UnityEngine;

public class GeradorPostes : MonoBehaviour
{
    public GameObject PostePrefab;
    public float tempoMaximo = 3f;
    private float cronometro = 0f;
    public float alturaVariavel = 3f;

    void Start()
    {
        GameObject novoPoste = Instantiate(PostePrefab, gameObject.transform);
        novoPoste.transform.position = transform.position;
    }

    void Update()
    {
        cronometro += Time.deltaTime;

        if (cronometro > tempoMaximo)
        {
            // Instancia o poste em uma altura aleatória
            GameObject novoPoste = Instantiate(PostePrefab, gameObject.transform);
            novoPoste.transform.position = transform.position + new Vector3(0, Random.Range(-alturaVariavel, alturaVariavel), 0);
            
            cronometro = 0;
            tempoMaximo -=0.05f;
        }

        
    }
}