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
using Niantic.ARDK.AR.Awareness.Depth;
using Niantic.ARDK.AR.HitTest;


public class GetSemanticChannel : MonoBehaviour
{

    GameObject obj;
    Transform objTransform;
    DropManager dropManager;
    public bool hasCollided;
    ThrowDrop throwDrop;

    public SemanticTextures semanticTextures;

    public GameObject markerCube;

    private Camera _camera;

    public ARSemanticSegmentationManager _semanticManager;

    public ARDepthManager _depthManager;

    IARSession _session;

    public ARHitTestResultType HitTestType = ARHitTestResultType.All; //ExistingPlane;

    void Start()
    {
        _camera = Camera.main;

        obj = this.gameObject;
        objTransform = obj.transform;
        dropManager = GameObject.Find("DropManager").GetComponent<DropManager>();
        throwDrop = gameObject.GetComponent<ThrowDrop>();
        hasCollided = false;

        markerCube = dropManager.GetComponent<DropManager>().markerCube;

        _semanticManager = Camera.main.GetComponent<ARSemanticSegmentationManager>();

        semanticTextures = Camera.main.GetComponent<SemanticTextures>();

        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;

        _depthManager = Camera.main.GetComponent<ARDepthManager>();

        _depthManager.DepthBufferUpdated += OnDepthBufferUpdated;

        // Enable debug visualization of depth buffer
       // _depthManager.ToggleDebugVisualization(true);

        ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
        //get the buffer that has been surfaced.
        ISemanticBuffer semanticBuffer = args.Sender.AwarenessBuffer;


    }

    //Depth callback
    private void OnDepthBufferUpdated(ContextAwarenessArgs<IDepthBuffer> args)
    {
        IDepthBuffer depthBuffer = args.Sender.AwarenessBuffer;

    }

    //callback for the session starting.
    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        //only run once guard
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;

        //save the session.
        _session = args.Session;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided)
        {

            GameObject other = collision.gameObject;

            if (other.tag != "MainCamera" || other.tag != "drop" && objTransform != null)
            {

                Debug.Log("drop colliding with " + other.name);


                obj.transform.GetComponent<Rigidbody>().useGravity = false;
                //obj.transform.GetComponent<Rigidbody>().isKinematic = true;
                //obj.transform.GetComponent<Collider>().enabled = false;

                //var markerObj = Instantiate(markerCube, obj.transform.position, Quaternion.identity);

                float x = other.transform.position.x;
                float y = other.transform.position.y;

                Vector3 worldToScreen = Camera.main.WorldToScreenPoint(other.transform.position);

                

                int xInt = (int)worldToScreen.x;
                int yInt = (int)worldToScreen.y;

                semanticTextures.SetSemanticTexture(xInt, yInt);

               // Vector3 surface = _depthManager.DepthBufferProcessor.GetWorldPosition(xInt, yInt);

               // Debug.DrawRay(markerObj.transform.position, markerObj.transform.forward * 5, Color.green, 5.0f);

                //Debug.Log("Surface normal: " + surface.x + "," + surface.y + "," + surface.z);

               // HitTest(markerObj.transform.forward * 5);

                

               Debug.Log("Querying semantic channels at " + x + "," + y);



                /*
               //return the indices
               int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(xInt, yInt);

               Debug.Log(channelsInPixel.Length + " channels there!");


               //print them to console
               foreach (var i in channelsInPixel)
               {
                   Debug.Log("channels found: " + i);

               }
                

                */

                dropManager.GetComponent<DropManager>().hasDropCollided = true;

                    hasCollided = true;

                    throwDrop.enabled = false;





                // Debug.Log(obj.name + " was destroyed");
                // Destroy(obj);   
                //}
            }


            

        }

        
    }

    void HitTest(Vector3 vector)
    {
        //check we have a valid frame.
        var currentFrame = _session.CurrentFrame;
        if (currentFrame == null)
        {
            return;
        }

        if (_camera == null)
            return;

        //do a hit test at at that screen point
        var hitTestResults =
          currentFrame.HitTest
          (
            _camera.pixelWidth,
            _camera.pixelHeight,
            vector,
            HitTestType
          );

        Debug.Log("hit results: " + hitTestResults.Count);

        if (hitTestResults.Count == 0)
            return;

        var xInt = (int)hitTestResults[0].WorldTransform.ToPosition().x;
        var yInt = (int)hitTestResults[0].WorldTransform.ToPosition().y;


        int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(xInt, yInt);

        Debug.Log(channelsInPixel.Length + " channels there!");

        foreach (var i in channelsInPixel)
        {
            Debug.Log("channels found: " + i);

        }
    }
}
