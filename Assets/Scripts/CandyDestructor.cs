using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyDestructor : MonoBehaviour {
    [SerializeField]
    private PlayerController player = null;
    [SerializeField]
    private CandyManager candyManager = null;
    [SerializeField]
    private FallingCandyGenerator candyGen = null;

    public float perChainBlockDestructionTime = 0.050f; // 50 mils by default

    float destruct_timer = 0;
    int destruct_count = 0;
    bool freezing = false;
    List<CandyScript> death_row = new List<CandyScript>();
    CandyScript.Colour death_colour;

    // Use this for initialization
    void Start () {
		
	}

	// Update is called once per frame
	void Update () {
        if (freezing)
        {
            if (destruct_count == 0 || death_row.Count == 0)
            {
                if (destruct_timer <= 0)
                {
                    if (candyManager.SettleStep())
                    {
                        destruct_timer += perChainBlockDestructionTime;
                    }
                    else
                    {
                        freezing = false;
                        player.Unfreeze();
                        candyManager.Unfreeze();
                        candyGen.Unfreeze();
                        death_row.Clear();
                    }
                }
                destruct_timer -= Time.deltaTime;
            } else
            {
                if (destruct_timer <= 0)
                {
                    --destruct_count;
                    destruct_timer += perChainBlockDestructionTime;
                    DestructNextTile();
                }
                destruct_timer -= Time.deltaTime;
            }
        }
	}
    
    public void DestructTile(CandyScript script, int count)
    {
        if (freezing)
        {
            return;
        }
        destruct_count = count;
        destruct_timer = 0;
        freezing = true;
        death_colour = script.colour;
        death_row.Add(script);
        player.Freeze();
        candyManager.Freeze();
        candyGen.Freeze();
    }

    // does a safety check before enqueueing the script to ensure the script doesn't already exist
    void SafeEnqueue(CandyScript script)
    {
        if (death_row.Contains(script))
        {
            Debug.Log("Warning: Double enqueue.");
            return;
        }
        death_row.Add(script);
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
