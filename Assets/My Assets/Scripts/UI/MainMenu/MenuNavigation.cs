using UnityEngine;
using System.Collections;

public class MenuNavigation : MonoBehaviour {

	public void CompetitionClicked()
    {
        Application.LoadLevel("CompetitionMode");
    }

    public void SandboxClicked()
    {
        Application.LoadLevel("SandboxMode");
    }
}
