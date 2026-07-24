using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavalhadaManager : MonoBehaviour
{
    public GameObject[] lanes;
    public GameObject cavalo;
    public float cooldown;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(summonCavalo), cooldown, cooldown);
    }

    void summonCavalo()
    {
        int random = Random.Range(0, lanes.Length);
        Instantiate(cavalo, lanes[random].transform.position, Quaternion.identity, lanes[random].transform);
    }
    
}
