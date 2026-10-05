using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachorroController : SpawnableObjects
{
    [SerializeField] private GameObject _comida;
    [Range(1f, 10f)]
    [SerializeField] private float _speed;

    // O desenho do cachorro (visto de lado) olha para a esquerda (-X) com a rotação zerada
    private const float SPRITE_FACING_OFFSET = 180f;
    // Proporção do sprite ocupada pelo colisor (corpo, sem o rabo e as patas), para o cachorro não "comer" a comida de longe
    private const float COLLIDER_WIDTH_RATIO = 0.85f;
    private const float COLLIDER_HEIGHT_RATIO = 0.7f;

    private SpriteRenderer _spriteRenderer;

    private const float X_CACHORRO_AIM_ERROR_THRESHOLD = 4f;
    private const float Y_CACHORRO_AIM_ERROR_THRESHOLD = 2f;

    void Start()
    {
        if (_comida == null)
            _comida = GameObject.FindWithTag("Player");
        _spriteRenderer = GetComponent<SpriteRenderer>();
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

    public void SetComida(GameObject comida)
    {
        _comida = comida;
    }

    // O colisor acompanha o tamanho do sprite
    private void FitColliderToSprite()
    {
        Sprite sprite = _spriteRenderer.sprite;
        if (sprite == null || !(_collider is CapsuleCollider2D capsule))
            return;

        Vector2 size = sprite.bounds.size;
        capsule.direction = CapsuleDirection2D.Horizontal;
        capsule.offset = Vector2.zero;
        capsule.size = new Vector2(size.x * COLLIDER_WIDTH_RATIO, size.y * COLLIDER_HEIGHT_RATIO);
    }

    private void SetSpriteFacingComida()
    {
        Vector2 direction = _comida.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ao ir para a direita o sprite é espelhado em vez de girado 180°, para o cachorro não ficar de cabeça para baixo
        bool olhandoParaDireita = direction.x > 0f;
        _spriteRenderer.flipX = olhandoParaDireita;
        float rotation = olhandoParaDireita ? angle : angle + SPRITE_FACING_OFFSET;
        this.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
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
