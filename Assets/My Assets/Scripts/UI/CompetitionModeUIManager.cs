using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompetitionModeUIManager : MonoBehaviour {

    [SerializeField] private Color bluesTurnBackgroundColor;
    [SerializeField] private Color redsTurnBackgroundColor;

    [SerializeField] 
    private List<Image> backgroundPanels;

    private CurrentTurnUIManager currentTurnUIManager;
    private ScoreUIManager scoreUIManager;
    private EndTurnUIManager endTurnUIManager;

    private AudioManager audioManager;

    private TurnManager turnManager;

	// Use this for initialization
	void Start ()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
        currentTurnUIManager = GetComponentInChildren<CurrentTurnUIManager>();
        scoreUIManager = GetComponentInChildren<ScoreUIManager>();
        endTurnUIManager = GetComponentInChildren<EndTurnUIManager>();

        audioManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>();

        //color the text based on whose turn it is
        var currentBackgroundColor = GetBackgroundColorForPlayer(turnManager.CurrentPlayer);
        foreach(var panel in backgroundPanels)
        {
            panel.color = currentBackgroundColor;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        
	}

    public void AdvanceTurn()
    {
        //prevent the player from triggering this stuff again or undoing any of their actions
        endTurnUIManager.SetEndTurnButtonInteractable(false);
        turnManager.BeginTurnTransition();

        float transitionDuration = 1.0f;

        //alert child ui managers to update their information
        currentTurnUIManager.UpdateTurnDisplay(transitionDuration);
        scoreUIManager.UpdateScoreDisplay(transitionDuration);

        //update the background colors of the UI panels
        var nextPlayer = turnManager.NextPlayer;
        var targetBackgroundColor = GetBackgroundColorForPlayer(nextPlayer);

        foreach (var panel in backgroundPanels)
        {
            LeanTween.color(panel.rectTransform, targetBackgroundColor, transitionDuration);
        }

        //after the transition duration, actually go through with ending the turn
        Invoke("TurnTransitionsFinished", transitionDuration);
    }

    private void TurnTransitionsFinished()
    {
        endTurnUIManager.SetEndTurnButtonInteractable(true);
        turnManager.AdvanceTurn();
    }

    private Color GetBackgroundColorForPlayer(TurnManager.Player player)
    {
        if (player == TurnManager.Player.Red)
            return redsTurnBackgroundColor;
        else
            return bluesTurnBackgroundColor;
    }
}
