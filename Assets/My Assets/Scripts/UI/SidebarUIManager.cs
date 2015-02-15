using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SidebarUIManager : MonoBehaviour {

    [SerializeField]
    private Button undoButton;

    [SerializeField]
    private Button undoAllButton;

    [SerializeField]
    private Button reloadButton;

    private MoveManager moveManager;

    public bool AllowReload = false;

	// Use this for initialization
	void Start () {
        moveManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();
	}
	
	// Update is called once per frame
	void Update () {

        //enable/disable the undo button
        if (moveManager.UndoStack.Count > 0)
        {
            undoButton.interactable = true;
        }
        else
        {
            undoButton.interactable = false;
        }

        //enable/disable the undo all button
        if (moveManager.UndoStack.Count > 0)
        {
            undoAllButton.interactable = true;
        }
        else
        {
            undoAllButton.interactable = false;
        }

        //hide/show the reload button
        reloadButton.gameObject.SetActive(AllowReload);
	}

    public void UndoClicked()
    {
        moveManager.Undo();
    }

    public void UndoAllClicked()
    {
        moveManager.UndoAll();
    }

    public void ReloadClicked()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }
}
