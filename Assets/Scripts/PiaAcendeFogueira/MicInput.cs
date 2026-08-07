using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class MicInput : MonoBehaviour
{
    public float sensitivity = 100f;
    public int sampleWindow = 64; //quantidade amostras para calc volume audio

    public float loudness = 0f;

    private AudioClip micClip;
    private string deviceName;

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
            if(Microphone.devices.Length > 0)
            {   
                deviceName = Microphone.devices[0];
                micClip = Microphone.Start(deviceName, true, 20, 44100);
            }
            else
            {
                Debug.LogWarning("Nenhum microfone conectado. O jogo não funcionará como esperado.");
            }
        #endif
    }

    #if !UNITY_WEBGL || UNITY_EDITOR
    private float getLoudnessMicrophone()
    {
        if (deviceName == null) return 0f;

        int clipPos = Microphone.GetPosition(deviceName) - sampleWindow;

        if(clipPos < 0 ) return 0f;

        float [] waveData = new float[sampleWindow];
        micClip.GetData(waveData, clipPos);

        float totalSqr = 0f;

        for(int i=0; i < sampleWindow; i++)
        {
            totalSqr += waveData[i] * waveData[i];
        }

        return Mathf.Sqrt(totalSqr / sampleWindow);
    }
    #endif

    void Update()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            loudness = GetWebGLLoudness() * sensitivity;
        #else
            if(micClip != null && Microphone.IsRecording(deviceName))
            {
                loudness = getLoudnessMicrophone() * sensitivity;
            }
        #endif
    }

    void OnDisable()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if(deviceName != null && Microphone.IsRecording(deviceName))
            Microphone.End(deviceName);
        #endif
    }
}