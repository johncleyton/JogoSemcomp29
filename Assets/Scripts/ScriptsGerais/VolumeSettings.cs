using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider slider;
    public AudioSource source;
    public AudioClip[] clipes;
    public TMP_Text textoMusica;

    private void Start()
    {
        if (PlayerPrefs.HasKey("volumeMusica"))
            LoadVolume();
        else
            SetMusicValue();
        MostrarMusica();
    }

    public void SetMusicValue()
    {
        float volume = slider.value;
        mixer.SetFloat("musica", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("volumeMusica", volume);
    }

    private void LoadVolume()
    {
        slider.value = PlayerPrefs.GetFloat("volumeMusica");
        SetMusicValue();
    }

    public void TocarMusica(int id)
    {
        source.Stop();
        source.clip = clipes[id];
        source.Play();
        MostrarMusica();
    }

    public void MostrarMusica()
    {
        textoMusica.text = $"Musica: {source.clip.name}";
    }
}
