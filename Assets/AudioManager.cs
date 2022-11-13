using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXTYPE
{
    pling = 0,
    click,
}

public enum TRACKTYPE
{
    main = 0,
    second
}

public enum NOISETYPE
{
    first1 = 0,
    first2,
    second1,
    second2,
    third1,
    third2,
}

public enum VOICETYPE 
{
    up = 0,
    littleUp,
    down,
    littleDown,
    right,
    littleRight,
    left, 
    littleLeft,
    intro,
    intro2,
    clearLevel1,
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private Coroutine COR_trackFade;
    private AudioSource as_sfx;
    private AudioSource as_voice;
    private AudioSource[] as_tracks;
    private AudioSource[] as_noise;
    private List<GameObject> noiseObjectList;
    
    [Header("Main Tracks")]
    [SerializeField] private AudioClip[] tracks;
    [Header("Voice Over")]
    [SerializeField] private AudioClip[] voices;
    [Header("Noise Tracks")]
    [SerializeField] private AudioClip[] noise;

    private int trackIndex;
    private int noiseIndex;
    private int previousTrackIndex;
    private int currentNoiseIndex = 0;
    [SerializeField] private int noiseObjects;
    [SerializeField] private float trackFadeDuration = 1f;
    [SerializeField] private AnimationCurve fadeCurve;
    [SerializeField] private SFXGroup[] sfxGroups;

    [System.Serializable]
    private class SFXGroup 
    {
        [SerializeField] private string name;
        public AudioClip[] sounds;
        public float[] volumes;
    }

    private void Awake() {
        instance = this;
        noiseObjectList = new List<GameObject>();
        as_tracks = new AudioSource[tracks.Length];
        for(int i = 0; i < 1; i++)
        {
            as_tracks[i] = transform.Find("AS_Track_" + i.ToString()).GetComponent<AudioSource>();
        }

        as_noise = new AudioSource[noise.Length];
        for(int i = 0; i < noiseObjects; i++)
        {
            GameObject noise = transform.Find("AS_Noise_" + i.ToString()).gameObject;
            as_noise[i] = noise.GetComponent<AudioSource>();
            noiseObjectList.Add(noise);
        }

        as_sfx = transform.Find("AS_SFX").GetComponent<AudioSource>();
        as_voice = transform.Find("AS_VOICE").GetComponent<AudioSource>();
    }

    public void RequestSFX(SFXTYPE sfx)
    {
        as_sfx.Stop();
        as_sfx.PlayOneShot(
            sfxGroups[(int)sfx].sounds[Random.Range(0, sfxGroups[(int)sfx].sounds.Length)], 
            Random.Range(sfxGroups[(int)sfx].volumes[0], sfxGroups[(int)sfx].volumes[1])
        );
    }

    public float RequestVoice(VOICETYPE voice)
    {
        as_voice.Stop();
        as_voice.PlayOneShot(voices[(int)voice], 1f);
        return voices[(int)voice].length;
    }

    public GameObject RequestNoise(NOISETYPE noiseID)
    {
        if(noiseIndex > as_noise.Length)
        {
            Debug.Log("Ran out of AudioSources");
            return null;
        }

        as_noise[noiseIndex].clip = noise[(int)noiseID];
        as_noise[noiseIndex].enabled = true;
        as_noise[noiseIndex].loop = true;
        as_noise[noiseIndex].volume = 1f;
        as_noise[noiseIndex].Play();

        GameObject result = as_noise[noiseIndex].gameObject;

        result.GetComponent<AudioHighPassFilter>().enabled = true;
        result.GetComponent<AudioHighPassFilter>().cutoffFrequency = 5000;

        result.GetComponent<AudioLowPassFilter>().enabled = true;
        result.GetComponent<AudioLowPassFilter>().cutoffFrequency = 5000;

        noiseIndex++;

        return result;
    }

    public void ResetIndex() 
    {
        noiseIndex = 0;
    }

    public GameObject GetNextNoiseObject()
    {
        if (noiseObjectList.Count <= 0)
        {
            return null;
        }

        for (int i = 0; i < noiseObjects; i++)
        {
            AudioSource source = as_noise[i];
            source.panStereo = -1;
            StartVolumeFade(source, (float)source.volume, .3f);
        }
        
        if (currentNoiseIndex < noiseObjectList.Count - 1)
        {
            currentNoiseIndex++;
        }
        else if(currentNoiseIndex >= noiseObjectList.Count - 1)
        {
            currentNoiseIndex = 0;
        }

        GameObject result = noiseObjectList[currentNoiseIndex];
        AudioSource currentSource = result.GetComponent<AudioSource>();
        currentSource.panStereo = 1;
        StartVolumeFade(currentSource, currentSource.volume, 1f);
        //source
        
        return result;
    }

    public void ConfirmLevel() 
    {
        for(int i = 0; i < noiseObjectList.Count; i++) 
        {
                noiseObjectList[i].GetComponent<AudioSource>().panStereo = 0;
                noiseObjectList[i].GetComponent<AudioSource>().volume = 1;
        }
    }

    public void RequestTrack(TRACKTYPE track, bool withFade)
    {
        previousTrackIndex = trackIndex;
        trackIndex = Mathf.Abs(trackIndex - 1);
        if(trackIndex > as_tracks.Length) 
        {
            Debug.Log("Ran out of AudioSources");
            return; 
        }

        as_tracks[trackIndex].clip = tracks[(int)track];
        if(!withFade)
        {
            as_tracks[trackIndex].enabled = true;
            as_tracks[trackIndex].volume = 1f;
            as_tracks[trackIndex].Play();
            as_tracks[previousTrackIndex].enabled = false;
            as_tracks[previousTrackIndex].volume = 0f;
            as_tracks[previousTrackIndex].Stop();
        }
        else
        {
            StartTrackFade();
        }
        //trackIndex++;
    }

    void StartTrackFade()
    {
        StopTrackFade();
        COR_trackFade = StartCoroutine(TrackFade());
    }

    void StopTrackFade()
    {
        if(COR_trackFade != null)
        {
            StopCoroutine(COR_trackFade);
        }
        COR_trackFade = null;
    }
    
    void StartVolumeFade(AudioSource source, float sVol, float eVol)
    {
        //StopVolumeFade();
        COR_trackFade = StartCoroutine(VolumeFade(source, sVol, eVol));
    }

    void StopVolumeFade()
    {
        if(COR_trackFade != null)
        {
            StopCoroutine(COR_trackFade);
        }
        COR_trackFade = null;
    }

    IEnumerator VolumeFade(AudioSource source, float sVol, float eVol)
    {
        float passingTime = 0f;
        while(passingTime <= 1f)
        {
            passingTime += Time.fixedDeltaTime / trackFadeDuration;
            for(int i = 0; i < 2; i++)
            {
                source.volume = Mathf.Lerp(sVol, eVol, fadeCurve.Evaluate(passingTime));
            }
            yield return new WaitForFixedUpdate();
        }
        StopVolumeFade();
    }

    IEnumerator TrackFade()
    {
        as_tracks[trackIndex].enabled = true;
        float passingTime = 0f;
        float[] sVols = new float[2] {as_tracks[previousTrackIndex].volume, as_tracks[trackIndex].volume};
        float[] eVols = new float[2] {0f, 1f};
        while(passingTime <= 1f)
        {
            passingTime += Time.fixedDeltaTime / trackFadeDuration;
            for(int i = 0; i < 2; i++)
            {
                as_tracks[previousTrackIndex].volume = Mathf.Lerp(sVols[0], eVols[0], fadeCurve.Evaluate(passingTime));
            }
            yield return new WaitForFixedUpdate();
        }
        as_tracks[previousTrackIndex].enabled = false;
        StopTrackFade();
    }
}
