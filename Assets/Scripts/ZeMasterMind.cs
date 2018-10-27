using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeMasterMind : MonoBehaviour {

    // The master mind game-master which controls how difficult things are
    // at any given point in time and also referees the game
    [SerializeField]
    private PlayerController player = null;
    [SerializeField]
    private CandyManager tileGen = null;
    [SerializeField]
    private FallingCandyGenerator candyGen = null;
    [SerializeField]
    private Camera mainCamera = null;
    [SerializeField]
    private Color startColor = new Color(49.0f / 255, 77.0f / 255, 121.0f / 255);
    [SerializeField]
    private Color endColor = new Color(1.0f, 0, 0);

    [SerializeField]
    private float deathHeight = 10.0f;

    [SerializeField]
    private float expectedDurationSeconds = 240.0f;
    [SerializeField]
    private float startingPConstant = 2.0f;
    [SerializeField]
    private float expectedFinalPConstant = 0.01f;
    [SerializeField]
    private float startingRisePeriod = 16.0f;
    [SerializeField]
    private float endingRisePeriod = 2.0f;
    [SerializeField]
    private float startingFallingGeneratorInterval = 1.0f;
    [SerializeField]
    private float endingFallingGeneratorInterval = 0.25f;


    private float duration = 0.0f;
    private float difficulty_scalar, rise_period_scalar, falling_candy_scalar;

    void Start () {
        difficulty_scalar = (expectedFinalPConstant - startingPConstant) / expectedDurationSeconds;
        rise_period_scalar = (endingRisePeriod - startingRisePeriod) / expectedDurationSeconds;
        falling_candy_scalar = (endingFallingGeneratorInterval - startingFallingGeneratorInterval) / expectedDurationSeconds;
    }

    // a hack
    private bool gameOver = false;

	void Update () {
        if (!gameOver)
        {

            duration += Time.deltaTime;
            tileGen.chainLengthProbabilityDecay = Mathf.Max(startingPConstant + difficulty_scalar * duration, expectedFinalPConstant);
            tileGen.SetRisePeriod(Mathf.Max(startingRisePeriod + rise_period_scalar * duration, endingRisePeriod));
            candyGen.generationInterval = Mathf.Max(startingFallingGeneratorInterval + falling_candy_scalar * duration, endingFallingGeneratorInterval);
            // tint screen
            float interp = Mathf.Log(Mathf.Max(1, Mathf.Exp(1) * Mathf.Min(tileGen.GetHighestHeight() / deathHeight, 0.9f)));
            if (tileGen.GetHighestHeight() >= deathHeight)
            {
                player.Freeze();
                candyGen.Freeze();
                tileGen.Freeze();
                gameOver = true;
            }
            mainCamera.backgroundColor = Color.Lerp(startColor, endColor, interp);
        }
    }
}
