using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CompetitionModeUIManager : MonoBehaviour {

    [SerializeField] private Color doneButtonNormalColor;
    [SerializeField] private Color doneButtonHighlightColor;

    [SerializeField] private Color bluesTurnTextColor;
    [SerializeField] private Color redsTurnTextColor;

    [SerializeField] private Color bluesTurnBackgroundColor;
    [SerializeField] private Color redsTurnBackgroundColor;

    [SerializeField] private Text currentTurnTextbox;
    [SerializeField] private Text currentPlayerTextbox;
    [SerializeField] private Text currentMovesTextbox;

    [SerializeField] private Text redScoreTextbox;
    [SerializeField] private Text blueScoreTextbox;

    [SerializeField] private Button endTurnButton;

    [SerializeField] private Image sidePanel;
    [SerializeField] private Image bottomPanel;

    private int displayScoreBlue;
    private int displayScoreRed;

    private TurnManager turnManager;

	// Use this for initialization
	void Start ()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();

        //color the text based on whose turn it is
        var currentPlayer = turnManager.CurrentPlayer;
        currentPlayerTextbox.color = GetTextColorForPlayer(currentPlayer);
        sidePanel.color = GetBackgroundColorForPlayer(currentPlayer);
        bottomPanel.color = GetBackgroundColorForPlayer(currentPlayer);

        currentPlayerTextbox.text = string.Format("{0}'s Turn", currentPlayer.ToString());

        displayScoreBlue = 0;
        displayScoreRed = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        currentTurnTextbox.text = "Round " + (turnManager.CurrentRound + 1).ToString();
        
        //update the "x moves left" textbox
        if (turnManager.MovesPerTurn <= 0)
        {
            endTurnButton.image.color = doneButtonNormalColor;
            currentMovesTextbox.text = "";
        }
        else if(turnManager.MovesThisTurn <= 0)
        {
            endTurnButton.image.color = doneButtonNormalColor;
            currentMovesTextbox.text = string.Format("{0} Moves Left", turnManager.MovesPerTurn - turnManager.MovesThisTurn);
        }
        else
        {

            var movesLeft = turnManager.MovesPerTurn - turnManager.MovesThisTurn;
            if (movesLeft <= 0)
            {
                endTurnButton.image.color = doneButtonHighlightColor;
                currentMovesTextbox.text = "No Moves Left";
            }
            else if (movesLeft == 1)
            {
                endTurnButton.image.color = doneButtonNormalColor;
                currentMovesTextbox.text = "1 Move Left";
            }
            else
            {
                endTurnButton.image.color = doneButtonNormalColor;
                currentMovesTextbox.text = string.Format("{0} Moves Left", turnManager.MovesPerTurn - turnManager.MovesThisTurn);
            }
        }

        //update the score textboxes
        blueScoreTextbox.text = displayScoreBlue.ToString();
        redScoreTextbox.text = displayScoreRed.ToString();
	}

    public void DoneButtonClicked()
    {
        var nextPlayer = turnManager.NextPlayer;
        var targetTextColor = GetTextColorForPlayer(nextPlayer);
        var targetBackgroundColor = GetBackgroundColorForPlayer(nextPlayer);

        float transitionDuration = 1.0f;
        endTurnButton.interactable = false;

        turnManager.BeginTurnTransition();

        LeanTween.textColor(currentPlayerTextbox.rectTransform, Color.black, transitionDuration * 0.5f).setOnComplete(() =>
        {
            currentPlayerTextbox.text = string.Format("{0}'s Turn", nextPlayer.ToString());
            LeanTween.textColor(currentPlayerTextbox.rectTransform, targetTextColor, transitionDuration * 0.5f);
        });

        LeanTween.color(sidePanel.rectTransform, targetBackgroundColor, transitionDuration);
        LeanTween.color(bottomPanel.rectTransform, targetBackgroundColor, transitionDuration);

        if(turnManager.CurrentRound != turnManager.NextRound)
        {
            UpdateScores(transitionDuration);
        }

        Invoke("TurnTransitionsFinished", transitionDuration);
    }

    private void UpdateScores(float transitionDuration)
    {
        var currentScoreBlue = turnManager.BlueScore;
        if (currentScoreBlue != displayScoreBlue)
        {
            System.Action<float> updateBlueScore = (currentValue) =>
            {
                displayScoreBlue = (int)System.Math.Round(currentValue);
            };
            LeanTween.value(gameObject, updateBlueScore, (float)displayScoreBlue, (float)currentScoreBlue, transitionDuration).setEase(LeanTweenType.easeOutCirc);
        }

        var currentScoreRed = turnManager.RedScore;
        if (currentScoreRed != displayScoreRed)
        {
            System.Action<float> updateRedScore = (currentValue) =>
            {
                displayScoreRed = (int)System.Math.Round(currentValue);
            };
            LeanTween.value(gameObject, updateRedScore, (float)displayScoreRed, (float)currentScoreRed, transitionDuration).setEase(LeanTweenType.easeOutCirc);
        }
    }

    private void TurnTransitionsFinished()
    {
        endTurnButton.interactable = true;
        turnManager.AdvanceTurn();
    }

    private Color GetTextColorForPlayer(TurnManager.Player player)
    {
        if (player == TurnManager.Player.Red)
            return redsTurnTextColor;
        else
            return bluesTurnTextColor;
    }

    private Color GetBackgroundColorForPlayer(TurnManager.Player player)
    {
        if (player == TurnManager.Player.Red)
            return redsTurnBackgroundColor;
        else
            return bluesTurnBackgroundColor;
    }
}
