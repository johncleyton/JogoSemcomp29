using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachorroController : SpawnableObjects
{
    [SerializeField] private GameObject _comida;
    [Range(1f, 10f)]
    [SerializeField] private float _speed;

    // Os desenhos dos cachorros olham para baixo (-Y) com a rotação zerada
    private const float SPRITE_FACING_OFFSET = 90f;
    // Proporção do sprite ocupada pelo colisor, para o cachorro não "comer" a comida de longe
    private const float COLLIDER_WIDTH_RATIO = 0.7f;
    private const float COLLIDER_HEIGHT_RATIO = 0.9f;

    private const float X_CACHORRO_AIM_ERROR_THRESHOLD = 4f;
    private const float Y_CACHORRO_AIM_ERROR_THRESHOLD = 2f;

    void Start()
    {
        if (_comida == null)
            _comida = GameObject.FindWithTag("Player");
        SetSpawnPosition();
        FitColliderToSprite();
        SetSpriteFacingComida();
        StartCoroutine(AtacarAposDelay());
    }

    // O cachorro fica parado, olhando para a comida, até o jogador ter tempo de perceber a cena
    private IEnumerator AtacarAposDelay()
    {
        CachorroGameManager gameManager = Object.FindFirstObjectByType<CachorroGameManager>();
        yield return new WaitForSeconds(gameManager.DelayAntesDoAtaque);

        SetSpriteFacingComida();
        SetDirectionToComida();
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetSprite(Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void SetComida(GameObject comida)
    {
        _comida = comida;
    }

    // Cada cachorro tem um desenho de tamanho diferente, então o colisor acompanha o sprite
    private void FitColliderToSprite()
    {
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        if (sprite == null || !(_collider is CapsuleCollider2D capsule))
            return;

        Vector2 size = sprite.bounds.size;
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.offset = Vector2.zero;
        capsule.size = new Vector2(size.x * COLLIDER_WIDTH_RATIO, size.y * COLLIDER_HEIGHT_RATIO);
    }

    private void SetSpriteFacingComida()
    {
        Vector2 direction = _comida.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        this.transform.rotation = Quaternion.Euler(0f, 0f, angle + SPRITE_FACING_OFFSET);
    }

    private void SetDirectionToComida()
    {
        Vector3 direction = (_comida.transform.position - transform.position).normalized;

        int numOfCachorros = DifficultyController.Instance.GetNumberOfCachorros();
        if (numOfCachorros > 1)
        {
            float x = Random.Range(-X_CACHORRO_AIM_ERROR_THRESHOLD, X_CACHORRO_AIM_ERROR_THRESHOLD);
            float y = Random.Range(-Y_CACHORRO_AIM_ERROR_THRESHOLD, Y_CACHORRO_AIM_ERROR_THRESHOLD);
            direction = new Vector3(direction.x + x, direction.y + y, direction.z);
        }

        _collider.gameObject.GetComponent<Rigidbody2D>().velocity = direction * _speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Cachorro comeu a comida!");
            Object.FindFirstObjectByType<CachorroGameManager>().Perder();
        }
    }
}
