using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

public class DragUI : MonoBehaviour //, IPointerDownHandler, IPointerUpHandler
{

    private bool mouseDown = false;
    private Vector3 startMousePos;
    private Vector3 startPos;
    private bool restrictX;
    private bool restrictY;
    private float fakeX;
    private float fakeY;
    private float myWidth;
    private float myHeight;

    private bool dragging = false;
    private Transform toDrag;

    private float dist;
    private Vector3 offset;


    public RectTransform ParentRT;
    public RectTransform MyRect;

    public GameObject cursorTarget;
    public SemanticTextures semanticTextures;

    public bool hasFirstTouchHappened;
    public TextureResetTimer textureResetTimer;

    public RawImage mandalaImage;


    void Start()
    {
        hasFirstTouchHappened = false;

        myWidth = (MyRect.rect.width + 5) / 2;
        myHeight = (MyRect.rect.height + 5) / 2;

        semanticTextures = Camera.main.GetComponent<SemanticTextures>();
        

        if (cursorTarget != null)
            cursorTarget.SetActive(false);
         
    }


    void Update()
    {
        Vector3 v3;

        if (PlatformAgnosticInput.touchCount != 1)
        {
            dragging = false;
            mandalaImage.enabled = true;

            return;
        }

        var touch = PlatformAgnosticInput.GetTouch(0);
        Vector3 touchPos = touch.position;

        if (touch.phase == TouchPhase.Began)
        {
            hasFirstTouchHappened = true;

            if(!textureResetTimer.hasTouchHappened)
            {
                textureResetTimer.hasTouchHappened = true;
            }

            mandalaImage.enabled = false;

            Vector3 currentPos = Input.mousePosition;
            Vector3 diff = currentPos; //- startMousePos;
            Vector3 pos = startPos + diff;
            transform.position = touchPos;



            //if (cursorTarget.activeSelf)
            //{
                //Debug.Log("target active");

                int x = (int)touch.position.x;
                int y = (int)touch.position.y;


                semanticTextures.SetSemanticTexture(x, y);
                cursorTarget.SetActive(false);
           // }


            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.tag == "cursor")
                {

                    toDrag = hit.transform;
                    dist = hit.transform.position.z - Camera.main.transform.position.z;
                    v3 = new Vector3(pos.x, pos.y);
                    v3 = Camera.main.ScreenToWorldPoint(v3);
                    offset = toDrag.position - v3;
                    dragging = true;

                }
            }

            if (dragging && touch.phase == TouchPhase.Moved)
            {
                //mandalaImage.enabled = true;

                if (cursorTarget.activeSelf)
                {
                    cursorTarget.SetActive(false);
                    

                }

                v3 = new Vector3(touch.position.x, touch.position.y, this.gameObject.transform.localPosition.z);
                v3 = Camera.main.ScreenToWorldPoint(v3);
                toDrag.position = v3; //+ offset;

                if (transform.localPosition.x < 0 - ((ParentRT.rect.width / 2) - myWidth) || transform.localPosition.x > ((ParentRT.rect.width / 2) - myWidth))
                    restrictX = true;
                else
                    restrictX = false;

                if (transform.localPosition.y < 0 - ((ParentRT.rect.height / 2) - myHeight) || transform.localPosition.y > ((ParentRT.rect.height / 2) - myHeight))
                    restrictY = true;
                else
                    restrictY = false;

                if (restrictX)
                {
                    if (transform.localPosition.x < 0)
                        fakeX = 0 - (ParentRT.rect.width / 2) + myWidth;
                    else
                        fakeX = (ParentRT.rect.width / 2) - myWidth;

                    Vector3 xpos = new Vector3(fakeX, transform.localPosition.y, 0.0f);
                    transform.localPosition = xpos;
                }

                if (restrictY)
                {
                    if (transform.localPosition.y < 0)
                        fakeY = 0 - (ParentRT.rect.height / 2) + myHeight;
                    else
                        fakeY = (ParentRT.rect.height / 2) - myHeight;

                    Vector3 ypos = new Vector3(transform.localPosition.x, fakeY, 0.0f);
                    transform.localPosition = ypos;
                }

            }

            if (dragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                if (!cursorTarget.activeSelf)
                    cursorTarget.SetActive(true);

                dragging = false;

                mandalaImage.enabled = true;

                semanticTextures.ResetSemanticTexture();

            }

        }

        
    }
}