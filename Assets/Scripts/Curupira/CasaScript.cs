using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasaScript : MonoBehaviour
{
    void OnMouseDown()
    {
      if(CurupiraManager.Instance != null)
        {
            CurupiraManager.Instance.VerificarCasa(this);
        }  
    }
}
