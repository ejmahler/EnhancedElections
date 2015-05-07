using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuSettings : MonoBehaviour
{

    [SerializeField]
    private Slider musicVolumeSlider;

    [SerializeField]
    private Slider sfxVolumeSlider;

    private AudioManager audioManager;

    // Use this for initialization
    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        //musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        audioManager.SetMusicLevel(0.0f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
    }

    public void OnMusicVolumeChanged()
    {
        float newValue = musicVolumeSlider.value;

        PlayerPrefs.SetFloat("MusicVolume", newValue);
        audioManager.SetMusicLevel(newValue);
    }

    public void OnSFXVolumeChanged()
    {
        float newValue = sfxVolumeSlider.value;

        PlayerPrefs.SetFloat("SFXVolume", newValue);
        audioManager.SetSFXLevel(newValue);
    }
}
