using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TocarMusicas : MonoBehaviour
{
    public AudioSource source;
    public AudioClip[] clips;
    private List<int> lista = new List<int>();
    private int ultimaMusica = -1;
    // Start is called before the first frame update
    void Start()
    {
        source = gameObject.GetComponent<AudioSource>();
        source.clip = SortearMusga();
        MapaBS.CalcularBPM();
        StartCoroutine(AudioHelper.FadeIn(source, 2f));
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying)
        {
            source.clip = SortearMusga();
            MapaBS.CalcularBPM();
            StartCoroutine(AudioHelper.FadeIn(source, 2f));
        }
    }

    private AudioClip SortearMusga()
    {
        if (lista.Count == 0)
        {
            for (int i = 0; i < clips.Length; i++)
                lista.Add(i);
        }

        int posicao = UnityEngine.Random.Range(0, lista.Count);

        if (lista.Count > 1 && lista[posicao] == ultimaMusica)
        {
            int deslocamento = UnityEngine.Random.Range(1, lista.Count);

            posicao = (posicao + deslocamento) % lista.Count;
        }

        int indiceEscolhido = lista[posicao];

        lista.RemoveAt(posicao);
        ultimaMusica = indiceEscolhido;

        return clips[indiceEscolhido];
    }
}
