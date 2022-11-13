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
    [SerializeField] private float voiceTimer = 5;
    [SerializeField] private float voiceTimerMinimum = 8;
    private float inputLow = 0;
    private float inputHigh = 0;
    private bool lowPass, highPass;
    private bool levelClear = false;

    public void Confirm(InputAction.CallbackContext context) 
    {
        if (context.performed) 
        {
            if(level == 0) 
            {
                level = 1;
                //Request noise
                //Level 1
                AudioManager.instance.ConfirmLevel();
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                AudioManager.instance.ResetIndex();
                AudioManager.instance.RequestNoise(NOISETYPE.first1);
                GameObject noiseObject = AudioManager.instance.RequestNoise(NOISETYPE.first2);
                noiseObject.GetComponent<AudioLowPassFilter>().enabled = false;
                
                //Game Logic;
            }
            else if(level == 1 && levelClear) 
            {
                level = 2;
                levelClear = false;
                AudioManager.instance.ConfirmLevel();
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                AudioManager.instance.ResetIndex();
                GameObject noiseObject1 = AudioManager.instance.RequestNoise(NOISETYPE.second1);
                GameObject noiseObject2 = AudioManager.instance.RequestNoise(NOISETYPE.second2);

                noiseObject1.GetComponent<AudioHighPassFilter>().enabled = false;
                noiseObject2.GetComponent<AudioHighPassFilter>().enabled = false;
            }
            else if(level == 2) 
            {
                level = 3;
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                AudioManager.instance.ResetIndex();
                AudioManager.instance.ConfirmLevel();
                GameObject noiseObject1 = AudioManager.instance.RequestNoise(NOISETYPE.third1);
                GameObject noiseObject2 = AudioManager.instance.RequestNoise(NOISETYPE.third2);

                noiseObject1.GetComponent<AudioHighPassFilter>().enabled = false;
                noiseObject2.GetComponent<AudioLowPassFilter>().enabled = false;
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
                lowPassFilter = null;
                highPassFilter = null;

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

    private void LevelLogic() 
    {
        if(level > 0) 
        {
            voiceTimer -= Time.deltaTime;
        }

        if(level == 1) 
        {
            if (highPassFilter.cutoffFrequency > 17500) 
            {
                //Pass level 1
                levelClear = true;
                AudioManager.instance.RequestVoice(VOICETYPE.clearLevel1);
                voiceTimer = voiceTimerMinimum;
            }
            else if(highPassFilter.cutoffFrequency > 12000 && voiceTimer < 0) 
            {
                //smidge up
                Debug.Log("Smidge up");
                AudioManager.instance.RequestVoice(VOICETYPE.littleUp);
                voiceTimer = voiceTimerMinimum;
            }
            else if (highPassFilter.cutoffFrequency < 8000 && voiceTimer < 0) 
            {
                //slide up
                AudioManager.instance.RequestVoice(VOICETYPE.up);
                voiceTimer = voiceTimerMinimum;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        LevelLogic();

        if(lowPassFilter && highPassFilter == null) { return; }
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
