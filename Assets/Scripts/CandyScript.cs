using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyScript : MonoBehaviour {
    // Structs and Enums
    public enum Colour { RED = 0, GREEN, BLUE, WHITE };
    public enum State { IDLE = 0, GLOW, EXPLODE };
    
    // Candy-Specific Information
    [HideInInspector]
    public CandyManager manager = null;
    [SerializeField]
    private GameObject ghostCandyPrefab = null;

    [HideInInspector]
    public Colour colour = Colour.BLUE;

    [HideInInspector]
    public bool isDead = false;

    private State state = State.IDLE;

    // Pause-Related Fields
    private Vector2 freeze_frame_vel;
    private bool currently_frozen = false;

    // Component refs
    private Rigidbody2D rb = null;
    private Animator an = null;
    
    // Use this for initialization
    void Start () {
        Debug.Assert(manager != null); // safety check... only manager should be creating Candies...
        GetRB();
        an = GetComponent<Animator>();
        an.Play(Animator.StringToHash("Red_Idle"));
        an.SetInteger("Colour", (int)colour);
    }

    public bool IsAnimComplete()
    {
        return an.GetBool("StartedAnimation") && an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
    }

    public void SetState(State state)
    {
        this.state = state;
        an.SetInteger("State", (int)state);
        an.SetBool("StartedAnimation", false);
    }

    public State GetState()
    {
        return this.state;
    }

    public void Freeze()
    {
        if (!currently_frozen)
        {
            freeze_frame_vel = rb.velocity;
            rb.velocity = new Vector2();
            currently_frozen = true;
        }
        else
        {
            Debug.LogWarning("Freezing frozen candy.");
        }
    }

    public void Unfreeze()
    {
        if (currently_frozen)
        {
            rb.velocity = freeze_frame_vel;
            currently_frozen = false;
        }
        else
        {
            Debug.LogWarning("Unfreezing unfrozen candy.");
        }
    }

    public Rigidbody2D GetRB()
    {
        if (rb == null)
        {
            rb = gameObject.GetComponent<Rigidbody2D>();
        }
        return rb;
    }
    
    public void SpawnGhost()
    {
        GameObject ghost = Instantiate(ghostCandyPrefab);
        ghost.transform.position = transform.position;
        ghost.GetComponent<GhostCandyScript>().colour = colour;
    }
}
