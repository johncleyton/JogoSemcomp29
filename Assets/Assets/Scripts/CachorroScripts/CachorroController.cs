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
        SetSpriteFacingComida();
        SetDirectionToComida();
    }

    private void SetSpriteFacingComida()
    {
        Vector2 direction = _comida.transform.position - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        this.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
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
            // Aqui você pode adicionar lógica para o que acontece quando o cachorro come a comida.
        }
    }
}
