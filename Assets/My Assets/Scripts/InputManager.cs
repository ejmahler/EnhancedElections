using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(MoveManager))]
public class InputManager : MonoBehaviour {

    private District currentDistrict = null;

    private Camera mainCamera;
    private MoveManager moveManager;

	// Use this for initialization
	void Start () {
        moveManager = GetComponent<MoveManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            var constituent = PickConstituent();
            if (constituent != null && constituent.district != currentDistrict)
            {
                moveManager.ConstituentDragged(constituent);
            }
        }
        else
        {
            var constituent = PickConstituent();
            if (constituent != null)
            {
                moveManager.SelectDistrict(constituent.district);
                currentDistrict = constituent.district;
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            moveManager.Undo();
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
