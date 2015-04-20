using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(MoveManager))]
public class InputManager : MonoBehaviour
{

    private District currentDistrict = null;

    private Camera mainCamera;
    private MoveManager moveManager;

    public bool SelectionEnabled { get; set; }

    // Use this for initialization
    void Awake()
    {
        moveManager = GetComponent<MoveManager>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        SelectionEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            var constituent = PickConstituent();
            if (constituent != null)
            {
                moveManager.ConstituentDragged(constituent);
            }
        }
        else if (SelectionEnabled)
        {
            var constituent = PickConstituent();
            if (constituent != null)
            {
                moveManager.SelectConstituent(constituent);
                currentDistrict = constituent.District;
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
