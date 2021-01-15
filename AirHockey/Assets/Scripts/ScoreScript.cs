using UnityEngine;
using UnityEngine.UI;
 
public class ScoreScript : MonoBehaviour
{
    public enum Score
    {
        RedScore, BlueScore
    }

    public Text RedScoreText, BlueScoreText;
    private int redScore, blueScore;

    public void Increment(Score whichScore)
    {
        if (whichScore == Score.RedScore)
            RedScoreText.text = (++redScore).ToString();
        else
            BlueScoreText.text = (++blueScore).ToString();
    }
}