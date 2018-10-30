using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingCandyScript : MonoBehaviour {
    public Vector2 fallVelocity = new Vector2();

    private PolyPieceGenerator ppGen = new PolyPieceGenerator();
    private Rigidbody2D rb;
    private bool currently_frozen = false;
    private Vector2 freeze_frame_vel;

    [HideInInspector]
    public CandyScript.Colour colour = CandyScript.Colour.RED;
    [HideInInspector]
    public bool isDead;

    public float mainRadius = 0.15f;

    public bool debugRender = true;
    public Material mainMaterial;
    public Material outlineMaterial = null;

    public Color redColour = new Color(1.0f, 0.2f, 0.2f);
    public Color greenColour = new Color(0.2f, 1.0f, 0.2f);
    public Color blueColour = new Color(0.2f, 0.2f, 1.0f);

    public Sprite redSprite = null;
    public Sprite greenSprite = null;
    public Sprite blueSprite = null;

    private GameObject sprite_child = null;

    // Use this for initialization
    void Start () {
        ppGen.numberOfSides = 12;
        ppGen.debugRender = this.debugRender;
        ppGen.mainRadius = this.mainRadius;
        ppGen.mainMaterial = new Material(this.mainMaterial)
        {
            color = GetDebugColour(colour)
        };
        ppGen.outlineMaterial = this.outlineMaterial;
        ppGen.generateCollider = true;
        ppGen.colliderIsTrigger = true;
        ppGen.Generate(gameObject);
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = fallVelocity;
        freeze_frame_vel = fallVelocity;

        // actual sprite
        sprite_child = new GameObject { name = "Sprite" };
        SpriteRenderer s = sprite_child.AddComponent<SpriteRenderer>();
        s.sprite = GetColourSprite(colour);
        s.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        sprite_child.transform.position = transform.position + new Vector3(0, 0, -1);
        sprite_child.transform.SetParent(transform, true);
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
            rb.velocity = new Vector2();
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
            rb.velocity = freeze_frame_vel;
            currently_frozen = false;
        }
        else
        {
            Debug.LogWarning("Unfreezing unfrozen falling candy.");
        }
    }

    Color GetDebugColour(CandyScript.Colour colour)
    {
        Color col = new Color();
        switch (colour)
        {
            case CandyScript.Colour.RED:
                col = redColour;
                break;
            case CandyScript.Colour.GREEN:
                col = greenColour;
                break;
            case CandyScript.Colour.BLUE:
                col = blueColour;
                break;
            default:
                break;
        }
        return col;
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
