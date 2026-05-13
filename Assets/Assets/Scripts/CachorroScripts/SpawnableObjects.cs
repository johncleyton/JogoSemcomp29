using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObjects : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform _upperLimit;
    [SerializeField] private Transform _lowerLimit;
    [SerializeField] private Transform _leftLimit;
    [SerializeField] private Transform _rightLimit;

    [SerializeField] protected Collider2D _collider;

    protected virtual void Awake()
    {
        SetSpawnPosition();
    }

    private void SetSpawnPosition()
    {
        float x = Random.Range(_leftLimit.position.x, _rightLimit.position.x);
        float y = Random.Range(_lowerLimit.position.y, _upperLimit.position.y);
        transform.position = new Vector3(x, y, 0f);
    }
}
