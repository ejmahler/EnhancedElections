using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(CityGenerator))]
public class GameStateManager : MonoBehaviour {

    public District CurrentlySelectedDistrict { get; private set; }
	public HashSet<Constituent> CurrentValidMoves { get; private set; }

    private CityGenerator cityGenerator;

	// Use this for initialization
	void Start () {
        cityGenerator = GetComponent<CityGenerator>();
        SelectDistrict(cityGenerator.Districts[0]);
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void ReloadLevel()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void ConstituentClicked(Constituent c)
    {
        SelectDistrict(c.district);
    }

    public void ConstituentDragged(Constituent c)
    {
        if (CurrentlySelectedDistrict != null) {
            if (cityGenerator.IsValidMove(c, CurrentlySelectedDistrict))
            {
                var oldDistrict = c.district;
                c.district = CurrentlySelectedDistrict;

				oldDistrict.UpdateMemberData();
				CurrentlySelectedDistrict.UpdateMemberData();

				UpdateValidMoves ();

				//update the borders of this constituent and its 4 neghbors
				foreach (var member in CurrentlySelectedDistrict.Constituents)
				{
					member.UpdateBorders ();
				}
            }
        }
    }

	private void SelectDistrict(District newDistrict)
	{
		if (CurrentlySelectedDistrict != newDistrict)
		{
			var oldDistrict = CurrentlySelectedDistrict;
			CurrentlySelectedDistrict = newDistrict;

			foreach (District d in cityGenerator.Districts)
			{
				if (d == newDistrict) {
					d.CurrentlySelected = true;
				}
				else
				{
					d.CurrentlySelected = false;
				}
			}
			UpdateValidMoves ();

			foreach (var member in newDistrict.Constituents)
			{
				member.UpdateBorders ();
			}

			if(oldDistrict != null)
			{
				foreach (var member in oldDistrict.Constituents)
				{
					member.UpdateBorders ();
				}
			}
		}
    }

	private void UpdateValidMoves()
	{
		CurrentValidMoves = new HashSet<Constituent> ();
		foreach (Constituent districtNeighbor in CurrentlySelectedDistrict.NeighborConstituents)
		{
			if(cityGenerator.IsValidMove(districtNeighbor, CurrentlySelectedDistrict))
			{
				CurrentValidMoves.Add (districtNeighbor);
			}
		}
	}
}
