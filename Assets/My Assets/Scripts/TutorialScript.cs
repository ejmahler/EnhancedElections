using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TutorialScript : MonoBehaviour {

    private MoveManager moveManager;
    private TurnManager turnManager;
    private CityGenerator cityGenerator;
    private InputManager inputManager;

    [SerializeField]
    private Button endTurnButton;

    private List<Constituent> allowedMoves;

    [SerializeField]
    private GameObject canvas;

    [SerializeField]
    private Text instructionText;

    [SerializeField]
    private GameObject introPrefab;

    [SerializeField]
    private GameObject gameGoalsPrefab;

    [SerializeField]
    private GameObject uiBottomIntroPrefab;

    [SerializeField]
    private GameObject uiSideIntroPrefab;

    [SerializeField]
    private GameObject conclusionPrefab;


	// Use this for initialization
	void Awake ()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<TurnManager>();
        turnManager.firstPlayer = TurnManager.Player.Red;
        moveManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();
        inputManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<InputManager>();

        allowedMoves = new List<Constituent>();
	}

    void Start()
    {
        StartCoroutine(RunTutorial());
    }

    void Update()
    {
        moveManager.CurrentValidMoves.IntersectWith(allowedMoves);
        foreach (var member in moveManager.CurrentlySelectedDistrict.Constituents)
        {
            member.UpdateBorders();
        }
    }

    private IEnumerator RunTutorial()
    {
        var fourthDistrict = cityGenerator.Districts.First((district => district.name == "4"));
        var upperLeftDistrict = cityGenerator.Districts.First((district => district.name == "3"));

        var seventhDistrict = cityGenerator.Districts.First((district => district.name == "7"));
        var upperRightDistrict = cityGenerator.Districts.First((district => district.name == "8"));

        turnManager.firstPlayer = TurnManager.Player.Red;
        inputManager.SelectionEnabled = false;
        endTurnButton.interactable = false;

        //show the intro screen
        yield return StartCoroutine(ShowCard(introPrefab));

        //show the goal screen
        yield return StartCoroutine(ShowCard(gameGoalsPrefab));

        //show the UI bottom intro screen
        yield return StartCoroutine(ShowCard(uiBottomIntroPrefab));

        //show the UI Side intro screen
        yield return StartCoroutine(ShowCard(uiSideIntroPrefab));

        //instruct the player to select districts.
        inputManager.SelectionEnabled = true;
        instructionText.text = "Let's get started. We're going to begin as the Red player. Select districts by hovering your mouse over them. Try selecting 5 different districts!";

        //wait until the player has selected 5 districts
        HashSet<District> selectedDistricts = new HashSet<District>();
        while(selectedDistricts.Count < 5)
        {
            selectedDistricts.Add(moveManager.CurrentlySelectedDistrict);
            yield return null;
        }

        //instruct the player to select the upper left district
        instructionText.text = "Good. Notice how the border changes color as you select a district.\n\nNow, select the upper left district.";
        yield return StartCoroutine(SelectDistrict(upperLeftDistrict));

        //teach the player how to capture territory
        while (fourthDistrict.CurrentMajority != Constituent.Party.Red)
        {
            //instruct the player to select the upper left district
            if (moveManager.CurrentlySelectedDistrict != upperLeftDistrict)
            {
                instructionText.text = "Select the upper left district.";
                allowedMoves.Clear();
                yield return StartCoroutine(SelectDistrict(upperLeftDistrict));
            }

            //only allow the player to move into neighbor contituents that are blue
            allowedMoves = moveManager.CurrentlySelectedDistrict.NeighborConstituents.Where((c) =>
            {
                return c.party == Constituent.Party.Blue && c.district == fourthDistrict;
            }).ToList();

            if (!Input.GetButton("Fire1"))
            {
                instructionText.text = "The district to the right has a pretty even vote. We can swing it red's favor by moving two of its blue voters into the upper left district.\n\nStart by clicking on a constituent in the upper left district and holding the mouse button down.";
            }
            else
            {
                instructionText.text = "Now, drag the cursor over the constituents you want to add to this district.\n\nNotice how the border is green if you can move in that direction, and red if you can't.";
            }
            
            yield return null;
        }
        allowedMoves.Clear();
        moveManager.UndoStack.Clear();

        //instruct the player to select the upper right district
        instructionText.text = "Great! The neighboring district lost two blue voters. The majority among the remaining voters is red, so the district is ours!\n\nNow, select the upper right district.";
        yield return StartCoroutine(SelectDistrict(upperRightDistrict));

        //teach the player how to secure existing territory
        while (seventhDistrict.VotesBlue > 4)
        {
            //instruct the player to select the upper right district
            if (moveManager.CurrentlySelectedDistrict != upperRightDistrict)
            {
                instructionText.text = "Select the upper right district.";
                allowedMoves.Clear();
                yield return StartCoroutine(SelectDistrict(upperRightDistrict));
            }

            //only allow the player to move into neighbor contituents that are blue
            allowedMoves = moveManager.CurrentlySelectedDistrict.NeighborConstituents.Where((c) =>
            {
                return c.party == Constituent.Party.Blue && c.district == seventhDistrict;
            }).ToList();

            instructionText.text = "We control the district below this, but the vote is too close for comfort. By removing blue voters, we can ensure that Blue can't take it from us.\n\nMove as many blue voters as possible out of the bottom right district.";

            yield return null;
        }
        allowedMoves.Clear();
        moveManager.UndoStack.Clear();

        //instuct the player to end the turn
        instructionText.text = "Great! Now it'll be more difficult for Blue to take that district from us.\n\nWe've run out of moves for this turn. Press the \"End turn\" button.";
        endTurnButton.interactable = true;
        while (turnManager.CurrentPlayer == TurnManager.Player.Red)
        {
            yield return null;
        }
        endTurnButton.interactable = false;

        //inform the player about locked etc's
        instructionText.text = "See how the constituents we moved last turn are gray now? That means they're Locked. Neither player is allowed to move locked constituents into other districts.\n\nPress any key to continue.";
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //wait one frame before the next item
        yield return null;

        //inform the player about the score
        instructionText.text = "The score at the bottom has changed! Whenever a player presses \"End Turn\", both players get one point for every district they control.\n\nPress any key to continue.";
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        instructionText.text = "";

        //show the conclusion screen
        yield return StartCoroutine(ShowCard(conclusionPrefab));

        instructionText.text = "Feel free to keep playing. Click the \"Main Menu\" button when you're ready to play an actual game.";

        //allow all moves, so that the player can keep messing around
        endTurnButton.interactable = true;
        allowedMoves = cityGenerator.Constituents;
    }

    private IEnumerator ShowCard(GameObject prefab)
    {
        //show the desired card
        var card = (GameObject)Instantiate(prefab);
        card.transform.SetParent(canvas.transform, false);

        //wait one frame before checking for input
        yield return null;

        //wait for the user to click through, then destroy the card
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        Destroy(card);
    }

    public IEnumerator SelectDistrict(District d)
    {
        while (moveManager.CurrentlySelectedDistrict != d)
        {
            yield return null;
        }
    }
}
