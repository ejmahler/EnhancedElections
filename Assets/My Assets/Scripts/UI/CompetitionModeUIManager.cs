using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CompetitionModeUIManager : MonoBehaviour {

    [SerializeField] private Color doneButtonNormalColor;
    [SerializeField]  private Color doneButtonHighlightColor;

    [SerializeField]  private Color bluesTurnColor;
    [SerializeField] private Color redsTurnColor;

    [SerializeField] private Text currentTurnTextbox;
    [SerializeField] private Text currentPlayerTextbox;
    [SerializeField] private Text currentMovesTextbox;

    [SerializeField] private Button endTurnButton;

    [SerializeField] private Button undoButton;

    private TurnManager turnManager;
    private MoveManager moveManager;

	// Use this for initialization
	void Start ()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
        moveManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        var currentPlayer = turnManager.CurrentPlayer;
        currentPlayerTextbox.text = string.Format("{0}'s Turn", currentPlayer.ToString());

        //color the text based on whose turn it is
        if (currentPlayer == TurnManager.Player.Red)
            currentPlayerTextbox.color = redsTurnColor;
        else
            currentPlayerTextbox.color = bluesTurnColor;

        currentTurnTextbox.text = "Turn " + (turnManager.CurrentRound + 1).ToString();
        
        //update the "x moves left" textbox
        if (turnManager.MovesPerTurn <= 0)
        {
            currentMovesTextbox.text = "";
            endTurnButton.interactable = false;
        }
        else if(turnManager.MovesThisTurn <= 0)
        {
            endTurnButton.interactable = false;
            currentMovesTextbox.text = string.Format("{0} Moves Left", turnManager.MovesPerTurn - turnManager.MovesThisTurn);
        }
        else
        {
            endTurnButton.interactable = true;

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

        //enable/disable the undo button
        if(moveManager.UndoStack.Count > 0)
        {
            undoButton.interactable = true;
        }
        else
        {
            undoButton.interactable = false;
        }
	}

    public void DoneButtonClicked()
    {
        
    }
}
