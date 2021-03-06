﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EndTurnUIManager : MonoBehaviour
{

    [SerializeField]
    private Color doneButtonNormalColor;
    [SerializeField]
    private Color doneButtonHighlightColor;

    [SerializeField]
    private Text currentMovesTextbox;

    [SerializeField]
    private Button endTurnButton;

    private TurnManager turnManager;

    // Use this for initialization
    void Start()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
    }

    // Update is called once per frame
    void Update()
    {

        //update the "x moves left" textbox
        if( turnManager.Mode == TurnManager.GameMode.AI && turnManager.CurrentPlayer == turnManager.LocalHumanPlayer)
        {
            endTurnButton.image.color = doneButtonNormalColor;
            currentMovesTextbox.text = "Opponent's Turn";
        }
        else if (turnManager.MovesPerTurn <= 0)
        {
            endTurnButton.image.color = doneButtonNormalColor;
            currentMovesTextbox.text = "";
        }
        else if (turnManager.MovesThisTurn <= 0)
        {
            endTurnButton.image.color = doneButtonNormalColor;
            currentMovesTextbox.text = string.Format("{0} Moves Remaining", turnManager.MovesPerTurn - turnManager.MovesThisTurn);
        }
        else
        {

            var movesLeft = turnManager.MovesPerTurn - turnManager.MovesThisTurn;
            if (movesLeft <= 0)
            {
                endTurnButton.image.color = doneButtonHighlightColor;
                currentMovesTextbox.text = "No Moves Remaining";
            }
            else if (movesLeft == 1)
            {
                endTurnButton.image.color = doneButtonNormalColor;
                currentMovesTextbox.text = "1 Move Remaining";
            }
            else
            {
                endTurnButton.image.color = doneButtonNormalColor;
                currentMovesTextbox.text = string.Format("{0} Moves Remaining", turnManager.MovesPerTurn - turnManager.MovesThisTurn);
            }
        }
    }

    public void SetEndTurnButtonInteractable(bool interactable)
    {
        endTurnButton.interactable = interactable;
    }
}
