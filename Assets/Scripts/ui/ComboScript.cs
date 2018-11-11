using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboScript : MonoBehaviour {

    public enum Type { GOOD = 0, GREAT, AWESOME };
    public Type type;
    string[] ANIM_NAMES = { "Combo Good", "Combo Great", "Combo Awesome" };
    Animator an = null;
    // Use this for initialization
    void Start()
    {
        an = GetComponent<Animator>();
        an.Play(ANIM_NAMES[(int)this.type]);
    }

    // Update is called once per frame
    void Update()
    {
        if (an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            Destroy(gameObject);
        }
    }
}
