using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DetectorAudio : MonoBehaviour
{
    public int sampleWindow = 64;
    private AudioClip micClip;
    private string nomeMic;

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void InitWebGLMic();

        [DllImport("__Internal")]
        private static extern float GetWebGLLoudness();
    #endif

    void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            InitWebGLMic();
        #else
        {
            if (Microphone.devices.Length > 0)
                MicToClip(0);
            else
                Debug.Log("Microfone não encontrado");
        }
        #endif
    }

    #if !UNITY_WEBGL || UNITY_EDITOR
    private void MicToClip(int index)
    {
        nomeMic = Microphone.devices[index];
        micClip = Microphone.Start(nomeMic, true, 20, AudioSettings.outputSampleRate);
    }
    #endif

    public float getLoudnessMic()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            return GetWebGLLoudness();
        #else
            if (Microphone.devices.Length == 0 || micClip == null) 
                return 0;
            return getLoudnessClip(Microphone.GetPosition(nomeMic), micClip);
        #endif
    }

    public float getLoudnessClip(int clipPos, AudioClip clip)
    {
        int inicio = clipPos - sampleWindow;
        if (inicio < 0)
            return 0;

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, inicio);


        float loudness = 0;
        foreach (var sample in waveData)
            loudness += Mathf.Abs(sample);

        return loudness / sampleWindow;
    }
}