using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField]
    private CandyDestructor candyDestructor = null;

    public float destructivePowerLog = 2;

    public float maxVelx = 5.0f;
    public float maxVely = 5.0f;
    public float accel = 4.0f;
    public float gAccel = 1.0f;
    public float deflectYVel = 10.0f;
    public float radius = 0.5f;

    public CandyScript.Colour colour = CandyScript.Colour.BLUE;

    public bool debugRender = true;
    public Material mainMaterial = null;
    public Material outlineMaterial = null;

    public Color redColour = new Color(1.0f, 0.0f, 0.0f);
    public Color greenColour = new Color(0.0f, 1.0f, 0.0f);
    public Color blueColour = new Color(0.0f, 0.0f, 1.0f);

    private Rigidbody2D rb;
    private Vector2 last_frame_vel;
    private Vector2 freeze_frame_vel;
    private bool currently_frozen = false;
    private PolyPieceGenerator ppGen = new PolyPieceGenerator();

    private float destructive_counter;

    private bool has_slammed = false;

    // Use this for initialization
	void Start () {
        ppGen.numberOfSides = 12;
        ppGen.debugRender = this.debugRender;
        ppGen.mainRadius = this.radius;
        ppGen.mainMaterial = new Material(this.mainMaterial)
        {
            color = GetDebugColour(this.colour)
        };
        ppGen.outlineMaterial = this.outlineMaterial;
        ppGen.generateCollider = true;
        ppGen.colliderIsTrigger = false;
        ppGen.Generate(gameObject);

        rb = GetComponent<Rigidbody2D>();

        destructive_counter = destructivePowerLog;
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

    void SetColour(CandyScript.Colour colour)
    {
        this.colour = colour;
        if (this.debugRender)
        {
            ppGen.mainMaterial.color = GetDebugColour(colour);
        }
    }

    public void Freeze()
    {
        if (!currently_frozen)
        {
            freeze_frame_vel = rb.velocity;
            rb.velocity = new Vector2();
            currently_frozen = true;
        } else
        {
            Debug.LogWarning("Freezing frozen player.");
        }
    }

    public void Unfreeze()
    {
        if (currently_frozen)
        {
            rb.velocity = freeze_frame_vel;
            currently_frozen = false;
        } else
        {
            Debug.LogWarning("Unfreezing unfrozen player.");
        }
    }

    // Update is called once per frame
    void Update () {
        if (currently_frozen)
        {
            rb.velocity = new Vector2();
            return;
        }
        Vector2 cur_vel = rb.velocity;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cur_vel.x -= accel * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            cur_vel.x += accel * Time.deltaTime;
        }

        cur_vel.x = Mathf.Clamp(cur_vel.x, -maxVelx, maxVelx);
 
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetColour((CandyScript.Colour)(((int)colour + 1) % 3));
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && !has_slammed)
        {
            has_slammed = true;
            cur_vel.y = -deflectYVel;
            cur_vel.x = 0;
        }
        cur_vel.y -= gAccel * Time.deltaTime;
        cur_vel.y = Mathf.Clamp(cur_vel.y, -maxVely, maxVely);
        rb.velocity = cur_vel;

        last_frame_vel = rb.velocity;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        switch (col.collider.tag) {
            case "Candy-Block":
                {
                    rb.velocity = col.contacts[0].normal * deflectYVel;
                    break;
                }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        switch (col.collider.tag)
        {
            case "Boundary-Floor":
                {
                    Vector2 cur_vel = last_frame_vel;
                    cur_vel.y = Mathf.Sign(cur_vel.y) * -deflectYVel;
                    rb.velocity = cur_vel;
                    break;
                }
            case "Boundary-Wall":
                {
                    Vector2 cur_vel = last_frame_vel;
                    cur_vel.x *= -1;
                    rb.velocity = cur_vel;
                    break;
                }
            case "Candy-Block":
                {
                    rb.velocity = col.contacts[0].normal * deflectYVel;
                    if (has_slammed)
                    {
                        bool will_destroy = colour == col.collider.gameObject.GetComponent<CandyScript>().colour;
                        if (will_destroy)
                        {
                            int destructivePower = Mathf.FloorToInt(Mathf.Log(destructive_counter, destructivePowerLog));
                            candyDestructor.DestructTile(col.collider.gameObject.GetComponent<CandyScript>(), destructivePower);
                        }
                    }
                    has_slammed = false;
                    break;
                }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        switch (col.tag)
        {
            case "Good-Candy":
                {
                    SetColour(col.gameObject.GetComponent<FallingCandyScript>().colour);
                    col.GetComponent<FallingCandyScript>().isDead = true;
                    ++destructive_counter;
                    break;
                }
        }
    }
}
