using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorMatcher : MonoBehaviour {

    public Color color = new Color(1,1,1,0);

	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().material.color = color;
	}
	
	// Update is called once per frame
	void Update () {
        GetComponent<SpriteRenderer>().material.color = color;
    }
}
