using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachorroSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _cachorroPrefab;
    [SerializeField] private GameObject _limitsList;
    // Desenhos disponíveis para os cachorros; cada cachorro spawnado usa um deles aleatoriamente
    [SerializeField] private Sprite[] _cachorroSprites;

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
        if (_cachorroSprites != null && _cachorroSprites.Length > 0)
            cachorro.GetComponent<CachorroController>().SetSprite(_cachorroSprites[Random.Range(0, _cachorroSprites.Length)]);

        cachorro.transform.SetParent(transform);
    }
}
