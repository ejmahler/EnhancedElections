using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuNavigation : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _AboutPopupPrefab;

    [SerializeField]
    private CanvasGroup _SettingsPopupPrefab;

    [SerializeField]
    private CanvasGroup _Local1v1PopupPrefab;

    [SerializeField]
    private CanvasGroup _LAN1v1PopupPrefab;

    [SerializeField]
    private CanvasGroup _AIPopuPrefabp;

    [SerializeField]
    private CanvasGroup _SandboxPopupPrefab;

    private CanvasGroup activePopupInstance;

    public void TutorialClicked()
    {
        AudioManager.instance.PlayGavel();
		SceneManager.LoadScene("TutorialMode");
    }

    public void Local1v1Clicked()
    {
        AudioManager.instance.PlayGavel();
        ShowPopup(_Local1v1PopupPrefab);
    }

    public void LAN1v1Clicked()
    {
        AudioManager.instance.PlayGavel();
        ShowPopup(_LAN1v1PopupPrefab);
    }

    public void AIClicked()
    {
        AudioManager.instance.PlayGavel();
        ShowPopup(_AIPopuPrefabp);
    }

    public void SandboxClicked()
    {
        AudioManager.instance.PlayGavel();
        ShowPopup(_SandboxPopupPrefab);
    }

    public void AboutClicked()
    {
        AudioManager.instance.PlayGavel();
        ShowPopup(_AboutPopupPrefab);
    }

    public void SettingsClicked()
    {
        AudioManager.instance.PlayGavel();
        ShowPopup(_SettingsPopupPrefab);
    }

    private void ShowPopup(CanvasGroup prefab)
    {
        if(activePopupInstance != null)
        {
            var previousPopup = activePopupInstance;
            LeanTween.alphaCanvas(previousPopup, 0f, .25f).setOnComplete(() => Destroy(previousPopup.gameObject));
        }

        activePopupInstance = Instantiate<CanvasGroup>(prefab);
        activePopupInstance.alpha = 0f;
        LeanTween.alphaCanvas(activePopupInstance, 1f, .25f);
    }
}
