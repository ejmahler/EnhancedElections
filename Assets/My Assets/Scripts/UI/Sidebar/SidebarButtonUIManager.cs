using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SidebarButtonUIManager : MonoBehaviour
{
    [SerializeField]
    private Button undoButton;

    [SerializeField]
    private Button undoAllButton;

    private MoveManager moveManager;

    // Use this for initialization
    void Start()
    {
        moveManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<MoveManager>();
    }

    // Update is called once per frame
    void Update()
    {

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

    public void MainMenuClicked()
    {
        Application.LoadLevel("MainMenu");
    }
}
