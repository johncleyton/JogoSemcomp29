using UnityEngine;

// A mesa nasce em um local aleatório (dentro dos limites da comida) e a coxinha começa apoiada nela.
// A coxinha é desenhada sempre acima da mesa.
[RequireComponent(typeof(SpriteRenderer))]
public class MesaController : SpawnableObjects
{
    [SerializeField] private ComidaController _comida;
    // Posição da coxinha no tampo da mesa, relativa ao centro do sprite da mesa
    [SerializeField] private Vector2 _offsetDaComida = new Vector2(-0.06f, 0.24f);

    private void Awake()
    {
        SetSpawnPosition();

        _comida.transform.position = transform.position + (Vector3)_offsetDaComida;
        _comida.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
    }
}
