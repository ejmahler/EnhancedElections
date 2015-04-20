using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreUIManager : MonoBehaviour
{

    [SerializeField]
    private Text redScoreTextbox;
    [SerializeField]
    private Text blueScoreTextbox;

    private int displayScoreBlue;
    private int displayScoreRed;

    private TurnManager turnManager;

    // Use this for initialization
    void Start()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();

        displayScoreBlue = 0;
        displayScoreRed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        blueScoreTextbox.text = displayScoreBlue.ToString();
        redScoreTextbox.text = displayScoreRed.ToString();
    }

    public void UpdateScoreDisplay(float transitionDuration)
    {
        var currentScoreBlue = turnManager.NextBlueScore;
        if (currentScoreBlue != displayScoreBlue)
        {
            System.Action<float> updateBlueScore = (currentValue) =>
            {
                displayScoreBlue = (int)System.Math.Round(currentValue);
            };
            LeanTween.value(gameObject, updateBlueScore, (float)displayScoreBlue, (float)currentScoreBlue, transitionDuration).setEase(LeanTweenType.easeOutCirc);
        }

        var currentScoreRed = turnManager.NextRedScore;
        if (currentScoreRed != displayScoreRed)
        {
            System.Action<float> updateRedScore = (currentValue) =>
            {
                displayScoreRed = (int)System.Math.Round(currentValue);
            };
            LeanTween.value(gameObject, updateRedScore, (float)displayScoreRed, (float)currentScoreRed, transitionDuration).setEase(LeanTweenType.easeOutCirc);
        }
    }
}
