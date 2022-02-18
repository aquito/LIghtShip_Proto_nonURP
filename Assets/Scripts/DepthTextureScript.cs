using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK.AR.ARSessionEventArgs;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Depth;
using Niantic.ARDK.AR.Configuration;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.Helpers;

public class DepthTextureScript : MonoBehaviour
{
    Texture2D _depthTexture;

    public Material _shaderMaterial;

    [Header("ARDK Managers")]
    //All of the ardk managers we need for this Tutorial
    //depth manager to get the depth buffer
    public ARDepthManager _depthManager;

    // Start is called before the first frame update
    void Start()
    {
        //hook the depth and semantic managers update functions 
        //in order to capture the buffers we need for our shader
        _depthManager.DepthBufferUpdated += OnDepthBufferUpdated;

    }

    //Depth callback
    private void OnDepthBufferUpdated(ContextAwarenessArgs<IDepthBuffer> args)
    {
        IDepthBuffer depthBuffer = args.Sender.AwarenessBuffer;

        //we have a choice of ARGB or RFloat for the texture depending on what your device supports.
        depthBuffer.CreateOrUpdateTextureRFloat(
            ref _depthTexture
        );
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //pass in our texture
        //Our Depth Buffer
        _shaderMaterial.SetTexture("_DepthTex", _depthTexture);

        //pass in our transform
        _shaderMaterial.SetMatrix("_depthTransform", _depthManager.DepthBufferProcessor.SamplerTransform);

        //blit everything with our shader
        Graphics.Blit(source, destination, _shaderMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        // _depthManager.DepthBufferProcessor.GetDepth
    }
}
