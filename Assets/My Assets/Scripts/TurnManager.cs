using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MoveManager))]
public class TurnManager : MonoBehaviour {

    [SerializeField]
    private int _MovesPerTurn; //0 implies that we don't care about moves per turn
    public int MovesPerTurn { get { return _MovesPerTurn; } }

    private int currentTurnIndex = 0;
    private Player firstPlayer;


    private MoveManager moveManager;
    private CityGenerator cityGenerator;

    private bool transitioningTurn = false;

    public int CurrentBlueScore { get; private set; }
    public int CurrentRedScore { get; private set; }

    public int NextBlueScore
    {
        get { return CurrentBlueScore + cityGenerator.Districts.Count((c) => { return c.CurrentMajority == Constituent.Party.Blue; }); }
    }
    public int NextRedScore
    {
        get { return CurrentRedScore + cityGenerator.Districts.Count((c) => { return c.CurrentMajority == Constituent.Party.Red; }); }
    }

    //code to deterime which player is currently playing, and which player is up next
    public Player CurrentPlayer
    {
        get
        {
            return (Player)(((int)firstPlayer + currentTurnIndex) % 2);
        }
    }

    public Player NextPlayer
    {
        get
        {
            return (Player)(((int)firstPlayer + currentTurnIndex + 1) % 2);
        }
    }


    public int CurrentRound { get { return currentTurnIndex / 2; } }
    public int NextRound { get { return (currentTurnIndex + 1) / 2; } }

    //Property that returns the number of moves that have been made during the current turn
    public int MovesThisTurn
    {
        get
        {
            return moveManager.MoveHistory.Keys.Count((entry) => { return entry.party != Constituent.Party.None; });
        }
    }

	// Use this for initialization
	void Awake () {
        moveManager = GetComponent<MoveManager>();
        cityGenerator = GetComponent<CityGenerator>();

        firstPlayer = Utils.ChooseRandom(new List<Player> { Player.Red, Player.Blue });

        CurrentBlueScore = 0;
        CurrentRedScore = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if((MovesThisTurn >= MovesPerTurn || transitioningTurn) && moveManager.AllowMoves)
        {
            moveManager.AllowMoves = false;
        }
        else if((MovesThisTurn < MovesPerTurn && !transitioningTurn) && !moveManager.AllowMoves)
        {
            moveManager.AllowMoves = true;
        }
	}

    public void BeginTurnTransition()
    {
        moveManager.UndoStack.Clear();

        transitioningTurn = true;
    }

    public void AdvanceTurn()
    {
        moveManager.LockedConstituents = new HashSet<Constituent>(moveManager.MoveHistory.Keys);

        moveManager.MoveHistory.Clear();
        moveManager.UndoStack.Clear();

        currentTurnIndex += 1;
        transitioningTurn = false;

        CurrentBlueScore = NextBlueScore;
        CurrentRedScore = NextRedScore;
    }

    public enum Player
    {
        Red = 1, Blue = 0,
    }
}
