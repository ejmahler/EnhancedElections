using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(TurnManager))]
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


    private Constituent currentConstituent = null;

    private Camera mainCamera;
    private TurnManager turnManager;
    private CityGenerator cityGenerator;

	// Use this for initialization
	void Start () {
        turnManager = GetComponent<TurnManager>();
        cityGenerator = GetComponent<CityGenerator>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        currentDistrictTextbox.text = DisplayNumber(turnManager.CurrentlySelectedDistrict.name) + " District";
        constituentCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotingMemberCount.ToString();

        redCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotesRed.ToString();
        blueCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotesBlue.ToString();
        otherCountTextbox.text = turnManager.CurrentlySelectedDistrict.VotesYellow.ToString();

        redDistrictTextbox.text = cityGenerator.Districts.Count((c) => { return c.CurrentMajority == Constituent.Party.Red; }).ToString();
        blueDistrictTextbox.text = cityGenerator.Districts.Count((c) => { return c.CurrentMajority == Constituent.Party.Blue; }).ToString();


        ProcessInput();
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

    private Constituent PickConstituent()
    {
        var hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), new Vector2(), 0f);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Constituent>();
        }
        else
        {
            return null;
        }
    }

    private void ProcessInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            var constituent = PickConstituent();
            if (constituent != null)
            {
                turnManager.ConstituentClicked(constituent);
                currentConstituent = constituent;
            }
        }
        else if (Input.GetButton("Fire1"))
        {
            var constituent = PickConstituent();
            if (constituent != null && constituent != currentConstituent)
            {
                turnManager.ConstituentDragged(constituent);
                currentConstituent = constituent;
            }
        }
        else
        {
            currentConstituent = null;
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            turnManager.Undo();
        }
    }

    public void ReloadClicked()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }
}
