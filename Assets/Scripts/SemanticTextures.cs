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
using UnityEngine.Audio;
using UnityEngine.UI;

using TMPro;
using Random = UnityEngine.Random;


public class SemanticTextures : MonoBehaviour
{
    //pass in our semantic manager
    public ARSemanticSegmentationManager _semanticManager;

    private Material _shaderMaterial;

    public Material startingMaterial;

    public Material[] shaderVariations;

    public AudioClip[] shaderSounds;

    public AudioSource audioSource;

    private AudioMixer audioMixer;

    private AudioMixerGroup audioMixerGroup;

    private AudioMixerGroup[] audioMixGroup;

    private Texture2D _semanticTexture;

    int[] channel;
    
    int channelIndexCount;

    public int maxChannelsToActivate;

    public TMP_Text _text;

    public DragUI _dragUI;

    Shader startingShader;

    Texture shaderTexture;

    void Start()
    {
        channelIndexCount = 0;
        channel = new int[100];

        audioMixerGroup = audioSource.GetComponent<AudioMixerGroup>();

        audioMixer = Resources.Load<AudioMixer>("AudioMixer");

        //Find AudioMixerGroup you want to load
        audioMixGroup = audioMixer.FindMatchingGroups("Master");

        Debug.Log(audioMixGroup.Length);

        

        shaderTexture = startingMaterial.GetTexture("_SemanticTex");

        _semanticTexture = shaderTexture as Texture2D;

        //add a callback for catching the updated semantic buffer
        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;
    }

    //will be called when there is a new buffer
    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
        if (!_dragUI.hasFirstTouchHappened)
        {

            _semanticTexture = shaderTexture as Texture2D;
        }

        if (channel != null && channelIndexCount != 0)
            {

                for (int i = 0; i < channelIndexCount; i++) // tried this to have the different shaders active simultaneously but not working
                {
                    ISemanticBuffer semanticBuffer = args.Sender.AwarenessBuffer;

                    //for (int j = 0; j < channel.Length; j++)
                    //{
                    //get the buffer that has been surfaced.
                    int randomIndex = Random.Range(1, channel.Length);

                    semanticBuffer.CreateOrUpdateTextureARGB32(
                           ref _semanticTexture, channel[randomIndex]

                       );
                    // }

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

        if (!_dragUI.hasFirstTouchHappened)
        {

            startingMaterial.SetTexture("_SemanticTex", _semanticTexture);

            //pass in our transform - samplerTransform will translate it to align with screen orientation
            startingMaterial.SetMatrix("_semanticTransform", _semanticManager.SemanticBufferProcessor.SamplerTransform);

            //blit everything with our shader
            Graphics.Blit(source, destination, startingMaterial);

            
        }
        else
        {
            if (channelIndexCount != 0)
            {
                // looping the blit so that multiple segments would be updated
                for (int i = 0; i < channelIndexCount; i++)
                {
                    // Debug.Log(i);

                    if (shaderVariations[i] != null)
                    {
                        shaderVariations[channel[i]].SetTexture("_SemanticTex", _semanticTexture);

                        //pass in our transform - samplerTransform will translate it to align with screen orientation
                        shaderVariations[channel[i]].SetMatrix("_semanticTransform", _semanticManager.SemanticBufferProcessor.SamplerTransform);

                        //blit everything with our shader
                        Graphics.Blit(source, destination, shaderVariations[channel[i]]);

                    }


                }

            }
            else
            {

                if (shaderVariations[channelIndexCount] != null)
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
        }


           
        

    }

    private void PlayAudio(int soundIndex)
    {
        if(soundIndex <= shaderSounds.Length)
        {
            audioSource.clip = shaderSounds[soundIndex];
            audioSource.outputAudioMixerGroup = audioMixGroup[soundIndex];

            audioSource.PlayOneShot(audioSource.clip);
           
        }
        
    
    }

    public void ResetSemanticTexture()
    {
        channelIndexCount = 0;

        Array.Clear(channel, 0, channel.Length);

        _semanticTexture = shaderTexture as Texture2D;

        _dragUI.hasFirstTouchHappened = false;
    }


    public void SetSemanticTexture(int x, int y)
    {
        

            //return the indices
            int[] channelsInPixel = _semanticManager.SemanticBufferProcessor.GetChannelIndicesAt(x, y);

            

            if (channelsInPixel !=null)
            {
                //print them to console
                foreach (var i in channelsInPixel)
                {
                    Debug.Log("channel indices: " + channelIndexCount);

               

                    channel[channelIndexCount] = i; // there can be multiple channels in a pixel which makes slightly wonky

                    if (channelIndexCount < maxChannelsToActivate)
                    {
                        //audioSource.Stop();

                        PlayAudio(i);

                        channelIndexCount = channelIndexCount + 1;


                    }
                    else
                    {
                        channelIndexCount = 0;

                        Array.Clear(channel, 0, channel.Length);

                        //audioSource.Stop();
                    }

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