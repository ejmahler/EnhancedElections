using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ImageCycle : MonoBehaviour
{

    [SerializeField]
    private List<Image> images;

    [SerializeField]
    private float cycleTime;

    [SerializeField]
    private float transitionTime;

    private int currentImage = 0;

    // Use this for initialization
    void Start()
    {
        images[currentImage].color = Color.white;

        InvokeRepeating("NextImage", cycleTime, cycleTime);
    }

    private void NextImage()
    {
        int previousImage = currentImage;
        currentImage = (currentImage + 1) % images.Count;
        int nextImage = currentImage;

        //crossfade the two images
        LeanTween.alpha(images[previousImage].rectTransform, 0.0f, transitionTime).setEase(LeanTweenType.easeInQuad);
        LeanTween.alpha(images[nextImage].rectTransform, 1.0f, transitionTime).setEase(LeanTweenType.easeOutQuad);
    }
}
