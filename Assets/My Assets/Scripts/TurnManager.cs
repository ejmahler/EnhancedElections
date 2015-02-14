using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MoveManager))]
public class TurnManager : MonoBehaviour {

    private MoveManager moveManager;

    public int MovesThisTurn
    {
        get
        {
            return moveManager.MoveHistory.Keys.Count((entry) => { return entry.party != Constituent.Party.None; });
        }
    }

    [SerializeField]
    private int _MovesPerTurn; //0 implies that we don't care about moves per turn
    public int MovesPerTurn { get { return _MovesPerTurn; } }

    private int currentTurnIndex = 0;
    private Player firstPlayer;
    public Player CurrentPlayer
    {
        get
        {
            int currentSide = currentTurnIndex % 2;
            return (Player)(((int)firstPlayer + currentSide) % 2);
        }
    }
    public int CurrentRound { get { return currentTurnIndex / 2; } }

	// Use this for initialization
	void Start () {
        moveManager = GetComponent<MoveManager>();

        firstPlayer = Utils.ChooseRandom(new List<Player> { Player.Red, Player.Blue });
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(MovesThisTurn >= MovesPerTurn && moveManager.AllowMoves)
        {
            moveManager.AllowMoves = false;
        }
        else if(MovesThisTurn < MovesPerTurn && !moveManager.AllowMoves)
        {
            moveManager.AllowMoves = true;
        }
	}

    public void AdvanceTurn()
    {
        currentTurnIndex += 1;
        moveManager.MoveHistory.Clear();
        moveManager.UndoStack.Clear();
    }



    public enum Player
    {
        Red = 0, Blue = 1,
    }
}
