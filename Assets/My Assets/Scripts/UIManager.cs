﻿using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(GameStateManager))]
[RequireComponent(typeof(CityGenerator))]
public class UIManager : MonoBehaviour {

    [SerializeField]
    private Text currentDistrictTextbox;

    [SerializeField]
    private Text constituentCountTextbox;

    [SerializeField] private Text redCountTextbox;
    [SerializeField] private Text blueCountTextbox;
    [SerializeField] private Text otherCountTextbox;

    [SerializeField] private Text redDistrictTextbox;
    [SerializeField] private Text blueDistrictTextbox;

    private GameStateManager gameStateManager;
    private CityGenerator cityGenerator;

	// Use this for initialization
	void Start () {
        gameStateManager = GetComponent<GameStateManager>();
        cityGenerator = GetComponent<CityGenerator>();
	}
	
	// Update is called once per frame
	void Update () {
        currentDistrictTextbox.text = DisplayNumber(gameStateManager.CurrentlySelectedDistrict.name) + " District";
        constituentCountTextbox.text = gameStateManager.CurrentlySelectedDistrict.VotingConstituents.Count().ToString();

        redCountTextbox.text = gameStateManager.CurrentlySelectedDistrict.Constituents.Where((c) => { return c.party == Constituent.Party.Red; }).Count().ToString();
        blueCountTextbox.text = gameStateManager.CurrentlySelectedDistrict.Constituents.Where((c) => { return c.party == Constituent.Party.Blue; }).Count().ToString();
        otherCountTextbox.text = gameStateManager.CurrentlySelectedDistrict.Constituents.Where((c) => { return c.party == Constituent.Party.Yellow; }).Count().ToString();

        redDistrictTextbox.text = cityGenerator.Districts.Where((c) => { return c.CurrentMajority == Constituent.Party.Red; }).Count().ToString();
        blueDistrictTextbox.text = cityGenerator.Districts.Where((c) => { return c.CurrentMajority == Constituent.Party.Blue; }).Count().ToString();
	}

    private string DisplayNumber(string number)
    {
        if (number.EndsWith("1") && !number.EndsWith("11"))
        {
            return number + "st";
        }
        else if (number.EndsWith("2") && !number.EndsWith("12"))
        {
            return number + "nd";
        }
        else if (number.EndsWith("3") && !number.EndsWith("13"))
        {
            return number + "rd";
        }
        else
        {
            return number + "th";
        }
    }
}