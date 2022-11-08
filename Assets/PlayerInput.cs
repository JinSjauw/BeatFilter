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
    private int noiseIndex = 0;
    private float inputLow = 0;
    private float inputHigh = 0;
    private bool lowPass, highPass;
    
    void Start() {
        AudioManager.instance.RequestNoise();
        AudioManager.instance.RequestNoise();
        AudioManager.instance.RequestNoise();
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
