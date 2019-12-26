using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    public static SFXController instance;
    [SerializeField] private int maxChannels;

    [SerializeField] private AudioSource sfxChannelPrefab;

    [System.Serializable]
    struct AudioPair
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private AudioPair[] audioPairs;
    private Dictionary<string,AudioClip> audioLookup;

    private AudioSource[] channels;

    // Start is called before the first frame update
    void Start()
    {
        audioLookup = new Dictionary<string, AudioClip>();
        for (int i = 0; i < audioPairs.Length;i++)
        {
            audioLookup.Add(audioPairs[i].name, audioPairs[i].clip);
        }


        channels = new AudioSource[maxChannels];
        for(int i = 0; i < maxChannels; i++)
        {
            channels[i] = GameObject.Instantiate(sfxChannelPrefab);
            channels[i].transform.parent = this.transform;
        }
        instance = this;
    }

    public bool PlaySFX(string name)
    {
        if (!audioLookup.ContainsKey(name)) return false;
        for (int i = 0; i < channels.Length; i++)
        {
            if(!channels[i].isPlaying)
            {
                channels[i].clip = audioLookup[name];
                channels[i].Play();
                return true;
            }
        }
        return false;
    }

    public void LocalStop()
    {
        for(int i = 0; i < channels.Length; i++)
        {
            channels[i].Stop();
        }
    }

    public static void Stop()
    {
        instance.LocalStop();
    }
}
