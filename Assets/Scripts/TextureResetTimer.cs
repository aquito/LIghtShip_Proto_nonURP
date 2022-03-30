using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextureResetTimer : MonoBehaviour
{
    float timeFromStart = 0;

    public float interval = 10;

    float storedInterval;

    public TMP_Text countdownText;

    public bool hasTouchHappened = false;

    public SemanticTextures semanticTextures;

    private string[] liveInTheNow = new string[6];

    private int t;

    private int timer;

    // Start is called before the first frame update
    void Start()
    {
        storedInterval = interval;

        liveInTheNow[0] = "Touch me!";
        liveInTheNow[1] = "Live";
        liveInTheNow[2] = "in";
        liveInTheNow[3] = "the";
        liveInTheNow[4] = "NOW";
        

    }

    // Update is called once per frame
    void Update()
    {

        if(timeFromStart <= interval)
        {
            timeFromStart += Time.deltaTime;
            t = (int)interval - (int)timeFromStart;
            timer = (int)Mathf.Round(timeFromStart);
        }
        else
        {
            timeFromStart = 0;
            //timer = 0;  
            hasTouchHappened = false;
        }
           

        if (!hasTouchHappened)
        {
            countdownText.text = liveInTheNow[0]; //"Touch me!";
            semanticTextures.ResetSemanticTexture();

        }
        else
        {
              if(timer < liveInTheNow.Length)
             {
               
                countdownText.text = liveInTheNow[timer];
            }
           // else
           // {
               // timer = 0;
          //  }

            
        }

            
        
    }
}
