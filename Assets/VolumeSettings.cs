using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider slider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("volumeMusica"))
            LoadVolume();
        else
            SetMusicValue();
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
}
