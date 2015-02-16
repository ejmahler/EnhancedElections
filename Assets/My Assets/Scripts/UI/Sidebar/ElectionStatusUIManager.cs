using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class ElectionStatusUIManager : MonoBehaviour {

    private CityGenerator cityGenerator;

    [SerializeField] private Text redDistrictTextbox;
    [SerializeField] private Text blueDistrictTextbox;

	// Use this for initialization
	void Start ()
    {
        cityGenerator = GameObject.FindGameObjectWithTag("GameController").GetComponent<CityGenerator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        redDistrictTextbox.text = cityGenerator.Districts.Count((c) => { return c.CurrentMajority == Constituent.Party.Red; }).ToString();
        blueDistrictTextbox.text = cityGenerator.Districts.Count((c) => { return c.CurrentMajority == Constituent.Party.Blue; }).ToString();
	}
}
