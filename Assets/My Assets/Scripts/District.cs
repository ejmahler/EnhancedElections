using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class District : MonoBehaviour {

    public int MemberCount { get; private set; }
    public int VotingMemberCount { get; private set; }

    public Constituent.Party CurrentMajority  { get; private set; }
	public int VotesRed { get; private set; }
	public int VotesBlue { get; private set; }
	public int VotesYellow { get; private set; }

	public Material ValidBorderMaterial { get; private set; }
	public Material InvalidBorderMaterial { get; private set; }
	public Material BackgroundMaterial { get; private set; }

	//set of constituents that would split this vertex in two if removed
	public HashSet<Constituent> ArticulationPoints { get; private set; }

	//set of consistituents that are adjacent, but not members of, this district
	public HashSet<Constituent> NeighborConstituents { get; private set; }

    [SerializeField] private Color selectedBorderColor;
	[SerializeField] private Color normalBorderColor;
	[SerializeField] private Color invalidBorderColor;
	
	[SerializeField] private Color redBackgroundColor;
    [SerializeField] private Color blueBackgroundColor;
    [SerializeField] private Color evenBackgroundColor;

    private CityGenerator cityGenerator;

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
                Color newValidColor = GetValidBorderColor(value);
                Color oldValidColor = GetValidBorderColor(_currentlySelected);
                LeanTween.value(gameObject, oldValidColor, newValidColor, 0.25f).setOnUpdateColor((currentColor) =>
                {
                    ValidBorderMaterial.SetColor("_Color", currentColor);
                });

				Color newInvalidColor = GetInvalidBorderColor(value);
				Color oldInvalidColor = GetInvalidBorderColor(_currentlySelected);
				LeanTween.value(gameObject, oldInvalidColor, newInvalidColor, 0.25f).setOnUpdateColor((currentColor) =>
				                                                                        {
					InvalidBorderMaterial.SetColor("_Color", currentColor);
				});
            }
        }
    }

    public IEnumerable<Constituent> Constituents
    {
        get
        {
            return cityGenerator.Constituents.Where((obj) => { return obj.district == this; });
        }
    }

    public IEnumerable<Constituent> VotingConstituents
    {
        get
        {
            return cityGenerator.Constituents.Where((obj) => { return obj.district == this && obj.party != Constituent.Party.None; });
        }
    }

    void Awake()
    {
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();

        //be sure to copy the materials rather than just using them directly - that way we can mess with the colors without screwing up other districts 
        BackgroundMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Background"));
		ValidBorderMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Border"));
		InvalidBorderMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Border"));
		
		BackgroundMaterial.SetColor("_Color", evenBackgroundColor);
		ValidBorderMaterial.SetColor("_Color", normalBorderColor);
		InvalidBorderMaterial.SetColor("_Color", normalBorderColor);
		
		CurrentMajority = Constituent.Party.None;
    }

    public void UpdateMemberData()
    {
		HashSet<Constituent> members = new HashSet<Constituent> (Constituents);
        MemberCount = members.Count();
        VotingMemberCount = VotingConstituents.Count();

		//update the set of articulation points
		ArticulationPoints = Utils.FindArticulationPoints<Constituent>(members.First(), (c) =>
		{
			return c.Neighbors.Where ((n) => { return members.Contains (n); });
		});

		//update the set of neighbors - find all constituents that share a n edge with a member of this district
		NeighborConstituents = new HashSet<Constituent>(members.SelectMany ((member) => { 
			return member.Neighbors.Where((neighbor) => { return neighbor != null && neighbor.district != this; });
		}));

        //filter the consistuents by the ones in this district and then group them by party
		var partyGrouping = members.GroupBy((obj) => { return obj.party; });

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

	private Color GetValidBorderColor(bool selected)
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

	private Color GetInvalidBorderColor(bool selected)
	{
		if (selected)
		{
			return invalidBorderColor;
		}
		else
		{
			return normalBorderColor;
		}
	}
}
