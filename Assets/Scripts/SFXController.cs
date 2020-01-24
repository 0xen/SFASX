using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{
    // A globally accessible instance to the sfx controller, so any script anywhere can request a sfx to be played, even from inspector events
    public static SFXController instance = null;

    // Maximum amount of concurrent sfx's that can be played
    [SerializeField] private int maxChannels = 0;
    // Audio source prefab
    [SerializeField] private AudioSource sfxChannelPrefab = null;
    
    [System.Serializable]
    struct AudioPair
    {
        public string name;
        public AudioClip clip;
    }

    // A array of all sfx names with there respective clip
    [SerializeField] private AudioPair[] audioPairs = null;

    // Look up table used for sfx tracks
    private Dictionary<string,AudioClip> audioLookup = null;

    // All created sfx channels
    private AudioSource[] channels = null;

    // Start is called before the first frame update
    void Start()
    {
        audioLookup = new Dictionary<string, AudioClip>();
        // Loop through all audio clips adding them to the lookup table
        for (int i = 0; i < audioPairs.Length;i++)
        {
            audioLookup.Add(audioPairs[i].name, audioPairs[i].clip);
        }

        // Create all channels that will be used
        channels = new AudioSource[maxChannels];
        for(int i = 0; i < maxChannels; i++)
        {
            channels[i] = GameObject.Instantiate(sfxChannelPrefab);
            channels[i].transform.parent = this.transform;
        }
        // Bind the current SFX Controller instance to the static one
        instance = this;
    }

    // Play sfx by name
    // SFX must have been defined in the SFX inspector
    public bool PlaySFX(string name)
    {
        if (!audioLookup.ContainsKey(name)) return false;
        for (int i = 0; i < channels.Length; i++)
        {
            // If we find a channel that is not playing a sfx, play our sfx here
            if(!channels[i].isPlaying)
            {
                channels[i].clip = audioLookup[name];
                channels[i].Play();
                return true;
            }
        }
        return false;
    }

    // Force stop all audio channels
    public void LocalStop()
    {
        for(int i = 0; i < channels.Length; i++)
        {
            channels[i].Stop();
        }
    }

    // Force stop all audio channels 
    public static void Stop()
    {
        instance.LocalStop();
    }
}
