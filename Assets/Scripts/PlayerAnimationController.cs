using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
    Animator an = null;

    public float eatAnimTime = 0.5f;

    float eatAnimTimer = 0.0f;
    bool has_slammed = false;
    int colour = 3;

    // Use this for initialization
	void Start () {
        an = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        eatAnimTimer = Mathf.Max(eatAnimTimer - Time.deltaTime, 0);
        an.SetInteger("Colour", colour);
        an.SetBool("Slamming", has_slammed);
        an.SetBool("Eating", eatAnimTimer != 0);
    }

    public void Slam(bool slamming)
    {
        has_slammed = slamming;
    }

    public void EatCandy()
    {
        eatAnimTimer = eatAnimTime;
    }

    public void SetColour(int col)
    {
        colour = col;
    }
}
