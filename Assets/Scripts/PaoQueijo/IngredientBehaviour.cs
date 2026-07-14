using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientBehaviour : MonoBehaviour
{


    [Tooltip("Quantos segundos demora a chegar na mesa")]
    public float flightDuration = 3.0f;

    private float timeElapsed = 0f;

    public bool gotCaught;

    public IngredientData ingredientData;

    public Vector3 targetPos;

    // OBSERVAÇÃO: este campo não entra em nenhum cálculo hoje — o voo é
    // controlado inteiramente por "flightDuration" (tempo fixo até a mesa,
    // independente da distância). Mantido sem alteração para não perder
    // valores já configurados em prefabs; me avise se quiser que o voo passe
    // a depender da velocidade em vez do tempo fixo.
    public float speed = 10;

    [Tooltip("How high the arc should be in units")]
    public float arcHeight = 1;

    Vector3 startPos;

    void Start()
    {
        gotCaught = false;

        startPos = transform.position;
        

        // pensei em ter uma animação aqui,
        // mostrando que ta sendo tacado o ingrediente no player
    }

    static Quaternion LookAt2D(Vector2 forward)
    {
        return Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }


    void Update()
    {
        timeElapsed += Time.deltaTime;

        float percentagePath = timeElapsed / flightDuration;

        percentagePath = Mathf.Clamp01(percentagePath);
        

        Vector3 basePos = Vector3.Lerp(startPos, targetPos, percentagePath);


        float arc = arcHeight * 4.0f * (percentagePath * (1 - percentagePath));

        Vector3 nextPos = new Vector3(basePos.x, basePos.y + arc, basePos.z);

        transform.rotation = LookAt2D(nextPos - transform.position);

        transform.position = nextPos;

        if(percentagePath >= 1f)
        {
            Arrived();
        }
    }


    void Arrived()
    {
        // TODO: hoje o ingrediente simplesmente desaparece se não for pego a
        // tempo, sem nenhuma penalidade (vida, pontos, som de erro, etc).
        // Esse é o lugar certo para adicionar essa reação no futuro.
        Destroy(gameObject);
    }
}
