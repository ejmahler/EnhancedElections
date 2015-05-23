using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIModeUIManager : MonoBehaviour
{

    [SerializeField]
    private Color bluesTurnBackgroundColor;
    [SerializeField]
    private Color redsTurnBackgroundColor;

    [SerializeField]
    private List<Image> backgroundPanels;

    [SerializeField]
    private GameObject endScreenPrefab;

    private CurrentTurnUIManager currentTurnUIManager;
    private EndTurnUIManager endTurnUIManager;

    private AudioManager audioManager;
    private AIManager aiManager;

    private TurnManager turnManager;
    private CityGenerator cityGenerator;

    // Use this for initialization
    void Start()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();
        aiManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<AIManager>();
        currentTurnUIManager = GetComponentInChildren<CurrentTurnUIManager>();
        endTurnUIManager = GetComponentInChildren<EndTurnUIManager>();

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        //color the background based on the human player
        var currentBackgroundColor = GetBackgroundColorForPlayer(turnManager.NextPlayer);
        foreach (var panel in backgroundPanels)
        {
            panel.color = currentBackgroundColor;
        }

        endTurnUIManager.SetEndTurnButtonInteractable(false);

        StartCoroutine(AITurn());
    }

    public void AdvanceTurn()
    {
        audioManager.PlayGavel();

        float transitionDuration = 1.0f;

        //alert child ui managers to update their information
        currentTurnUIManager.UpdateTurnDisplay(transitionDuration);

        //prevent the player from triggering this stuff again or undoing any of their actions
        endTurnUIManager.SetEndTurnButtonInteractable(false);

        if (turnManager.NextRound > turnManager.TotalRounds)
        {
            StartCoroutine(EndGame());
        }
        else
        {
            turnManager.AdvanceTurn();

            if(turnManager.CurrentPlayer == turnManager.firstPlayer)
            {
                StartCoroutine(AITurn());
            }
            else
            {
                endTurnUIManager.SetEndTurnButtonInteractable(true);
            }
        }
    }

    public IEnumerator AITurn()
    {
        yield return StartCoroutine(aiManager.AITurn());

        AdvanceTurn();
    }

    private IEnumerator EndGame()
    {
        Constituent.Party winner;

        int finalRedScore = cityGenerator.Districts.Count((c) => c.CurrentMajority == Constituent.Party.Red);
        int finalblueScore = cityGenerator.Districts.Count((c) => c.CurrentMajority == Constituent.Party.Blue);
        if (finalRedScore > finalblueScore)
        {
            winner = Constituent.Party.Red;
        }
        else if (finalblueScore > finalRedScore)
        {
            winner = Constituent.Party.Blue;
        }
        else
        {
            winner = Constituent.Party.None;
        }

        if (winner != Constituent.Party.None)
        {
            //convert all constituents to the winning party
            var nonWinningConstituents = cityGenerator.Constituents.Where((c) => { return c.party != Constituent.Party.None && c.party != winner; });
            while (nonWinningConstituents.Count() > 0)
            {
                var constituent = Utils.ChooseRandom(nonWinningConstituents.ToList());

                constituent.party = winner;
                constituent.District.UpdateMemberData();

                yield return null;
            }
        }

        var endScreen = (GameObject)Instantiate(endScreenPrefab);
        endScreen.transform.SetParent(GetComponentInParent<Canvas>().transform, false);

        //determine the background color for the victory screen
        var backgroundColor = GetBackgroundColorForPlayer(winner);
        if (backgroundColor == Color.black)
            backgroundColor = endScreen.GetComponent<Image>().color;
        else
            backgroundColor = Color.Lerp(backgroundColor, Color.white, 0.75f);

        backgroundColor.a = 0.0f;

        endScreen.GetComponent<Image>().color = backgroundColor;

        //fade in the victory screen
        LeanTween.alpha(endScreen.GetComponent<RectTransform>(), 1.0f, 1.0f);

        endScreen.GetComponent<EndGameUIManager>().winner = winner;

        yield return null;
    }

    private Color GetBackgroundColorForPlayer(TurnManager.Player player)
    {
        if (player == TurnManager.Player.Red)
            return redsTurnBackgroundColor;
        else
            return bluesTurnBackgroundColor;
    }

    private Color GetBackgroundColorForPlayer(Constituent.Party player)
    {
        if (player == Constituent.Party.Red)
            return redsTurnBackgroundColor;
        else if (player == Constituent.Party.Blue)
            return bluesTurnBackgroundColor;
        else
            return Color.black;
    }
}
