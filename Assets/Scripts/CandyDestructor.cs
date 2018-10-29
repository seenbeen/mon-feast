using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyDestructor : MonoBehaviour {
    private enum State { IDLE, DESTROYING_CANDY, STABILIZING_CANDY }

    [SerializeField]
    PlayerController player = null;

    [SerializeField]
    CandyManager candyManager = null;

    [SerializeField]
    FallingCandyGenerator candyGen = null;

    public float perChainBlockDestructionTime = 0.050f; // 50 mils by default

    State state = State.IDLE;
    float destruct_timer = 0;
    int destruct_count = 0;

    List<CandyScript> death_row = new List<CandyScript>();
    CandyScript.Colour death_colour;

    // Use this for initialization
    void Start () {
		
	}

	// Update is called once per frame
	void Update () {
        switch (state)
        {
            case (State.IDLE):
                {
                    break;
                }
            case (State.DESTROYING_CANDY):
                {
                    if (destruct_count == 0 || death_row.Count == 0)
                    {
                        destruct_timer = 0;
                        death_row.Clear();
                        state = State.STABILIZING_CANDY;
                        break;
                    }

                    if (destruct_timer <= 0)
                    {
                        --destruct_count;
                        destruct_timer += perChainBlockDestructionTime;
                        DestructNextTile();
                    }
                    destruct_timer -= Time.deltaTime;
                    break;
                }
            case (State.STABILIZING_CANDY):
                {
                    if (destruct_timer <= 0)
                    {
                        if (!candyManager.SettleStep())
                        {
                            player.Unfreeze();
                            candyManager.Unfreeze();
                            candyGen.Unfreeze();
                            state = State.IDLE;
                            break;
                        }
                        destruct_timer += perChainBlockDestructionTime;
                    }
                    destruct_timer -= Time.deltaTime;
                    break;
                }
        }
	}
    
    public void DestructTile(CandyScript script, int count)
    {
        switch (state)
        {
            case State.IDLE:
                {
                    destruct_count = count;
                    destruct_timer = 0;
                    death_colour = script.colour;
                    death_row.Add(script);

                    player.Freeze();
                    candyManager.Freeze();
                    candyGen.Freeze();
                    state = State.DESTROYING_CANDY;
                    break;
                }
        }
    }

    void DestructNextTile()
    {
        foreach (CandyScript c in death_row)
        {
            c.isDead = true;
        }
        death_row = candyManager.BFS(death_row, c => c.colour == death_colour, 1);
    }
}
