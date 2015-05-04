using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CompetitionModeUIManager : MonoBehaviour
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

    private TurnManager turnManager;
    private CityGenerator cityGenerator;

    // Use this for initialization
    void Start()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();
        currentTurnUIManager = GetComponentInChildren<CurrentTurnUIManager>();
        endTurnUIManager = GetComponentInChildren<EndTurnUIManager>();

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        //color the text based on whose turn it is
        var currentBackgroundColor = GetBackgroundColorForPlayer(turnManager.CurrentPlayer);
        foreach (var panel in backgroundPanels)
        {
            panel.color = currentBackgroundColor;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AdvanceTurn()
    {
        audioManager.PlayGavel();

        //prevent the player from triggering this stuff again or undoing any of their actions
        endTurnUIManager.SetEndTurnButtonInteractable(false);
        turnManager.BeginTurnTransition();

        if (turnManager.NextRound > turnManager.TotalRounds)
        {
            StartCoroutine(EndGame());
        }
        else
        {
            float transitionDuration = 1.0f;

            //alert child ui managers to update their information
            currentTurnUIManager.UpdateTurnDisplay(transitionDuration);

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
    }

    private void TurnTransitionsFinished()
    {
        endTurnUIManager.SetEndTurnButtonInteractable(true);
        turnManager.AdvanceTurn();
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
