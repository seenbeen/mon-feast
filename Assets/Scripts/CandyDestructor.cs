using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyDestructor : MonoBehaviour {
    private enum State { IDLE, SUPER, GLOW, WAIT_GLOW, SETTLE, SUPER_CHAIN }

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
    bool is_super = false;
    
    List<CandyScript> death_row = new List<CandyScript>();
    HashSet<CandyScript> dedded = new HashSet<CandyScript>();

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
            case State.SUPER:
                {
                    SuperExplodeTiles();
                    state = State.SETTLE;
                    break;
                }
            case State.GLOW:
                {
                    if (destruct_count == 0 || death_row.Count == 0)
                    {
                        destruct_timer = perChainBlockDestructionTime;
                        death_row.Clear();
                        state = State.WAIT_GLOW;
                        break;
                    }

                    if (destruct_timer <= 0)
                    {
                        --destruct_count;
                        destruct_timer += perChainBlockDestructionTime;
                        GlowNextTiles();
                    }
                    destruct_timer -= Time.deltaTime;
                    break;
                }
            case State.WAIT_GLOW:
                {
                    bool done_glow = true;
                    foreach (CandyScript c in dedded)
                    {
                        if (c.GetState() == CandyScript.State.GLOW && !c.IsAnimComplete())
                        {
                            done_glow = false;
                            break;
                        }
                    }
                    if (done_glow)
                    {
                        foreach (CandyScript c in dedded)
                        {
                            c.isDead = true;
                        }
                        dedded.Clear();
                        state = State.SETTLE;
                    }
                    break;
                }
            case State.SETTLE:
                {
                    if (destruct_timer <= 0)
                    {
                        CandyScript settled = candyManager.SettleStep();
                        if (settled == null)
                        {
                            destruct_timer = 0;
                            state = State.SUPER_CHAIN;
                            break;
                        }
                        settled_candies.Add(settled);
                        destruct_timer += perChainBlockDestructionTime;
                    }
                    destruct_timer -= Time.deltaTime;
                    break;
                }
            case State.SUPER_CHAIN:
                {

                    if (is_super)
                    {
                        foreach (CandyScript s in settled_candies)
                        {
                            death_row.Add(s);
                        }
                        destruct_count = 1000; // you're God-like :)
                        is_super = false;
                        state = State.GLOW;
                    } else
                    {
                        player.Unfreeze();
                        candyManager.Unfreeze();
                        candyGen.Unfreeze();
                        state = State.IDLE;
                    }
                    settled_candies.Clear();
                    break;
                }
        }
	}
    
    public void DestructTile(CandyScript script, int count, bool is_super)
    {
        switch (state)
        {
            case State.IDLE:
                {
                    destruct_count = count;
                    
                    Debug.Assert(destruct_timer == 0);
                    Debug.Assert(death_row.Count == 0);
                    Debug.Assert(settled_candies.Count == 0);
                    Debug.Assert(dedded.Count == 0);

                    death_row.Add(script);

                    this.is_super = is_super;

                    player.Freeze();
                    candyManager.Freeze();
                    candyGen.Freeze();
                    state = this.is_super ? State.SUPER : State.GLOW;
                    break;
                }
        }
    }

    void SuperExplodeTiles()
    {
        Debug.Assert(death_row.Count == 1);
        CandyScript c = death_row[0];
        death_row.Clear();
        // insta-kill
        List<CandyScript> flooded = candyManager.BFS(new List<CandyScript> { c }, cc => cc.colour == c.colour, -1);
        foreach (CandyScript cc in flooded)
        {
            cc.isDead = true;
        }
        c.isDead = true;
    }

    void GlowNextTiles()
    {
        HashSet<CandyScript> marked = new HashSet<CandyScript>();
        foreach (CandyScript c in death_row)
        {
            c.SetState(CandyScript.State.GLOW);
            settled_candies.Remove(c);
            List<CandyScript> flooded = candyManager.BFS(new List<CandyScript> { c }, cc => !dedded.Contains(cc) && cc.colour == c.colour, 1);
            foreach (CandyScript s in flooded)
            {
                marked.Add(s);
            }
        }
        foreach (CandyScript c in death_row)
        {
            dedded.Add(c);
            marked.Remove(c);
        }
        death_row = new List<CandyScript>(marked);
    }
}
