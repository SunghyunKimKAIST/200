using UnityEngine;
using UnityEngine.UI;
 
public class ScoreScript : MonoBehaviour
{
    public enum Score
    {
        AIScore, PlayerScore
    }

    public Text AIScoreText, PlayerScoreText;
    private int aiScore, playerScore;

    public void Increment(Score whichScore)
    {
        if (whichScore == Score.AIScore)
            AIScoreText.text = (++aiScore).ToString();
        else
            PlayerScoreText.text = (++playerScore).ToString();
    }
}