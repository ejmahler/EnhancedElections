﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CurrentDistrictUIManager : MonoBehaviour
{

    [SerializeField]
    private Text currentDistrictTextbox;

    [SerializeField]
    private Text constituentCountTextbox;

    [SerializeField]
    private Text redCountTextbox;
    [SerializeField]
    private Text blueCountTextbox;

    private MoveManager turnManager;

    // Use this for initialization
    void Start()
    {
        turnManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentDistrictTextbox.text = DisplayNumber(turnManager.CurrentlySelectedDistrict.name) + " District";
        constituentCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotingMemberCount.ToString();

        redCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotesRed.ToString();
        blueCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotesBlue.ToString();
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
