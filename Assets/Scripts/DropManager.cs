using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;

public class DropManager : MonoBehaviour
{

    public GameObject drop;

    private GameObject[] drops;

    public GameObject markerCube;

    public bool hasDropCollided;
 
    public int maxDropsInTheScene;

    private int dropsInTheScene;

    public Transform dropStartPosition;

    /*
    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
        //get the buffer that has been surfaced.
        ISemanticBuffer semanticBuffer = args.Sender.AwarenessBuffer;


    }
    */

    private void Start()
    {

        drops = new GameObject[maxDropsInTheScene];

        dropsInTheScene = 0;

        var obj = Instantiate(drop, this.gameObject.transform.position, Quaternion.identity);

       // obj.transform.position = dropStartPosition.position;

        drops[0] = obj;

        hasDropCollided = obj.GetComponent<GetSemanticChannel>().hasCollided;

        dropsInTheScene++;

        

    }


    /*
    public void GetSemanticChannels(float xFloat, float yFloat)
    {

        int x = (int)xFloat;
        int y = (int)yFloat;

        Debug.Log("Querying semantic channels at " + x + "," + y);
        //return the indices
        int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(x, y);

        Debug.Log(channelsInPixel.Length + " channels there!");

        //print them to console
        foreach (var i in channelsInPixel)
        {
            Debug.Log("channels found: " + i);

        }

        if(dropsInTheScene < maxDropsInTheScene)
        {
            InstantiateNewDrop();
            
        }
        else
        {
            //DestroyDrops();

        }
        

    }
    */

    private void Update()
    {
       
            if (hasDropCollided)
            {
                if (dropsInTheScene < maxDropsInTheScene)
                {
                    InstantiateNewDrop();

                }
                else
                {
                    DestroyDrops();

                }
            }
        
        
    }

    private void InstantiateNewDrop()
    {

        var obj = Instantiate(drop, this.gameObject.transform.position, Quaternion.identity);

        //obj.transform.position = this.gameObject.transform.position;

        drops[dropsInTheScene] = obj;

        hasDropCollided = obj.GetComponent<GetSemanticChannel>().hasCollided;
        
        dropsInTheScene++;
    }

    private void DestroyDrops()
    {
        foreach (GameObject obj in drops)
        {
            Destroy(obj);

        }

        dropsInTheScene = 0;
        InstantiateNewDrop();
    }    


}
