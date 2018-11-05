using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPlayerController : MonoBehaviour {
    [SerializeField]
    PlayerController player = null;
    [SerializeField]
    GameObject ghostFlame = null;

    SpriteRenderer flameRenderer = null;
    SpriteRenderer charRenderer = null;

    RaycastHit2D[] results = new RaycastHit2D[1000]; // proly not going to have this much falling candies

    CircleCollider2D playerCol = null;
    Animator playerAn = null;
    Animator an = null;

    // Use this for initialization
	void Start () {
        an = GetComponent<Animator>();
        playerCol = player.GetComponent<CircleCollider2D>();
        playerAn = player.GetComponent<Animator>();

        charRenderer = GetComponent<SpriteRenderer>();
        flameRenderer = ghostFlame.GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
        if (player.IsFrozen())
        {
            return;
        }
        int hit = playerCol.Cast(new Vector2(0, -1), results);
        RaycastHit2D result = results[0];
        for (int i = 0; i < hit; ++i)
        {
            if (results[i].collider.gameObject.tag == "Candy-Block" || results[i].collider.gameObject.tag == "Boundary-Floor")
            {
                result = results[i];
                break;
            }
        }
        transform.position = result.point + result.normal * playerCol.radius * player.transform.localScale;

        an.SetInteger("Colour", playerAn.GetInteger("Colour"));
        an.SetBool("Slamming", playerAn.GetBool("Slamming"));
        an.SetBool("Eating", playerAn.GetBool("Eating"));

        float interp = (player.transform.position.y - transform.position.y) / (player.max_height - transform.position.y);
        charRenderer.material.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, interp));
        flameRenderer.material.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, interp));
    }
}
