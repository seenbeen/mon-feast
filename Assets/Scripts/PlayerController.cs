using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private enum SlamState { NOT_SLAMMING, SLAMMING, SUPER_SLAMMING }
    [SerializeField]
    private CandyDestructor candyDestructor = null;

    public float destructivePowerLog = 2;

    public float maxVelx = 5.0f;
    public float maxVely = 5.0f;
    public float accel = 4.0f;
    public float gAccel = 1.0f;
    public float max_height = 9.0f;
    public float slam_vel = 10.0f;

    public CandyScript.Colour colour = CandyScript.Colour.BLUE;

    private Rigidbody2D rb;
    private Vector2 last_frame_vel;
    private Vector2 freeze_frame_vel;
    private bool currently_frozen = false;

    private float destructive_counter;

    private SlamState slam_state = SlamState.NOT_SLAMMING;

    private PlayerAnimationController pac = null;

    // Use this for initialization
	void Start () {

        rb = GetComponent<Rigidbody2D>();

        destructive_counter = destructivePowerLog;
        pac = GetComponent<PlayerAnimationController>();
    }

    void SetColour(CandyScript.Colour colour)
    {
        this.colour = colour;
        pac.SetColour((int)this.colour + 1);
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

        if (cur_vel.y > 0)
        {
            slam_state = SlamState.NOT_SLAMMING;
            pac.Slam(false);
        }

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

        if (Input.GetKeyDown(KeyCode.DownArrow) && slam_state == SlamState.NOT_SLAMMING)
        {
            pac.Slam(true);
            slam_state = SlamState.SLAMMING;
            cur_vel.y = -slam_vel;
            cur_vel.x = 0;
        } else if (Input.GetKeyDown(KeyCode.UpArrow) && slam_state == SlamState.NOT_SLAMMING)
        {
            pac.Slam(true);
            pac.SetColour((int)CandyScript.Colour.WHITE + 1);
            slam_state = SlamState.SUPER_SLAMMING;
            cur_vel.y = -slam_vel * 2;
            cur_vel.x = 0;
        }


        cur_vel.y -= gAccel * Time.deltaTime;
        cur_vel.y = Mathf.Clamp(cur_vel.y, -maxVely, maxVely);
        rb.velocity = cur_vel;

        last_frame_vel = rb.velocity;
    }

    void HandleCandyBlockCollision(Collision2D col)
    {
        // no bouncing back n forth
        if (Mathf.Abs(Vector3.Dot(col.GetContact(0).normal, new Vector3(1, 0))) > 0.8391f)
        {
            rb.velocity = Vector3.Reflect(last_frame_vel, col.contacts[0].normal);
        }
        else
        {
            rb.velocity = RestrictVelocityToCeilingHeight(col.GetContact(0).normal);
            if (slam_state != SlamState.NOT_SLAMMING)
            {
                bool is_super_slam = slam_state == SlamState.SUPER_SLAMMING;
                bool will_destroy = colour == col.collider.gameObject.GetComponent<CandyScript>().colour || is_super_slam;
                if (will_destroy)
                {
                    SetColour(colour);
                    int INF = 1000; // 10 * 10 = 100; 1000 is more than safe to be considered inf
                    candyDestructor.DestructTile(col.collider.gameObject.GetComponent<CandyScript>(), INF, is_super_slam);
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        switch (col.collider.tag) {
            case "Boundary-Floor":
                {
                    rb.velocity = RestrictVelocityToCeilingHeight(new Vector2(0,1));
                    break;
                }
            case "Candy-Block":
                {
                    HandleCandyBlockCollision(col);
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
                    cur_vel.y = 1;
                    rb.velocity = RestrictVelocityToCeilingHeight(cur_vel);
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
                    HandleCandyBlockCollision(col);
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
                    pac.EatCandy();
                    SetColour(col.gameObject.GetComponent<FallingCandyScript>().colour);
                    col.GetComponent<FallingCandyScript>().isDead = true;
                    ++destructive_counter;
                    break;
                }
        }
    }

    Vector2 RestrictVelocityToCeilingHeight(Vector2 vel)
    {
        // h = v_0 * t + 1/2 * a * t ^ 2
        // (max_height - my_height) = h_max
        // 0 = v_0 + a * t
        // t_max = -v_0 / a
        // h_max = v_0 * t_max + 1/2 * a * t_max ^ 2
        // h_max = v_0 * -v_0 / a + 1/2 * a * v_0 ^ 2 / a ^ 2
        // h_max = v_0 ^ 2 * (-1 / a + 1/2 * 1 / a)
        // h_max = v_0 ^ 2 * (-1 / ( 2 * a))
        // v_0 = sqrt(h_max * -2 * a)
        // y_v = v_0
        // ny/nx = M
        // y_v / x_v = M
        // x_v = y_v / M
        // x_v = y_v * nx / ny
        float h_max = max_height - transform.position.y;
        float y_v = Mathf.Sqrt(h_max * 2 * gAccel);
        float x_v = y_v * vel.x / vel.y;
        return new Vector2(x_v, y_v);
    }
}
