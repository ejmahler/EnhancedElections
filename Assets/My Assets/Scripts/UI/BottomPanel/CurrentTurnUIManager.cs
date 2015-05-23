using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurrentTurnUIManager : MonoBehaviour
{

    [SerializeField]
    private Color bluesTurnTextColor;
    [SerializeField]
    private Color redsTurnTextColor;

    [SerializeField]
    private Text currentTurnTextbox;
    [SerializeField]
    private Text currentPlayerTextbox;

    private TurnManager turnManager;

    // Use this for initialization
    void Start()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();

        //color the text based on whose turn it is
        var currentPlayer = turnManager.CurrentPlayer;
        currentPlayerTextbox.color = GetTextColorForPlayer(currentPlayer);

        currentPlayerTextbox.text = string.Format("{0}'s Turn", currentPlayer.ToString());
        currentTurnTextbox.text = string.Format("Round {0} of {1}", (turnManager.CurrentRound).ToString(), turnManager.TotalRounds);
    }

    public void UpdateTurnDisplay(float transitionDuration)
    {
        var nextPlayer = turnManager.NextPlayer;
        var targetTextColor = GetTextColorForPlayer(nextPlayer);
        var nextRound = turnManager.NextRound.ToString();

        LeanTween.textColor(currentPlayerTextbox.rectTransform, Color.black, transitionDuration * 0.5f).setOnComplete(() =>
        {
            currentTurnTextbox.text = string.Format("Round {0} of {1}", nextRound, turnManager.TotalRounds);
            currentPlayerTextbox.text = string.Format("{0}'s Turn", nextPlayer.ToString());
            LeanTween.textColor(currentPlayerTextbox.rectTransform, targetTextColor, transitionDuration * 0.5f);
        });
    }

    private Color GetTextColorForPlayer(TurnManager.Player player)
    {
        if (player == TurnManager.Player.Red)
            return redsTurnTextColor;
        else
            return bluesTurnTextColor;
    }
}
