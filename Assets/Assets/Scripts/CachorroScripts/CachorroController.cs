using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachorroController : SpawnableObjects
{
    [SerializeField] private GameObject _comida;
    [Range(1f, 10f)]
    [SerializeField] private float _speed;

    void Start()
    {
        if (_comida == null)
            _comida = GameObject.FindWithTag("Player");
        SetSpawnPosition();
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

    private void SetSpriteFacingComida()
    {
        Vector2 direction = _comida.transform.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        this.transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);       // MUDAR ANGULO PARA O CORRETO DE ACORDO COM A SPRITE DO CACHORRO, SE NECESSÁRIO
    }

    private void SetDirectionToComida()
    {
        Vector3 direction = (_comida.transform.position - transform.position).normalized;
        _collider.gameObject.GetComponent<Rigidbody2D>().velocity = direction * _speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<ComidaController>().IsEaten = true;

            Debug.Log("Cachorro comeu a comida!");
        }
    }
}
