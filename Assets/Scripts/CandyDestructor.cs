using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyDestructor : MonoBehaviour {
    private enum State { IDLE, DESTROYING_CANDY, STABILIZING_CANDY, CHAINING_CANDY }

    [SerializeField]
    PlayerController player = null;

    [SerializeField]
    CandyManager candyManager = null;

    [SerializeField]
    FallingCandyGenerator candyGen = null;

    public float perChainBlockDestructionTime = 0.050f; // 50 mils by default

    State state = State.IDLE;
    float destruct_timer = 0;
    int initial_destruct_count = 0;
    int destruct_count = 0;
    bool is_chain = false;

    List<CandyScript> death_row = new List<CandyScript>();
    HashSet<CandyScript> settled_candies = new HashSet<CandyScript>();

    // Use this for initialization
    void Start () {
		
	}

	// Update is called once per frame
	void Update () {
        switch (state)
        {
            case State.IDLE:
                {
                    break;
                }
            case State.DESTROYING_CANDY:
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
            case State.STABILIZING_CANDY:
                {
                    if (destruct_timer <= 0)
                    {
                        CandyScript settled = candyManager.SettleStep();
                        if (settled == null)
                        {
                            destruct_timer = 0;
                            state = State.CHAINING_CANDY;
                            break;
                        }
                        settled_candies.Add(settled);
                        destruct_timer += perChainBlockDestructionTime;
                    }
                    destruct_timer -= Time.deltaTime;
                    break;
                }
            case State.CHAINING_CANDY:
                {
                    if (is_chain && (initial_destruct_count == 0 || true))
                    {
                        while (settled_candies.Count > 0)
                        {
                            foreach (CandyScript s in settled_candies)
                            {
                                death_row.Add(s);
                                List<CandyScript> flood = candyManager.BFS(death_row, c => c.colour == s.colour);
                                settled_candies.Remove(s);
                                foreach (CandyScript fs in flood)
                                {
                                    settled_candies.Remove(fs);
                                }
                                break;
                            }
                        }
                        destruct_count = 10000; // you're temporarily God-like :)
                        is_chain = false;
                        settled_candies.Clear();
                        state = State.DESTROYING_CANDY;
                        break;
                    }
                    settled_candies.Clear();
                    player.Unfreeze();
                    candyManager.Unfreeze();
                    candyGen.Unfreeze();
                    state = State.IDLE;
                    break;
                }
        }
	}
    
    public void DestructTile(CandyScript script, int count, bool is_chain)
    {
        switch (state)
        {
            case State.IDLE:
                {
                    destruct_count = count;
                    initial_destruct_count = destruct_count;

                    Debug.Assert(destruct_timer == 0);
                    Debug.Assert(death_row.Count == 0);
                    Debug.Assert(settled_candies.Count == 0);

                    death_row.Add(script);

                    this.is_chain = is_chain;

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
        HashSet<CandyScript> marked = new HashSet<CandyScript>();
        foreach (CandyScript c in death_row)
        {
            c.isDead = true;
            settled_candies.Remove(c);
            List<CandyScript> flooded = candyManager.BFS(new List<CandyScript> { c }, cc => cc.colour == c.colour, 1);
            foreach (CandyScript s in flooded)
            {
                marked.Add(s);
            }
        }
        foreach (CandyScript c in death_row)
        {
            marked.Remove(c);
        }
        death_row = new List<CandyScript>(marked);
    }
}
