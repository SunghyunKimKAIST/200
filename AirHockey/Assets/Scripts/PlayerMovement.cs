using UnityEngine;
using UnityEngine.Networking;

public class PlayerMovement : NetworkBehaviour
{
    bool wasJustClicked = true;
    bool canMove;

    Rigidbody2D rb;

    public Transform BoundaryHolder;

    Boundary playerBoundary;

    Collider2D playerCollider;

    [SyncVar(hook = "ChangePos")]
    Vector2 pos;
    void ChangePos(Vector2 pos)
    {
        rb.MovePosition(pos);
    }

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        playerBoundary = new Boundary(BoundaryHolder.GetChild(0).position.y,
                                      BoundaryHolder.GetChild(1).position.y,
                                      BoundaryHolder.GetChild(2).position.x,
                                      BoundaryHolder.GetChild(3).position.x);
    }

    [Command]
    void CmdSetPos(float x, float y)
    {
        pos = new Vector2(x, y);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (wasJustClicked)
            {
                wasJustClicked = false;

                if (playerCollider.OverlapPoint(mousePos))
                {
                    canMove = true;
                }
                else
                {
                    canMove = false;
                }
            }

            if (canMove)
            {
                CmdSetPos(Mathf.Clamp(mousePos.x, playerBoundary.Left, playerBoundary.Right),
                          Mathf.Clamp(mousePos.y, playerBoundary.Down, playerBoundary.Up));
            }
        }
        else
        {
            wasJustClicked = true;
        }
    }
}