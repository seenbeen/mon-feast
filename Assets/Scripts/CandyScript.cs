using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyScript : MonoBehaviour {
    // Structs and Enums
    public enum Colour { RED=0, GREEN, BLUE, NONE };

    // Candy-Specific Information
    [HideInInspector]
    public CandyManager manager = null;

    [HideInInspector]
    public Colour colour = Colour.NONE;

    [HideInInspector]
    public bool isDead = false;

    // Pause-Related Fields
    private Vector2 freeze_frame_vel;
    private bool currently_frozen = false;

    // Component refs
    private Rigidbody2D rb = null;

    // Use this for initialization
    void Start () {
        Debug.Assert(manager != null); // safety check... only manager should be creating Candies...
        GetRB();
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

    public void Explosion()
    {
        List<CandyScript> r = manager.BFS(new List<CandyScript> { this }, c => c.colour == colour);
        r.Add(this);
        foreach (CandyScript c in r)
        {
            c.isDead = true;
        }
    }

    private void OnMouseDown()
    {
        Color color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        List<CandyScript> neighbours = manager.BFS(new List<CandyScript> { this }, c => c.colour == colour);
        neighbours.Add(this);
        foreach (CandyScript neighbour in neighbours)
        {
            neighbour.gameObject.GetComponent<MeshRenderer>().material.color = color;
        }
    }
}
