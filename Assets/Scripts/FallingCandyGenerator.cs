using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingCandyGenerator : MonoBehaviour {

    [SerializeField]
    private CandyManager candy_manager = null;

    public GameObject fallingCandyPrefab = null;
    public Vector2 generatedCandyVelocity = new Vector2(0.0f, -0.5f);
    public float generationInterval = 0.1f;
    public float generationProbability = 0.1f;
    public float generationXMin = 0.0f;
    public float generationXMax = 9.0f;
    public float generationYPos = 14.0f;
    public float generationXDistribution = 1.0f;
    public float generationYDistribution = 1.0f;

    private float current_time = 0;
    private bool currently_frozen;
    private List<FallingCandyScript> candies = new List<FallingCandyScript>();
    private int generationXRange;
    private float[] l_time;
    private float generationYDistributionTime;

    // Use this for initialization
	void Start () {
        current_time = generationInterval;
        generationXRange = Mathf.CeilToInt((generationXMax - generationXMin) / generationXDistribution) + 1;
        generationYDistributionTime = Mathf.Abs(generationYDistribution / generatedCandyVelocity.y);
        l_time = new float[generationXRange];
        for (int i = 0; i < generationXRange; ++i)
        {
            l_time[i] = generationYDistributionTime;
        }
    }

    // Update is called once per frame
    void Update() {
        for (int i = candies.Count - 1; i >= 0; --i)
        {
            if (candies[i].isDead)
            {
                Destroy(candies[i].gameObject);
                candies.RemoveAt(i);
            }
        }

        if (currently_frozen)
        {
            return;
        }

        current_time -= Time.deltaTime;
        for (int i = 0; i < generationXRange; ++i)
        {
            l_time[i] = Mathf.Max(0.0f, l_time[i] - Time.deltaTime);
        }
        if (current_time < 0)
        {
            current_time += generationInterval;
            float trial = Random.Range(0, 1.0f);
            if (trial < generationProbability)
            {
                TrySpawnGoody();
            }
        }
    }

    void TrySpawnGoody()
    {
        bool all_lanes_occupied = true;
        for (int i = 0; i < generationXRange; ++i)
        {
            if (l_time[i] == 0.0f)
            {
                all_lanes_occupied = false;
            }
        }
        if (all_lanes_occupied)
        {
            return;
        }
        GameObject candy = Instantiate(fallingCandyPrefab);
        FallingCandyScript script = candy.GetComponent<FallingCandyScript>();
        script.fallVelocity = generatedCandyVelocity;
        script.colour = candy_manager.GenerateSpawnColour();

        int gen_x_index = Random.Range(0, generationXRange);
        while (l_time[gen_x_index] > 0.0f)
        {
            gen_x_index = Random.Range(0, generationXRange);
        }
        l_time[gen_x_index] += generationYDistributionTime;
        float generated_x = generationXMin + gen_x_index * generationXDistribution;
        candy.transform.position = new Vector2(generated_x, generationYPos);
        candies.Add(script);
    }

    public void Freeze()
    {
        if (!currently_frozen)
        {
            foreach (FallingCandyScript c in candies)
            {
                c.Freeze();
            }
            currently_frozen = true;
        }
        else
        {
            Debug.LogWarning("Freezing frozen falling candy generator.");
        }
    }

    public void Unfreeze()
    {
        if (currently_frozen)
        {
            foreach (FallingCandyScript c in candies)
            {
                c.Unfreeze();
            }
            currently_frozen = false;
        }
        else
        {
            Debug.LogWarning("Unfreezing unfrozen falling candy generator.");
        }
    }
}
