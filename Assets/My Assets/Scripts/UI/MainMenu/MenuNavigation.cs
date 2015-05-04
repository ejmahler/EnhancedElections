using UnityEngine;
using System.Collections;

public class MenuNavigation : MonoBehaviour
{
    private AudioManager audioManager;

    [SerializeField]
    private GameObject canvas;

    [SerializeField]
    private GameObject aboutCard;

    [SerializeField]
    private GameObject settingsCard;

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

    public void AboutClicked()
    {
        StartCoroutine(ShowCard(aboutCard));
    }

    public void SettingsClicked()
    {
        StartCoroutine(ShowCard(settingsCard));
    }

    private IEnumerator ShowCard(GameObject prefab)
    {
        //show the desired card
        var card = (GameObject)Instantiate(prefab);
        card.transform.SetParent(canvas.transform, false);

        //wait one frame before checking for input
        yield return null;

        //wait for the user to click through, then destroy the card
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        Destroy(card);
    }
}
