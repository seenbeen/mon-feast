using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingCandyScript : MonoBehaviour {
    public Vector2 fallVelocity = new Vector2();
    public float degreesPerSecond = 360.0f;

    private Rigidbody2D rb;
    private bool currently_frozen = false;

    [HideInInspector]
    public CandyScript.Colour colour = CandyScript.Colour.BLUE;
    [HideInInspector]
    public bool isDead;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = fallVelocity;
        rb.angularVelocity = degreesPerSecond;
        GetComponent<Animator>().SetInteger("Colour", (int)colour);
    }

    public void Freeze()
    {
        if (!currently_frozen)
        {
            rb.velocity = new Vector2();
            rb.angularVelocity = 0;
            currently_frozen = true;
        }
        else
        {
            Debug.LogWarning("Freezing frozen falling candy.");
        }
    }

    public void Unfreeze()
    {
        if (currently_frozen)
        {
            rb.velocity = fallVelocity;
            rb.angularVelocity = degreesPerSecond;
            currently_frozen = false;
        }
        else
        {
            Debug.LogWarning("Unfreezing unfrozen falling candy.");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.tag)
        {
            case "Candy-Block":
                {
                    isDead = true;
                    break;
                }
            case "Boundary-Floor":
                {
                    isDead = true;
                    break;
                }
        }
    }
}
