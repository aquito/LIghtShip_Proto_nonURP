using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Depth;
using Niantic.ARDK.AR.Awareness.Semantics;

public class SemanticSurfaces : MonoBehaviour
{
    ISemanticBuffer _currentBuffer;

    public ARSemanticSegmentationManager _semanticManager;
    //depth manager to get the depth buffer
    public ARDepthManager _depthManager;



    public Camera _camera;

    public Material groundShaderMaterial;
    public Material skyShaderMaterial;
    public Material statueShaderMaterial;

    private Material defaultMaterial;

    public TMP_Text _text;

    void Start()
    {
        //add a callback for catching the updated semantic buffer
        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;
        _depthManager.DepthBufferUpdated += OnDepthBufferUpdated;
    }

    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
        //get the current buffer
        _currentBuffer = args.Sender.AwarenessBuffer;
    }

    //Depth callback
    private void OnDepthBufferUpdated(ContextAwarenessArgs<IDepthBuffer> args)
    {
        IDepthBuffer depthBuffer = args.Sender.AwarenessBuffer;

        //we have a choice of ARGB or RFloat for the texture depending on what your device supports.
        //depthBuffer.CreateOrUpdateTextureRFloat(
        //    ref _depthTexture
        //);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 1;
        Debug.DrawRay(transform.position, forward, Color.green);

        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //print("I'm looking at " + hit.transform.name);

            SwitchSurfaceMaterial(hit.transform);

            int x = (int)hit.transform.position.x;
            int y = (int)hit.transform.position.y;


            //return the indices
            int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(x, y);

            //print them to console
            foreach (var i in channelsInPixel)
                Debug.Log(i);

            //return the names
            string[] channelsNamesInPixel = _semanticManager.SemanticBufferProcessor.GetChannelNamesAt(x, y);

            //print them to console
            foreach (var i in channelsNamesInPixel)
            {
                Debug.Log(i);

                //print to screen
                _text.text = i;
            }
                

            channelsInPixel = null;

            int xDepth = (int)hit.transform.position.x;
            int yDepth = (int)hit.transform.position.y;

            float depthPixel =_depthManager.DepthBufferProcessor.GetDepth(xDepth, yDepth);

            Vector3 depthNormal = _depthManager.DepthBufferProcessor.GetSurfaceNormal(xDepth, yDepth);

            Vector3 worldPosition = _depthManager.DepthBufferProcessor.GetWorldPosition(xDepth, yDepth);

            _text.text = "depthPixel: " + depthPixel + "; depthNormal: " + worldPosition;

        }
            
        else
            print("I'm looking at nothing!");

        if (PlatformAgnosticInput.touchCount <= 0) { return; }

        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            //list the channels that are available
            Debug.Log("Number of Channels available " + _semanticManager.SemanticBufferProcessor.ChannelCount);
            foreach (var c in _semanticManager.SemanticBufferProcessor.Channels)
                Debug.Log(c);


            int x = (int)touch.position.x;
            int y = (int)touch.position.y;

            //return the indices
            int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(x, y);

            //print them to console
            foreach (var i in channelsInPixel)
                Debug.Log(i);

            //return the names
            string[] channelsNamesInPixel = _semanticManager.SemanticBufferProcessor.GetChannelNamesAt(x, y);

            //print them to console
            foreach (var i in channelsNamesInPixel)
                Debug.Log(i);

        }

    }



    private void SwitchSurfaceMaterial(Transform transform)
    {
        GameObject obj = transform.gameObject;

        print("Gameobject I'm looking at is " + obj.name);

        defaultMaterial = obj.GetComponent<Renderer>().material;

        if(obj.name == "Statue")
        {
            obj.GetComponent<Renderer>().material = statueShaderMaterial;
        }
        else if(obj.name == "Ground")
        {
            obj.GetComponent<Renderer>().material = groundShaderMaterial;
        }
        else if(obj.name == "Sky")
        {
            obj.GetComponent<Renderer>().material = skyShaderMaterial;
        }    

    }
}
