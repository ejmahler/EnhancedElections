using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuPopup : MonoBehaviour {

    [SerializeField]
    private string _NormalSceneName;

    [SerializeField]
    private string _LargeSceneName;

    [SerializeField]
    private Toggle _NormalToggle;

    [SerializeField]
    private Toggle _LargeToggle;

    public void CancelClicked()
    {

    }

    public void LaunchClicked()
    {
        if (_NormalToggle.isOn)
        {
            SceneManager.LoadScene(_NormalSceneName);
        }
        else if (_LargeToggle.isOn)
        {
            SceneManager.LoadScene(_LargeSceneName);
        }
    }
}
