using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyScript : MonoBehaviour {
    // Structs and Enums
    public enum Colour { RED = 0, GREEN, BLUE, WHITE };
    public enum State { IDLE = 0, GLOW, EXPLODE };

    [SerializeField]
    AudioClip settleInPlaceClip = null;

    // Candy-Specific Information
    [HideInInspector]
    public CandyManager manager = null;
    [SerializeField]
    private GameObject ghostCandyPrefab = null;

    [HideInInspector]
    public Colour colour = Colour.BLUE;

    [HideInInspector]
    public bool isDead = false;

    private State state = State.IDLE;

    // Component refs
    private Animator an = null;
    private AudioSource audioSource = null;

    // Use this for initialization
    void Start () {
        Debug.Assert(manager != null); // safety check... only manager should be creating Candies...
        an = GetComponent<Animator>();
        an.Play(Animator.StringToHash("Red_Idle"));
        an.SetInteger("Colour", (int)colour);
        audioSource = GetComponent<AudioSource>();
    }

    public bool IsAnimComplete()
    {
        return an.GetBool("StartedAnimation") && an.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
    }

    public void SetState(State state)
    {
        this.state = state;
        an.SetInteger("State", (int)state);
        an.SetBool("StartedAnimation", false);
    }

    public State GetState()
    {
        return this.state;
    }

    public void SpawnGhost()
    {
        GameObject ghost = Instantiate(ghostCandyPrefab);
        ghost.transform.position = transform.position;
        ghost.GetComponent<GhostCandyScript>().colour = colour;
    }

    public void PlaySettleSound()
    {
        audioSource.PlayOneShot(settleInPlaceClip, 0.6f);
    }
}
