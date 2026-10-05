using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachorroSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _cachorroPrefab;
    [SerializeField] private GameObject _limitsList;

    public static CachorroSpawner Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        Instance = this;
    }

    public void SpawnCachorro(float speed)
    {
        GameObject cachorro = Instantiate(_cachorroPrefab, transform.position, Quaternion.identity);
        cachorro.GetComponent<CachorroController>().SetLimits(_limitsList);
        cachorro.GetComponent<CachorroController>().SetSpawnPosition();
        cachorro.GetComponent<CachorroController>().SetSpeed(speed);

        cachorro.transform.SetParent(transform);
    }
}
