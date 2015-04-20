using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    private AudioSource mainCameraSource;

    // Use this for initialization
    void Start()
    {
        mainCameraSource = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AudioSource>();
    }

    public void PlayEffect(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + name);
        mainCameraSource.PlayOneShot(clip);
    }
}
