using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameStateManager))]
public class MouseStateManager : MonoBehaviour {

    private Constituent currentConstituent = null;

    private Camera mainCamera;
    private GameStateManager gameStateManager;

	// Use this for initialization
	void Start () {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        gameStateManager = GetComponent<GameStateManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1"))
        {
            var constituent = PickConstituent();
            if (constituent != null)
            {
                gameStateManager.ConstituentClicked(constituent);
                currentConstituent = constituent;
            }
        }
        else if (Input.GetButton("Fire1"))
        {
            var constituent = PickConstituent();
            if (constituent != null && constituent != currentConstituent)
            {
                gameStateManager.ConstituentDragged(constituent);
                currentConstituent = constituent;
            }
        }
        else
        {
            currentConstituent = null;
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
}
