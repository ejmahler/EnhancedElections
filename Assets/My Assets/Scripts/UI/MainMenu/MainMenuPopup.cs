using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuPopup : MonoBehaviour
{
    [SerializeField]
    private string _TargetSceneName;

    [SerializeField]
    private Toggle _NormalToggle;

    [SerializeField]
    private Toggle _LargeToggle;

    public void CancelClicked()
    {

    }

    public virtual void LaunchClicked()
    {
        SceneManager.LoadScene(_TargetSceneName);
    }
}
