using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class LANModeManager : MonoBehaviour
{
    [SerializeField]
    private Color blueBackgroundColor;
    [SerializeField]
    private Color redBackgroundColor;

    [SerializeField]
    private List<Image> backgroundPanels;

    [SerializeField]
    private GameObject endScreenPrefab;

    private CurrentTurnUIManager currentTurnUIManager;
    private EndTurnUIManager endTurnUIManager;

    private MoveManager moveManager;
    private TurnManager turnManager;
    private CityGenerator cityGenerator;

    private MatchSettings settings;
    private NetworkClient client;

    // Use this for initialization
    void Start()
    {
        moveManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();
        currentTurnUIManager = GetComponentInChildren<CurrentTurnUIManager>();
        endTurnUIManager = GetComponentInChildren<EndTurnUIManager>();

        MatchConfig config = GameObject.FindGameObjectWithTag("MatchConfig").GetComponent<MatchConfig>();
        settings = config.Settings;
        client = config.Client;

        //color the background based on the human player
        var currentBackgroundColor = GetBackgroundColorForPlayer(settings.ThisPlayer);
        foreach (var panel in backgroundPanels)
        {
            panel.color = currentBackgroundColor;
        }

        endTurnUIManager.SetEndTurnButtonInteractable(turnManager.CurrentPlayer == turnManager.LocalHumanPlayer);

        //listen for stuff from the client
        client.RegisterHandler(GameServer.OPPONENT_ENDED_TURN, (msg) =>
        {
            AdvanceTurn();
        });
        client.RegisterHandler(GameServer.OPPONENT_MOVED, (msg) =>
        {
            MoveMessage moveData = msg.ReadMessage<MoveMessage>();

            Constituent movedC = cityGenerator.ConstituentsByPosition[moveData.location];
            District newDistrict = cityGenerator.DistrictsByName[moveData.newDistrictName];

            //temporarily stop listening to the move manager's "move made" message, otherwise we'll have an infinite loop
            moveManager.MoveMade -= MoveMade;

            //make this move
            moveManager.TryMove(movedC, newDistrict, moveData.undo);

            //resubscribe to move made messages
            moveManager.MoveMade += MoveMade;
        });

        //listen for changes made by the move manager
        moveManager.MoveMade += MoveMade;
    }

    public void AdvanceTurnButton()
    {
        //notify the server that the turn has ended
        client.Send(GameServer.PERFORM_END_TURN, new EndTurnMessage());

        //advance the turn on our end
        AdvanceTurn();
    }

    private void AdvanceTurn()
    {
        AudioManager.instance.PlayGavel();

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

            endTurnUIManager.SetEndTurnButtonInteractable(turnManager.CurrentPlayer == turnManager.LocalHumanPlayer);
        }
    }

    private void MoveMade(Constituent c, District d, bool undo)
    {
        client.Send(GameServer.PERFORM_MOVE, new MoveMessage(c.Position, d.name, undo));
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
            return redBackgroundColor;
        else
            return blueBackgroundColor;
    }

    private Color GetBackgroundColorForPlayer(Constituent.Party player)
    {
        if (player == Constituent.Party.Red)
            return redBackgroundColor;
        else if (player == Constituent.Party.Blue)
            return blueBackgroundColor;
        else
            return Color.black;
    }
}
