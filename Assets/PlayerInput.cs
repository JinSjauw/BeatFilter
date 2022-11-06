using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private AudioLowPassFilter lowPassFilter;
    [SerializeField] private AudioHighPassFilter highPassFilter;

    [SerializeField] private float stepAmount = 100f;
    private int noiseIndex = 0;
    
    void Start() {
        //AudioManager.instance.RequestTrack(TRACKTYPE.second, true);
        AudioManager.instance.RequestNoise();
        AudioManager.instance.RequestNoise();
        AudioManager.instance.RequestNoise();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameObject nextNoise = AudioManager.instance.GetNextNoiseObject();
            
            if (nextNoise == null)
            {
                return;
            }
            
            AudioManager.instance.RequestSFX(SFXTYPE.pling);
            lowPassFilter = nextNoise.GetComponent<AudioLowPassFilter>();
            highPassFilter = nextNoise.GetComponent<AudioHighPassFilter>();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            AudioManager.instance.RequestSFX(SFXTYPE.click);
            AudioManager.instance.OnConfirm();
        }
        
        if(Input.GetKey(KeyCode.UpArrow))
        {
            float cutoffFrequency = lowPassFilter.cutoffFrequency;
            lowPassFilter.cutoffFrequency = cutoffFrequency + stepAmount * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.DownArrow))
        {
            float cutoffFrequency = lowPassFilter.cutoffFrequency;
            lowPassFilter.cutoffFrequency = cutoffFrequency - stepAmount * Time.deltaTime;
        }

         if(Input.GetKey(KeyCode.RightArrow))
        {
            float cutoffFrequency = highPassFilter.cutoffFrequency;
            highPassFilter.cutoffFrequency = cutoffFrequency + stepAmount * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            float cutoffFrequency = highPassFilter.cutoffFrequency;
            highPassFilter.cutoffFrequency = cutoffFrequency - stepAmount * Time.deltaTime;
        }
    }
}
