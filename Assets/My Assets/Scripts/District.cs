using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class District : MonoBehaviour
{
    public Color PartyColor { get; private set; }
    public Color GlazeColor
    {
        get
        {
            return Color.Lerp(PartyColor, Color.white, maxGlazePercent);
        }
    }
    private float maxGlazePercent = 0.5f;

    public Color CurrentColor
    {
        get
        {
            return Color.Lerp(PartyColor, GlazeColor, selectedGlazePercent);
        }
    }
    private float selectedGlazePercent = 0.0f;

    public Color CurrentLockedColor
    {
        get
        {
            return Color.Lerp(CurrentColor, Color.gray, 0.5f);
        }
    }

    //set of constituents that would split this vertex in two if removed
    public HashSet<Constituent> ArticulationPoints { get; private set; }

    //set of consistituents that are adjacent, but not members of, this district
    public HashSet<Constituent> NeighborConstituents { get; private set; }

    public int MemberCount { get; private set; }
    public int VotingMemberCount { get; private set; }

    public Constituent.Party CurrentMajority { get; private set; }
    public float CurrentMajorityPercent { get; private set; }
    public int VotesRed { get; private set; }
    public int VotesBlue { get; private set; }
    public int VotesYellow { get; private set; }

    public Material ValidBorderMaterial { get; private set; }
    public Material InvalidBorderMaterial { get; private set; }

    private Material _lockedBackgroundMaterial;
    public Material LockedBackgroundMaterial
    {
        get
        {
            return _lockedBackgroundMaterial;
        }
    }

    private Material _currentBackgroundMaterial;
    public Material BackgroundMaterial
    {
        get { return _currentBackgroundMaterial; }
        private set
        {
            if (value != _currentBackgroundMaterial)
            {
                _currentBackgroundMaterial = value;
                foreach (Constituent c in ConstituentsQuery)
                {
                    c.UpdateBackground();
                }
            }
        }
    }

   

    public Material NormalBackgroundMaterial { get; private set; }
    public Material MinimumBackgroundMaterial { get; private set; }

    [SerializeField]
    private Color selectedBorderColor;
    [SerializeField]
    private Color normalBorderColor;
    [SerializeField]
    private Color invalidBorderColor;

    [SerializeField]
    private Color redBackgroundColor;
    [SerializeField]
    private Color blueBackgroundColor;
    [SerializeField]
    private Color evenBackgroundColor;

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

                //update the "glaze percentage" so that the entire district's background is lighter in color
                System.Action<float> glazeUpdate = (percent) =>
                {
                    selectedGlazePercent = percent;

                    var currentColor = CurrentColor;

                    NormalBackgroundMaterial.SetColor("_Color", currentColor);
                    MinimumBackgroundMaterial.SetColor("_StripeColor", currentColor);
                    LockedBackgroundMaterial.SetColor("_Color", CurrentLockedColor);
                };

                if (value)
                {
                    LeanTween.value(gameObject, 0.0f, 1.0f, 0.25f).setOnUpdate(glazeUpdate);
                }
                else
                {
                    LeanTween.value(gameObject, 1.0f, 0.0f, 0.25f).setOnUpdate(glazeUpdate);
                }
            }
        }
    }

    public IEnumerable<Constituent> ConstituentsQuery
    {
        get
        {
            return cityGenerator.Constituents.Where((obj) => { return obj.District == this; });
        }
    }

    public IEnumerable<Constituent> VotingConstituentsQuery
    {
        get
        {
            return cityGenerator.Constituents.Where((obj) => { return obj.District == this && obj.party != Constituent.Party.None; });
        }
    }

    void Awake()
    {
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();

        ValidBorderMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Border"));
        InvalidBorderMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Border"));


        NormalBackgroundMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Background"));
        MinimumBackgroundMaterial = (Material)Object.Instantiate(Resources.Load("Materials/Striped District Background"));
        _lockedBackgroundMaterial = (Material)Object.Instantiate(Resources.Load("Materials/District Background"));

        BackgroundMaterial = MinimumBackgroundMaterial;

        PartyColor = evenBackgroundColor;
        NormalBackgroundMaterial.SetColor("_Color", CurrentColor);

        MinimumBackgroundMaterial.SetColor("_StripeColor", CurrentColor);
        MinimumBackgroundMaterial.SetColor("_NonStripeColor", GlazeColor);

        LockedBackgroundMaterial.SetColor("_Color", CurrentLockedColor);

        ValidBorderMaterial.SetColor("_Color", normalBorderColor);
        InvalidBorderMaterial.SetColor("_Color", normalBorderColor);

        CurrentMajority = Constituent.Party.None;
    }

    public void UpdateMemberData()
    {
        HashSet<Constituent> members = new HashSet<Constituent>(ConstituentsQuery);
        MemberCount = members.Count();
        VotingMemberCount = VotingConstituentsQuery.Count();

        //create a function to get the neighbors of a constituent
        System.Func<Constituent, IEnumerable<Constituent>> neighborFunction = (c) =>
        {
            return c.Neighbors.Where((n) => { return members.Contains(n); });
        };

        //update the set of articulation points. we pass in a neighbor function for a depth first search
        ArticulationPoints = Utils.FindArticulationPoints<Constituent>(members.First(), neighborFunction);

        //update the set of neighbors - find all constituents that share a n edge with a member of this district
        NeighborConstituents = new HashSet<Constituent>(members.SelectMany((member) =>
        {
            return member.Neighbors.Where((neighbor) => { return neighbor != null && neighbor.District != this; });
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
            CurrentMajorityPercent = ((float)votesBlue) / VotingMemberCount;
        }
        else if (votesBlue < votesRed)
        {
            majority = Constituent.Party.Red;
            CurrentMajorityPercent = ((float)votesRed) / VotingMemberCount;
        }
        else
        {
            majority = Constituent.Party.None;
            CurrentMajorityPercent = 0.5f;
        }
        VotesBlue = votesBlue;
        VotesRed = votesRed;
        VotesYellow = votesYellow;

        //update the background color
        if (majority != CurrentMajority)
        {
            Color newColor = GetBackgroundColor(majority);
            Color oldColor = GetBackgroundColor(CurrentMajority);
            LeanTween.value(gameObject, oldColor, newColor, 0.25f).setOnUpdateColor((currentColor) =>
            {
                PartyColor = currentColor;
                var interpolatedColor = CurrentColor;

                NormalBackgroundMaterial.SetColor("_Color", interpolatedColor);
                MinimumBackgroundMaterial.SetColor("_StripeColor", interpolatedColor);
                MinimumBackgroundMaterial.SetColor("_NonStripeColor", GlazeColor);

                LockedBackgroundMaterial.SetColor("_Color", CurrentLockedColor);
            });

            CurrentMajority = majority;
        }

        if (VotingMemberCount <= cityGenerator.MinDistrictSize)
        {
            BackgroundMaterial = MinimumBackgroundMaterial;
        }
        else
        {
            BackgroundMaterial = NormalBackgroundMaterial;
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
