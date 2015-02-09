using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(CityGenerator))]
public class GameStateManager : MonoBehaviour {

    public District CurrentlySelectedDistrict { get; private set; }

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

                oldDistrict.UpdateMajority();
                CurrentlySelectedDistrict.UpdateMajority();
            }
        }
    }

    private void SelectDistrict(District district)
    {
        CurrentlySelectedDistrict = district;

        foreach (District d in cityGenerator.Districts)
        {
            if (d == district)
            {
                d.CurrentlySelected = true;
            }
            else
            {
                d.CurrentlySelected = false;
            }
        }
    }
}
