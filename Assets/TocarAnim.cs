using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TocarAnim : MonoBehaviour
{
    public ParticleSystem particula;
    public void soltarParticula()
    {
        particula.Play();
    }
}
