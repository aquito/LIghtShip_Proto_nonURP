using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;



public class Dragging : MonoBehaviour
{

    private float dist;

    private bool dragging = false;

    private Vector3 offset;

    private GameObject cursorTarget;

    private Transform toDrag;

    public SemanticTextures semanticTextures;

    // Start is called before the first frame update
    void Start()
    {
        cursorTarget = GameObject.FindGameObjectWithTag("target");
        cursorTarget.SetActive(false);

        semanticTextures = Camera.main.GetComponent<SemanticTextures>();
    }

   
private void Update() {

     Vector3 v3;

    // if(PlatformAgnosticInput.touchCount <= 0) { return; }

     if(PlatformAgnosticInput.touchCount !=1)
        {
            dragging = false;
            return;
        }

        var touch = PlatformAgnosticInput.GetTouch(0);
        Vector3 pos = touch.position;

        if (touch.phase == TouchPhase.Began)
        {

            if(cursorTarget.activeSelf)
            {
                Debug.Log("target active");

                int x = (int)touch.position.x;
                int y = (int)touch.position.y;
                semanticTextures.SetSemanticTexture(x, y);
                cursorTarget.SetActive(false);
            }

            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.tag == "cursor")
                {

                    toDrag = hit.transform;
                    dist = hit.transform.position.z - Camera.main.transform.position.z;
                    v3 = new Vector3(pos.x, pos.y);
                    v3 = Camera.main.ScreenToWorldPoint(v3);
                    offset = toDrag.position - v3;
                    dragging = true;
                    
                }
            }
        }

        if(dragging && touch.phase == TouchPhase.Moved)
        {

            if (cursorTarget.activeSelf)
            {
                cursorTarget.SetActive(false);
            }

            v3 = new Vector3(touch.position.x, touch.position.y, dist);
            v3 = Camera.main.ScreenToWorldPoint(v3);
            toDrag.position = v3; //+ offset;
            
        }

        if(dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
        {
            if (!cursorTarget.activeSelf)
                cursorTarget.SetActive(true);

            dragging = false;
            //dist = 0;

            

        }

    }

}
