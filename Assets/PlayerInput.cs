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
    [SerializeField] private float voiceTimer, levelEndTimer = 5;
    [SerializeField] private float voiceTimerMinimum = 8;
    [SerializeField] private int autoCorrect;
    private float inputLow = 0;
    private float inputHigh = 0;
    private bool lowPass, highPass;
    private bool levelClear = false;
    private bool lowFrequency, highFrequency = false;

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
                noiseObject2.GetComponent<AudioLowPassFilter>().enabled = false;
            }
            else if(level == 2) 
            {
                level = 3;
                levelClear = false;
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
            if(highPassFilter.gameObject.name != "AS_Noise_1") 
            {
                return;
            }

            if(highPassFilter.cutoffFrequency + autoCorrect > 18000) 
            {
                highPassFilter.cutoffFrequency = 18000;
            }

            if (highPassFilter.cutoffFrequency > 17500) 
            {
                //Pass level 1

                levelEndTimer = AudioManager.instance.RequestVoice(VOICETYPE.clearLevel1);
                levelEndTimer -= Time.deltaTime;
                if(levelEndTimer < 0) 
                {
                    levelClear = true;
                }

                voiceTimer = voiceTimerMinimum;
            }
            else if(highPassFilter.cutoffFrequency > 12000 && voiceTimer < 0 && highPassFilter.enabled) 
            {
                //smidge up
                AudioManager.instance.RequestVoice(VOICETYPE.littleUp);
                voiceTimer = voiceTimerMinimum;
            }
            else if (highPassFilter.cutoffFrequency < 8000 && voiceTimer < 0 && highPassFilter.enabled) 
            {
                //slide up
                AudioManager.instance.RequestVoice(VOICETYPE.up);
                voiceTimer = voiceTimerMinimum;
            }
        }
        //Level 2
        if (level == 2)
        {
            if (highPassFilter.cutoffFrequency + autoCorrect > 22000)
            {
                highPassFilter.cutoffFrequency = 22000;
                highFrequency = true;
            }

            if (lowPassFilter.cutoffFrequency - autoCorrect < 900)
            {
                lowPassFilter.cutoffFrequency = 900;
                lowFrequency = true;
            }

            if (highFrequency && lowFrequency)
            {
                //Pass level 2
                levelClear = true;
                AudioManager.instance.RequestVoice(VOICETYPE.clearLevel1);
                voiceTimer = voiceTimerMinimum;
            }
            else if (highPassFilter.cutoffFrequency > 18000 && voiceTimer < 0)
            {
                //smidge up
                AudioManager.instance.RequestVoice(VOICETYPE.littleUp);
                voiceTimer = voiceTimerMinimum;
            }
            else if (highPassFilter.cutoffFrequency < 12000 && voiceTimer < 0)
            {
                //slide up
                AudioManager.instance.RequestVoice(VOICETYPE.up);
                voiceTimer = voiceTimerMinimum;
            }

            if (lowPassFilter.cutoffFrequency < 2000 && voiceTimer < 0)
            {
                //smidge left
                AudioManager.instance.RequestVoice(VOICETYPE.littleLeft);
                voiceTimer = voiceTimerMinimum;
            }
            else if (lowPassFilter.cutoffFrequency < 5000 && voiceTimer < 0)
            {
                //slide left
                AudioManager.instance.RequestVoice(VOICETYPE.left);
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
