using UnityEngine;
using System.Collections;

public class MenuNavigation : MonoBehaviour {

    public void TutorialClicked()
    {
        Application.LoadLevel("TutorialMode");
    }

    public void NormalCompetitionClicked()
    {
        Application.LoadLevel("CompetitionMode");
    }

    public void NormalSandboxClicked()
    {
        Application.LoadLevel("SandboxMode");
    }

    public void LargeCompetitionClicked()
    {
        Application.LoadLevel("CompetitionModeLarge");
    }

    public void LargeSandboxClicked()
    {
        Application.LoadLevel("SandboxModeLarge");
    }
}
