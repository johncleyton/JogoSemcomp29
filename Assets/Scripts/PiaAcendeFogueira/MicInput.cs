/*using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.iOS;

public class MicInput : MonoBehaviour
{
    public float sensitivity = 100f;
    public int sampleWindow = 64; //quantidade amostras para calc volume audio


    public float loudness = 0f;

    private AudioClip micClip;

    private string deviceName;

    // Start is called before the first frame update
    void Start()
    {

        // algum microfone conectado
        if(Microphone.devices.Length > 0)
        {   
            //primeiro conectar - padrao
            deviceName = Microphone.devices[0];

            // name, loopado, 20 seg, freq superior
            micClip = Microphone.Start(deviceName, true, 20, 44100);

        }
        else
        {
            Debug.LogWarning("Nenhum microfone conectado. O jogo não funcionará como esperado.");
        }

    }


    private float getLoudnessMicrophone()
    {
        int clipPos = Microphone.GetPosition(deviceName) - sampleWindow;

        if(clipPos < 0 ) return 0f;

        // n sampleWindow posicoes
        float [] waveData = new float[sampleWindow];


        micClip.GetData(waveData, clipPos);

        float totalSqr = 0f;

        for(int i=0; i < sampleWindow; i++)
        {
            totalSqr += waveData[i] * waveData[i];
        }

        return Mathf.Sqrt(totalSqr / sampleWindow);
     }

    // Update is called once per frame
    void Update()
    {
        if(micClip != null && Microphone.IsRecording(deviceName))
        {
            loudness = getLoudnessMicrophone() * sensitivity;
        }
    }



    void onDisable()
    {
        if(deviceName != null && Microphone.IsRecording(deviceName))
        {
            // libera
            Microphone.End(deviceName);
        }
    }

}
*/