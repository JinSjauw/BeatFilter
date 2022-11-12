using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private AudioLowPassFilter lowPassFilter;
    [SerializeField] private AudioHighPassFilter highPassFilter;

    [SerializeField] private float stepAmount = 100f;
    [SerializeField] private int level = 0;
    private float inputLow = 0;
    private float inputHigh = 0;
    private bool lowPass, highPass;

    public void Confirm(InputAction.CallbackContext context) 
    {
        if (context.performed) 
        {
            AudioManager.instance.RequestSFX(SFXTYPE.click);
            if(level == 0) 
            {
                level = 1;
                //Request noise
                //Level 1
                AudioManager.instance.ResetIndex();
                AudioManager.instance.RequestNoise(NOISETYPE.first1);
                AudioManager.instance.RequestNoise(NOISETYPE.first2);
            }
            else if(level == 1) 
            {
                level = 2;
                AudioManager.instance.ResetIndex();
                AudioManager.instance.RequestNoise(NOISETYPE.second1);
                AudioManager.instance.RequestNoise(NOISETYPE.second2);
            }
            else if(level == 2) 
            {
                level = 3;
                AudioManager.instance.ResetIndex();
                AudioManager.instance.RequestNoise(NOISETYPE.third1);
                AudioManager.instance.RequestNoise(NOISETYPE.third2);
            }
        }
    }
    
    public void NextTrack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AudioManager.instance.RequestSFX(SFXTYPE.pling);
            GameObject nextNoise = AudioManager.instance.GetNextNoiseObject();
            
            if (nextNoise == null)
            {
                return;
            }
            
            lowPassFilter = nextNoise.GetComponent<AudioLowPassFilter>();
            highPassFilter = nextNoise.GetComponent<AudioHighPassFilter>();
        }
    }

    public void LowFrequency(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputLow = context.ReadValue<float>();
            lowPass = true;
        }

        if (context.canceled)
        {
            lowPass = false;
        }
    }
    
    public void HighFrequency(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputHigh = context.ReadValue<float>();
            highPass = true;
        }

        if (context.canceled)
        {
            highPass = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lowPass)
        {
            float cutoffFrequency = lowPassFilter.cutoffFrequency;
            lowPassFilter.cutoffFrequency = cutoffFrequency + stepAmount * inputLow * Time.deltaTime;
        }
        if (highPass)
        {
            float cutoffFrequency = highPassFilter.cutoffFrequency;
            highPassFilter.cutoffFrequency = cutoffFrequency + stepAmount * inputHigh * Time.deltaTime;
        }
    }
}
