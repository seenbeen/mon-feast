using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyScript : MonoBehaviour {
    // Structs and Enums
    public enum Colour { RED=0, GREEN, BLUE, NONE };

    public Sprite redSprite = null;
    public Sprite greenSprite = null;
    public Sprite blueSprite = null;

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

    // the object holding sprite for this candy
    private GameObject sprite_child = null;

    // Use this for initialization
    void Start () {
        Debug.Assert(manager != null); // safety check... only manager should be creating Candies...
        sprite_child = new GameObject{ name = "Sprite" };
        SpriteRenderer s = sprite_child.AddComponent<SpriteRenderer>();
        s.sprite = GetColourSprite(colour);

        sprite_child.transform.localScale = new Vector3(0.48f, 0.48f, 1);
        sprite_child.transform.position = transform.position + new Vector3(0, 0, -1);
        sprite_child.transform.SetParent(transform, true);

        GetRB();
    }

    private Sprite GetColourSprite(CandyScript.Colour colour)
    {
        Sprite result = null;
        switch (colour)
        {
            case CandyScript.Colour.RED:
                result = redSprite;
                break;
            case CandyScript.Colour.GREEN:
                result = greenSprite;
                break;
            case CandyScript.Colour.BLUE:
                result = blueSprite;
                break;
        }
        return result;
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
}
