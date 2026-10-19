using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditosManager : MonoBehaviour
{
    public GameObject creditos1, creditos2;
    public TMP_Text qualPag;

    public void TrocarPagina()
    {
        if (creditos1.activeInHierarchy)
        {
            creditos1.SetActive(false);
            creditos2.SetActive(true);
            qualPag.text = "2/2";
        }
        else if (creditos2.activeInHierarchy)
        {
            creditos2.SetActive(false);
            creditos1.SetActive(true);
            qualPag.text = "1/2";
        }
    }
    
    void OnEnable()
    {
        creditos2.SetActive(false);
        creditos1.SetActive(true);
        qualPag.text = "1/2";
    }

}
