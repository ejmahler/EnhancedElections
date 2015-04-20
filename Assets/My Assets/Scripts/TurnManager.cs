using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(MoveManager))]
public class TurnManager : MonoBehaviour
{

    [SerializeField]
    private int lockDuration;

    [SerializeField]
    private int _MovesPerTurn;
    public int MovesPerTurn { get { return _MovesPerTurn; } }

    [SerializeField]
    private int _totalRounds;
    public int TotalRounds { get { return _totalRounds; } }

    private int currentTurnIndex = 0;
    public Player firstPlayer;


    private MoveManager moveManager;
    private CityGenerator cityGenerator;

    //dictionarry mapping constituents to the turn index on which that lock expires
    private Dictionary<Constituent, int> lockedConstituents;

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


    public int CurrentRound { get { return currentTurnIndex / 2 + 1; } }
    public int NextRound { get { return (currentTurnIndex + 1) / 2 + 1; } }

    //Property that returns the number of moves that have been made during the current turn
    public int MovesThisTurn
    {
        get
        {
            return moveManager.OriginalDistricts.Keys.Count;
        }
    }

    // Use this for initialization
    void Awake()
    {
        moveManager = GetComponent<MoveManager>();
        cityGenerator = GetComponent<CityGenerator>();

        lockedConstituents = new Dictionary<Constituent, int>();

        firstPlayer = Utils.ChooseRandom(new List<Player> { Player.Red, Player.Blue });

        CurrentBlueScore = 0;
        CurrentRedScore = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (MovesThisTurn >= MovesPerTurn || transitioningTurn)
        {
            moveManager.AllowMoves = false;
        }
        else if (MovesThisTurn < MovesPerTurn && !transitioningTurn)
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
        currentTurnIndex += 1;
        transitioningTurn = false;

        //remove locked constituents if the lock has expired, aka only keep them if their turn hasn't been reached yet
        lockedConstituents = lockedConstituents.Where(pair => pair.Value > currentTurnIndex)
                                 .ToDictionary(pair => pair.Key,
                                               pair => pair.Value);

        //add locked constituents from the move manager's move history
        foreach (var c in moveManager.OriginalDistricts.Keys)
        {
            lockedConstituents.Add(c, currentTurnIndex + lockDuration);
        }

        moveManager.LockedConstituents = new HashSet<Constituent>(lockedConstituents.Keys);

        moveManager.OriginalDistricts.Clear();
        moveManager.UndoStack.Clear();

        CurrentBlueScore = NextBlueScore;
        CurrentRedScore = NextRedScore;

        ConvertUndecideds();
    }

    private void ConvertUndecideds()
    {
        float conversion_cutoff_low = 0.65f;
        float conversion_chance_low = 0.2f;

        float conversion_cutoff_high = 0.8f;
        float conversion_chance_high = 0.4f;

        foreach (District d in cityGenerator.Districts)
        {
            if (d.CurrentMajorityPercent > conversion_cutoff_high)
            {
                foreach (Constituent c in d.ConstituentsQuery.Where((c) => c.party == Constituent.Party.Yellow))
                {
                    if (Utils.Chance(conversion_chance_high))
                        c.party = d.CurrentMajority;
                }
            }
            else if (d.CurrentMajorityPercent > conversion_cutoff_low)
            {
                foreach (Constituent c in d.ConstituentsQuery.Where((c) => c.party == Constituent.Party.Yellow))
                {
                    if (Utils.Chance(conversion_chance_low))
                        c.party = d.CurrentMajority;
                }
            }
            d.UpdateMemberData();
        }
    }

    public enum Player
    {
        Red = 1, Blue = 0,
    }
}
