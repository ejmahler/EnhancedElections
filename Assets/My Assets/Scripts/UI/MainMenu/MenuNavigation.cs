using UnityEngine;
using System.Collections;

public class MenuNavigation : MonoBehaviour
{
    private AudioManager audioManager;

    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    public void TutorialClicked()
    {
        audioManager.PlayGavel();
        Application.LoadLevel("TutorialMode");
    }

    public void NormalCompetitionClicked()
    {
        audioManager.PlayGavel();
        Application.LoadLevel("CompetitionMode");
    }

    public void NormalSandboxClicked()
    {
        audioManager.PlayGavel();
        Application.LoadLevel("SandboxMode");
    }

    public void LargeCompetitionClicked()
    {
        audioManager.PlayGavel();
        Application.LoadLevel("CompetitionModeLarge");
    }

    public void LargeSandboxClicked()
    {
        audioManager.PlayGavel();
        Application.LoadLevel("SandboxModeLarge");
    }
}
