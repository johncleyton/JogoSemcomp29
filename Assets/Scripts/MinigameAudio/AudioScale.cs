/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScale : MonoBehaviour
{
    public Vector3 minScale, maxScale;
    public DetectorAudio detector;

    public float audioSens;
    public float threshold = 0.1f;

    // Update is called once per frame
    void Update()
    {
        float loudness = detector.getLoudnessMic() * audioSens;
        if (loudness < threshold)
            loudness = 0;
        
        transform.localScale = Vector3.Lerp(minScale, maxScale, loudness);
    }
}
*/