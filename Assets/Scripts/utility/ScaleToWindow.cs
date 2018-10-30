using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleToWindow : MonoBehaviour {

    Vector4 screen_dim_vector;
    
    // Use this for initialization
    void Start () {
        screen_dim_vector = new Vector4();
    }
	
	// Update is called once per frame
	void Update () {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float crh = Camera.main.orthographicSize * 2 * Camera.main.rect.height;
        float crw = crh * Camera.main.aspect;
        float width = sr.sprite.bounds.size.x / 1.15f; // woopity wop; mahjik numberz
        float height = sr.sprite.bounds.size.y / 1.075f;

        Vector4 cur_dim_vector = new Vector4(crw, crh, width, height);
        if ((screen_dim_vector - cur_dim_vector).magnitude > 0.01f)
        {
            transform.localScale = new Vector3(crw  / width, crh / height, 1);
            screen_dim_vector = cur_dim_vector;
        }
    }
}
