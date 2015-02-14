using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(TurnManager))]
public class InputManager : MonoBehaviour {

    private Constituent currentConstituent = null;

    private Camera mainCamera;
    private TurnManager turnManager;

	// Use this for initialization
	void Start () {
        turnManager = GetComponent<TurnManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
    void Update()
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            turnManager.Undo();
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

    public void ReloadClicked()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }
}
