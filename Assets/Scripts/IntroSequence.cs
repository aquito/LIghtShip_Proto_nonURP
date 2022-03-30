using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{

    float timeFromStart = 0;

    public float introDuration = 2;


    [SerializeField]
    GameObject cursor;

    SemanticTextures semanticTextures;

    // Start is called before the first frame update
    void Start()
    {
        semanticTextures = Camera.main.GetComponent<SemanticTextures>();
        //semanticTextures.enabled = false;
        cursor.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        timeFromStart += Time.deltaTime;

        if(timeFromStart >= introDuration)
        {
            semanticTextures.enabled = true;
            //flybyText.SetActive(false);
            cursor.SetActive(true);

        }
        
    }
}
