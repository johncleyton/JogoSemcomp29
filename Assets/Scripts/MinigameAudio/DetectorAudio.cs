/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorAudio : MonoBehaviour
{
    public int sampleWindow = 64;
    private AudioClip micClip;
    private string nomeMic;

    void Start()
    {
        MicToClip(0);
    }

    private void MicToClip(int index)
    {
        nomeMic = Microphone.devices[index];
        micClip = Microphone.Start(nomeMic, true, 20, AudioSettings.outputSampleRate);
    }

    public float getLoudnessMic()
    {
        return getLoudnessClip(Microphone.GetPosition(nomeMic), micClip);
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
*/