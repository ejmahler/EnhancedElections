using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class District : MonoBehaviour {

    public string DistrictName { get; set; }
    public int Count { get; private set; }

    public Constituent.Party CurrentMajority  { get; private set; }
	public int VotesRed { get; private set; }
	public int VotesBlue { get; private set; }
	public int VotesYellow { get; private set; }

    public Material BorderMaterial { get; private set; }
    public Material BackgroundMaterial { get; private set; }

    [SerializeField] private Color selectedBorderColor;
    [SerializeField] private Color normalBorderColor;

    [SerializeField] private Color redBackgroundColor;
    [SerializeField] private Color blueBackgroundColor;
    [SerializeField] private Color evenBackgroundColor;

    private GameObject gameController;

    private bool _currentlySelected;
    public bool CurrentlySelected
    {
        get
        {
            return _currentlySelected;
        }
        set
        {
            if (value != _currentlySelected)
            {
				_currentlySelected = value;

				//update the border color
                Color newColor = GetBorderColor(value);
                Color oldColor = GetBorderColor(_currentlySelected);
                LeanTween.value(gameObject, oldColor, newColor, 0.25f).setOnUpdateColor((currentColor) =>
                {
                    BorderMaterial.SetColor("_Color", currentColor);
                });

				//tell our constituents to update their borders
				foreach(var c in Constituents)
				{
					c.UpdateBorders();
				}
            }
        }
    }

    public IEnumerable<Constituent> Constituents
    {
        get
        {
            return gameController.GetComponentsInChildren<Constituent>().Where((obj) => { return obj.district == this; });
        }
    }

    public IEnumerable<Constituent> VotingConstituents
    {
        get
        {
            return gameController.GetComponentsInChildren<Constituent>().Where((obj) => { return obj.district == this && obj.party != Constituent.Party.None; });
        }
    }

    void Awake()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController");

        //be sure to copy the materials rather than just using them directly - that way we can mess with the colors without screwing up other districts 
        BackgroundMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Background"));
        BorderMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Border"));

        BackgroundMaterial.SetColor("_Color", evenBackgroundColor);
        BorderMaterial.SetColor("_Color", normalBorderColor);

        CurrentMajority = Constituent.Party.None;
    }

    public void UpdateMajority()
    {
        //get all the current consistuents
		var constituents = VotingConstituents;

        Count = constituents.Count();

        //filter the consistuents by the ones in this district and then group them by party
        var partyGrouping = constituents.GroupBy((obj) => { return obj.party; });

        Dictionary<Constituent.Party, int> voteCounts = new Dictionary<Constituent.Party, int>();
        foreach (var group in partyGrouping)
        {
            voteCounts.Add(group.Key, group.Count());
        }

        Constituent.Party majority;
		int votesBlue = 0, votesRed = 0, votesYellow = 0;
		voteCounts.TryGetValue(Constituent.Party.Blue, out votesBlue);
		voteCounts.TryGetValue(Constituent.Party.Red, out votesRed);
		voteCounts.TryGetValue(Constituent.Party.Yellow, out votesYellow);
		if (votesBlue > votesRed)
        {
            majority = Constituent.Party.Blue;
        }
		else if (votesBlue < votesRed)
        {
            majority = Constituent.Party.Red;
        }
        else
        {
            majority = Constituent.Party.None;
        }
		VotesBlue = votesBlue;
		VotesRed = votesRed;
		VotesYellow = votesYellow;

        //update the background color
        if (majority != CurrentMajority)
        {
            Color newColor = GetBackgroundColor(majority);
            Color oldColor = GetBackgroundColor(CurrentMajority);
            LeanTween.value(gameObject, oldColor, newColor, 0.25f).setOnUpdateColor((currentColor) => {
                BackgroundMaterial.SetColor("_Color", currentColor);
            });

            CurrentMajority = majority;
        }
    }

    private Color GetBackgroundColor(Constituent.Party party)
    {
        if (party == Constituent.Party.Blue)
        {
            return blueBackgroundColor;
        }
        else if (party == Constituent.Party.Red)
        {
            return redBackgroundColor;
        }
        else
        {
            return evenBackgroundColor;
        }
    }

    private Color GetBorderColor(bool selected)
    {
        if (selected)
        {
            return selectedBorderColor;
        }
        else
        {
            return normalBorderColor;
        }
    }
}
