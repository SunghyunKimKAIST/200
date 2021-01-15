using System.Collections;
using UnityEngine;

public class PuckScript : MonoBehaviour
{
    public ScoreScript ScoreScriptInstance;
    public static bool WasGoal { get; private set; }

    public AudioManager audioManager;

    private Rigidbody2D rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        WasGoal = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!WasGoal)
        {
            if (other.tag == "RedGoal")
            {
                ScoreScriptInstance.Increment(ScoreScript.Score.BlueScore);
                WasGoal = true;
                audioManager.PlayGoal();
                StartCoroutine(ResetPuck());
            }
            else if (other.tag == "BlueGoal")
            {
                ScoreScriptInstance.Increment(ScoreScript.Score.RedScore);
                WasGoal = true;
                audioManager.PlayGoal();
                StartCoroutine(ResetPuck());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        audioManager.PlayPuckCollision();
    }

    private IEnumerator ResetPuck()
    {
        yield return new WaitForSecondsRealtime(1);
        WasGoal = false;
        rb.velocity = rb.position = new Vector2(0, 0);
    }
}