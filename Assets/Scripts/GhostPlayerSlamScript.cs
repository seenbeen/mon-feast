using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPlayerSlamScript : MonoBehaviour {

    Animator an = null;

	// Use this for initialization
	void Start () {
        an = GetComponent<Animator>();
        an.Play("Ghost Explode");
	}
	
	// Update is called once per frame
	void Update () {
		if (an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            Destroy(gameObject);
        }
	}

}
