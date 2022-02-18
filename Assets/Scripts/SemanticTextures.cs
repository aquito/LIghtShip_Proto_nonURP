using System.Collections;
using System.Collections.Generic;
using System;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

using Niantic.ARDK.Extensions;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SemanticTextures : MonoBehaviour
{
    //pass in our semantic manager
    public ARSemanticSegmentationManager _semanticManager;

    private Material _shaderMaterial;

    public Material[] shaderVariations;

    public AudioClip[] shaderSounds;

    public AudioSource audioSource;

    //private AudioClip audioClip;

    private Texture2D _semanticTexture;

    int[] channel;
    
    int channelIndexCount;

    public int maxChannelsToActivate;

    public TMP_Text _text;

    void Start()
    {
        channelIndexCount = 0;
        channel = new int[100];
        //audioClip = audioSource.GetComponent<AudioClip>();

        //add a callback for catching the updated semantic buffer
        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;
    }

    //will be called when there is a new buffer
    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
 
        //channel = semanticBuffer.GetChannelIndex("sky");

        //get the channel from the buffer we would like to use using create or update.

        if (channel != null && channelIndexCount != 0)
        {
            

            for(int i = 0; i < channelIndexCount; i++) // tried this to have the different shaders active simultaneously but not working
            {
                //get the buffer that has been surfaced.
                ISemanticBuffer semanticBuffer = args.Sender.AwarenessBuffer;

                semanticBuffer.CreateOrUpdateTextureARGB32(
                       ref _semanticTexture, channel[i] 
                   );
            }
                

            
            

        }
        
            
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        /*
        _shaderMaterial = shaderVariations[channel[channelIndex]];
        //pass in our texture
        //Our Depth Buffer
        _shaderMaterial.SetTexture("_SemanticTex", _semanticTexture);

        //pass in our transform - samplerTransform will translate it to align with screen orientation
        _shaderMaterial.SetMatrix("_semanticTransform", _semanticManager.SemanticBufferProcessor.SamplerTransform);
        */

        if(channelIndexCount != 0)
        {
            // looping the blit so that multiple segments would be updated
            for(int i = 0; i < channelIndexCount; i++)
            {
                shaderVariations[channel[i]].SetTexture("_SemanticTex", _semanticTexture);

                //pass in our transform - samplerTransform will translate it to align with screen orientation
                shaderVariations[channel[i]].SetMatrix("_semanticTransform", _semanticManager.SemanticBufferProcessor.SamplerTransform);

                //blit everything with our shader
                Graphics.Blit(source, destination, shaderVariations[channel[i]]);
            }

        }
        else
        {
            // if there is only one channel then do not loop
            _shaderMaterial = shaderVariations[channel[channelIndexCount]];
            //pass in our texture
            //Our Depth Buffer
            _shaderMaterial.SetTexture("_SemanticTex", _semanticTexture);

            //pass in our transform - samplerTransform will translate it to align with screen orientation
            _shaderMaterial.SetMatrix("_semanticTransform", _semanticManager.SemanticBufferProcessor.SamplerTransform);

            Graphics.Blit(source, destination, _shaderMaterial);
        }


    }

    private void PlayAudio(int soundIndex)
    {
        if(soundIndex <= shaderSounds.Length)
        {
            audioSource.clip = shaderSounds[soundIndex];
            audioSource.Play();
        }
        
    
    }

    private void Update()
    {
        
        if (PlatformAgnosticInput.touchCount <= 0) { return; }

        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            

            int x = (int)touch.position.x;
            int y = (int)touch.position.y;

            //return the indices
            int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(x, y);

            

      
            //print them to console
            foreach (var i in channelsInPixel)
            {
                Debug.Log("channel indices: " + channelIndexCount);
                channel[channelIndexCount] = i; // there can be multiple channels in a pixel which makes slightly wonky

                if (channelIndexCount < maxChannelsToActivate)
                {
                    audioSource.Stop();

                    PlayAudio(i);

                    channelIndexCount = channelIndexCount + 1;

                   

                }
                else
                {
                    channelIndexCount = 0;

                    Array.Clear(channel, 0, channel.Length);

                    audioSource.Stop();
                }

                
            }

            

            //return the names
            string[] channelsNamesInPixel = _semanticManager.SemanticBufferProcessor.GetChannelNamesAt(x, y);

            //print them to console
            foreach (var i in channelsNamesInPixel)
            {
                Debug.Log(i);

                //print to screen
                _text.text = i;
            }

        
           
        }
        
    }
}