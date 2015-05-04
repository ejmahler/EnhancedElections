using UnityEngine;
using System.Collections;

public class LevelInit : MonoBehaviour
{

    // Use this for initialization
    void Awake()
    {
        LeanTween.init(800);
    }
}
