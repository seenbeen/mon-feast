using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperSlamBarScript : MonoBehaviour {

    [SerializeField]
    GameObject BarGlow = null, BarFilling = null, BarFillingFull = null;
    [SerializeField]
    float rateOfScale = 0.5f; // how much closer to the target does the bar get per second
    [SerializeField]
    float glowFlickersPerSecond = 1;

    SpriteRenderer glow_renderer = null;
    SpriteRenderer bar_full_renderer = null;
    
    float cur_val, target_val = 0;
    float cur_glow = 0, target_glow = 0;
    
    float glow_rate;

	// Use this for initialization
	void Start () {
        cur_val = target_val;
        glow_renderer = BarGlow.GetComponent<SpriteRenderer>();
        glow_rate = 2 * glowFlickersPerSecond;
        bar_full_renderer = BarFillingFull.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        float delta = (target_val - cur_val) * rateOfScale * Time.deltaTime;
        cur_val += delta;

        // did we overshoot the target?
        if ((target_val - cur_val) * delta < 0 || Mathf.Abs(target_val - cur_val) < 0.01f)
        {
            cur_val = target_val;
        }

        float glowSign = Mathf.Sign(target_glow - cur_glow);
        cur_glow += glowSign * glow_rate * Time.deltaTime;
        if ((target_glow - cur_glow) * glowSign < 0)
        {
            cur_glow = target_glow; // did we overshoot the target?
        }
        
        if (cur_val == 1.0f)
        {
            if (delta != 0)
            {
                target_glow = 1.0f;
                cur_glow = 0;
            }
            if (cur_glow == target_glow)
            {
                target_glow = 1 - target_glow;
            }
        }

        if (target_val != 1.0f)
        {
            target_glow = 0;
        }

        SetBar();
        SetGlow();
	}

    void SetBar()
    {
        float d_percent = bar_full_renderer.sprite.bounds.size.x * transform.localScale.x * (1 - cur_val);

        BarFilling.transform.position = new Vector3(-d_percent, 0, 0) + transform.position;

        if (cur_val == 1.0f)
        {
            bar_full_renderer.material.color = new Color(1, 1, 1, 1);
        } else
        {
            bar_full_renderer.material.color = new Color(1, 1, 1, 0);
        }
    }

    void SetGlow()
    {
        glow_renderer.material.color = new Color(1, 1, 1, cur_glow);
    }

    public void SetBarLevel(float val)
    {
        val = Mathf.Clamp01(val);
        target_val = val;
    }
}
