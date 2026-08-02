/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fogueira : MonoBehaviour
{


    public MicInput micInput;

    public Transform fogoSprite;

    public float timeLimit = 6f;

    private float currentFire = 0f;
    private float timer = 0f;

    private bool gameOver = false;


    public float blowThreshold = 0.5f;
    public float fireGrowth = 25f;

    public float fireDecay = 10f;

    public float maxFireScale = 3f;
    // Start is called before the first frame update
    void Start()
    {
        fogoSprite.localScale = Vector3.one * 0.1f;
    }

    
    private void checkWin()
    {
        if(currentFire >= 100f)
        {
            gameOver = true;
            // vitoria
            Debug.Log("Fogueira acesa!");

        }else if (timer >= timeLimit)
        {
            gameOver = true;


            // derrota
            Debug.Log("QUE DEMORA PIÁ!!!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameOver) return;

        timer += Time.deltaTime;

        if(micInput.loudness > blowThreshold)
        {
            currentFire += fireGrowth * Time.deltaTime;
        }
        else
        {
            currentFire -= fireDecay * Time.deltaTime;
        }


        float mappedScale = Mathf.Lerp(0.1f, maxFireScale, currentFire / 100f);

        fogoSprite.localScale = Vector3.one * mappedScale;


        checkWin();
    }

}
*/