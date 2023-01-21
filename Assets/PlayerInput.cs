using System;
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
    private Coroutine COR_levelEnd, COR_intro;
    private float inputLow = 0;
    private float inputHigh = 0;
    private bool lowPass, highPass;
    private bool levelClear, nextLevel = false;
    [SerializeField] private bool lowFrequency, highFrequency = false;
    private bool running, start = false;

    private void Start()
    {
        StartIntro();
    }

    public void Confirm(InputAction.CallbackContext context) 
    {
        if (context.performed) 
        {
            StopIntro();
            
            if (!start)
            {
                return;
            }
            
            if(level == 0)
            {
                running = true;
                level = 1;
                //Request noise
                //Level 1
                AudioManager.instance.ConfirmLevel();
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                AudioManager.instance.ResetIndex();
                GameObject noiseObject1 = AudioManager.instance.RequestNoise(NOISETYPE.first1);
                GameObject noiseObject2 = AudioManager.instance.RequestNoise(NOISETYPE.first2);
                noiseObject1.GetComponent<AudioLowPassFilter>().enabled = false;
                noiseObject1.GetComponent<AudioHighPassFilter>().enabled = false;
                noiseObject1.GetComponent<AudioSource>().volume = .150f;
                
                noiseObject2.GetComponent<AudioLowPassFilter>().enabled = false;
            }
            else if(level == 1 && nextLevel) 
            {
                running = true;
                level = 2;
                nextLevel = false;
                AudioManager.instance.ConfirmLevel();
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                AudioManager.instance.ResetIndex();
                GameObject noiseObject1 = AudioManager.instance.RequestNoise(NOISETYPE.second1);
                GameObject noiseObject2 = AudioManager.instance.RequestNoise(NOISETYPE.second2);

                noiseObject1.GetComponent<AudioHighPassFilter>().enabled = false;
                noiseObject2.GetComponent<AudioLowPassFilter>().enabled = false;
                highPassFilter = noiseObject1.GetComponent<AudioHighPassFilter>();
                lowPassFilter = noiseObject1.GetComponent<AudioLowPassFilter>();
            }
            else if(level == 2 && nextLevel) 
            {
                running = true;
                level = 3;
                nextLevel = false;
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                AudioManager.instance.ResetIndex();
                AudioManager.instance.ConfirmLevel();
                GameObject noiseObject1 = AudioManager.instance.RequestNoise(NOISETYPE.third1);
                GameObject noiseObject2 = AudioManager.instance.RequestNoise(NOISETYPE.third2);

                noiseObject1.GetComponent<AudioHighPassFilter>().enabled = false;
                noiseObject2.GetComponent<AudioLowPassFilter>().enabled = false;
                highPassFilter = noiseObject1.GetComponent<AudioHighPassFilter>();
                lowPassFilter = noiseObject1.GetComponent<AudioLowPassFilter>();
            }
        }
    }
    
    public void NextTrack(InputAction.CallbackContext context)
    {
        if (context.performed && running)
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
        if (context.started && running)
        {
            inputLow = context.ReadValue<float>();
            lowPass = true;
        }
        
        if (lowPassFilter.cutoffFrequency - autoCorrect < 900 && level == 2)
        {
            lowPassFilter.cutoffFrequency = 900;
            if (!lowFrequency)
            {
                //pling
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                lowFrequency = true;
            }
        }
        
        if (lowPassFilter.cutoffFrequency - autoCorrect < 170 && level == 3)
        {
            lowPassFilter.cutoffFrequency = 190;
            if (!lowFrequency)
            {
                //pling
                AudioManager.instance.RequestSFX(SFXTYPE.click);
                lowFrequency = true;
            }
        }

        if (context.canceled)
        {
            lowPass = false;
        }
    }
    
    public void HighFrequency(InputAction.CallbackContext context)
    {
        if (context.started && running)
        {
            inputHigh = context.ReadValue<float>();
            highPass = true;
            
            if (highPassFilter.cutoffFrequency + autoCorrect > 18000 && level == 1)
            {
                //Pass level 1
                
                highPassFilter.cutoffFrequency = 18000;
                levelClear = true;
                if (levelClear)
                {
                    AudioManager.instance.RequestSFX(SFXTYPE.click);
                }
                //voiceTimer = voiceTimerMinimum;
            }
            
            if (highPassFilter.cutoffFrequency + autoCorrect > 22000 && level == 2)
            {
                highPassFilter.cutoffFrequency = 22000;
                //pling
                if (!highFrequency)
                {
                    AudioManager.instance.RequestSFX(SFXTYPE.click);
                    highFrequency = true;
                }
            }
            
            if (highPassFilter.cutoffFrequency - autoCorrect < 4600 && level == 3)
            {
                highPassFilter.cutoffFrequency = 4600;
                //pling
                if (!highFrequency)
                {
                    AudioManager.instance.RequestSFX(SFXTYPE.click);
                    highFrequency = true;
                }
            }
            
        }

        if (context.canceled)
        {
            highPass = false;
        }
    }

    private void LevelLogic() 
    {
        if(running) 
        {
            voiceTimer -= Time.deltaTime;
        }

        if(level == 1) 
        {
            if(highPassFilter.gameObject.name != "AS_Noise_1") 
            {
                return;
            }

            if (levelClear)
            {
                levelClear = false;
                StartLevelEnd(VOICETYPE.clearLevel1);
            }
            
            if(highPassFilter.cutoffFrequency > 12000 && voiceTimer < 0 && highPassFilter.enabled) 
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
            if (levelClear)
            {
                levelClear = false;
                StartLevelEnd(VOICETYPE.clearLevel2);
            }
            
            if (highFrequency && lowFrequency)
            {
                //Pass level 2
                levelClear = true;
                highFrequency = false;
                lowFrequency = false;
            }
            
            if (highPassFilter.cutoffFrequency > 18000 && voiceTimer < 0 && !highFrequency && highPassFilter.enabled)
            {
                //smidge up
                AudioManager.instance.RequestVoice(VOICETYPE.littleUp);
                voiceTimer = voiceTimerMinimum;
            }
            else if (highPassFilter.cutoffFrequency < 12000 && voiceTimer < 0 && !highFrequency && highPassFilter.enabled)
            {
                //slide up
                AudioManager.instance.RequestVoice(VOICETYPE.up);
                voiceTimer = voiceTimerMinimum;
            }

            if (lowPassFilter.cutoffFrequency < 2000 && voiceTimer < 0 && !lowFrequency && lowPassFilter.enabled)
            {
                //smidge left
                AudioManager.instance.RequestVoice(VOICETYPE.littleLeft);
                voiceTimer = voiceTimerMinimum;
            }
            else if (lowPassFilter.cutoffFrequency < 5000 && voiceTimer < 0 && !lowFrequency && lowPassFilter.enabled)
            {
                //slide left
                AudioManager.instance.RequestVoice(VOICETYPE.left);
                voiceTimer = voiceTimerMinimum;
            }
        }
        
        //Level 3
        if (level == 3)
        {
            if (levelClear)
            {
                levelClear = false;
                StartLevelEnd(VOICETYPE.clearLevel3);
            }
            
            if (highFrequency && lowFrequency)
            {
                //Pass level 3
                levelClear = true;
                highFrequency = false;
                lowFrequency = false;
            }
            
            if (highPassFilter.cutoffFrequency > 12000 && voiceTimer < 0 && !highFrequency && highPassFilter.enabled)
            {
                //smidge up
                AudioManager.instance.RequestVoice(VOICETYPE.down);
                voiceTimer = voiceTimerMinimum;
            }
            else if (highPassFilter.cutoffFrequency > 6000 && voiceTimer < 0 && !highFrequency && highPassFilter.enabled)
            {
                //slide up
                AudioManager.instance.RequestVoice(VOICETYPE.littleDown);
                voiceTimer = voiceTimerMinimum;
            }

            if (lowPassFilter.cutoffFrequency > 3000  && voiceTimer < 0 && !lowFrequency && lowPassFilter.enabled)
            {
                //smidge left
                AudioManager.instance.RequestVoice(VOICETYPE.left);
                voiceTimer = voiceTimerMinimum;
            }
            else if (lowPassFilter.cutoffFrequency > 1500 && voiceTimer < 0 && !lowFrequency && lowPassFilter.enabled)
            {
                //slide left
                AudioManager.instance.RequestVoice(VOICETYPE.littleLeft);
                voiceTimer = voiceTimerMinimum;
            }
        }
    }

    void StartLevelEnd(VOICETYPE voice)
    {
        StopLevelEnd();
        running = false;
        COR_levelEnd = StartCoroutine(LevelEnd(voice));
    }

    void StopLevelEnd()
    {
        if(COR_levelEnd != null)
        {
            StopCoroutine(COR_levelEnd);
        }
        COR_levelEnd = null;
    }
    
    private IEnumerator LevelEnd(VOICETYPE voice)
    {
        AudioManager.instance.RequestVoice(voice);
        float voiceTime = AudioManager.instance.getClipLength(voice);
        yield return new WaitForSeconds(voiceTime);
        nextLevel = true;
        
        StopLevelEnd();
    }
    
    void StartIntro()
    {
        StopLevelEnd();
        running = false;
        COR_intro = StartCoroutine(Intro());
    }

    void StopIntro()
    {
        if(COR_intro != null)
        {
            if (!start)
            {
                start = true;
                AudioManager.instance.StopVoice();   
            }
            StopCoroutine(COR_intro);
        }
        COR_intro = null;
    }
    
    private IEnumerator Intro()
    {
        AudioManager.instance.RequestVoice(VOICETYPE.introUitleg);
        float voiceTime = AudioManager.instance.getClipLength(VOICETYPE.introUitleg);
        yield return new WaitForSeconds(voiceTime);
        
        AudioManager.instance.RequestVoice(VOICETYPE.intro);
        voiceTime = AudioManager.instance.getClipLength(VOICETYPE.intro);
        yield return new WaitForSeconds(voiceTime);
        
        GameObject noise1 = AudioManager.instance.RequestNoise(NOISETYPE.first1);
        GameObject noise2 = AudioManager.instance.RequestNoise(NOISETYPE.first2);
        yield return new WaitForSeconds(.7f); 
        noise1.GetComponent<AudioSource>().Stop();
        noise2.GetComponent<AudioSource>().Stop();

        AudioManager.instance.RequestVoice(VOICETYPE.intro2);
        voiceTime = AudioManager.instance.getClipLength(VOICETYPE.intro2);
        yield return new WaitForSeconds(voiceTime);

        AudioManager.instance.RequestVoice(VOICETYPE.pressEnter);
        voiceTime = AudioManager.instance.getClipLength(VOICETYPE.pressEnter);
        yield return new WaitForSeconds(voiceTime);

        start = true;
        
        StopIntro();
    }
    
    // Update is called once per frame
    void Update()
    {
        LevelLogic();

        if(lowPassFilter == null && highPassFilter == null) { return; }
        
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
