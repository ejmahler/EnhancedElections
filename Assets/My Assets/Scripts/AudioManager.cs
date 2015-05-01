﻿using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    private AudioSource[] sources;

    [SerializeField]
    private List<AudioClip> scrapeSounds;

    [SerializeField]
    private AudioClip gavelSound;

    [SerializeField]
    private AudioMixer audioMixer;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        sources = new AudioSource[] { 
			transform.FindChild ("Audio Source 1").GetComponent<AudioSource>(),
			transform.FindChild ("Audio Source 2").GetComponent<AudioSource>(),
			transform.FindChild ("Audio Source 3").GetComponent<AudioSource>(),
		};

        SetMusicLevel(PlayerPrefs.GetFloat("MusicVolume", 1.0f));
        SetSFXLevel(PlayerPrefs.GetFloat("SFXVolume", 1.0f));
    }

    public void PlayScrape()
    {
        var shuffledSounds = Utils.RandomShuffle(scrapeSounds);

        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].PlayOneShot(shuffledSounds[i]);
        }
    }

    public void PlayGavel()
    {
        sources[0].PlayOneShot(gavelSound);
    }

    public void SetMusicLevel(float level)
    {
        audioMixer.SetFloat("MusicVolume", convertLevelToDb(level));
    }

    public void SetSFXLevel(float level)
    {

        audioMixer.SetFloat("SFXVolume", convertLevelToDb(level));
    }

    private float convertLevelToDb(float level)
    {
        if (level <= 0.0f)
            return -80.0f;
        else
            return Mathf.Log10(level) * 20.0f;
    }
}
