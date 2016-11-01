using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndGameUIManager : MonoBehaviour
{

    [SerializeField]
    private Text winnerHeader;

    [SerializeField]
    private Text winnerText;

    public Constituent.Party winner
    {
        set
        {
            if (value == Constituent.Party.None)
            {
                winnerHeader.text = "Tie!";
                winnerText.text = "Despite your best efforts, neither of you were able to gain unfair control of the city.";
            }
            else if (value == Constituent.Party.Blue)
            {
                winnerHeader.text = "Blue Wins!";
                winnerText.text = "Blue has successfully ensured that Red voters will be disenfranchised for decades to come.\nCongratulations!";
            }
            else if (value == Constituent.Party.Red)
            {
                winnerHeader.text = "Red Wins!";
                winnerText.text = "Red has successfully ensured that Blue voters will be disenfranchised for decades to come.\nCongratulations!";
            }
        }
    }

    public void MainMenuClicked()
    {
		SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgainClicked()
    {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
