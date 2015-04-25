using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private AudioSource[] sources;

    [SerializeField]
    private List<AudioClip> scrapeSounds;

    // Use this for initialization
    void Start()
    {
        sources = GetComponentsInChildren<AudioSource>();
    }

    public void PlayScrape()
    {
        var shuffledSounds = Utils.RandomShuffle(scrapeSounds);

        for(int i = 0; i < sources.Length; i++)
        {
            sources[i].PlayOneShot(shuffledSounds[i]);
        }
    }
}
