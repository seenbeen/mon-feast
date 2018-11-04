using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostCandyScript : MonoBehaviour {

    public CandyScript.Colour colour;
    Animator an = null;
    // Use this for initialization
    void Start () {
        an = GetComponent<Animator>();
        an.SetInteger("State", (int)CandyScript.State.EXPLODE);
        an.SetInteger("Colour", (int)colour);
    }
	
	// Update is called once per frame
	void Update () {
        if (an.GetBool("StartedAnimation") && an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) {
            Destroy(gameObject);
        }
    }
}
