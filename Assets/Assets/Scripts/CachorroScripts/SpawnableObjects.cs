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


    public void SetLimits(GameObject objectWithLimits)
    {
        _upperLimit = objectWithLimits.transform.Find("CachorroUpperLimit");
        _lowerLimit = objectWithLimits.transform.Find("CachorroLowerLimit");
        _leftLimit = objectWithLimits.transform.Find("CachorroLeftLimit");
        _rightLimit = objectWithLimits.transform.Find("CachorroRightLimit");    
    }

    public void SetSpawnPosition()
    {
        if (_upperLimit != null && _lowerLimit != null && _leftLimit != null && _rightLimit != null)
        {
            float x = Random.Range(_leftLimit.position.x, _rightLimit.position.x);
            float y = Random.Range(_lowerLimit.position.y, _upperLimit.position.y);
            transform.position = new Vector3(x, y, 0f);
        }
        else
        {
            Debug.LogError("Limits not set for " + gameObject.name);
        }
    }
}
