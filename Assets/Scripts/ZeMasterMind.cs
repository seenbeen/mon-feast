using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeMasterMind : MonoBehaviour {

    // The master mind game-master which controls how difficult things are
    // at any given point in time and also referees the game
    [SerializeField]
    private Muter muter = null;

    [SerializeField]
    private GameObject pausePanel = null;

    [SerializeField]
    private GameObject gameOverPanel = null;
    
    [SerializeField]
    private CandyManager candyManager = null;
    [SerializeField]
    private FallingCandyGenerator candyGen = null;

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

    private bool isCurrentlyPaused = false;

    private float duration = 0.0f;
    private float difficulty_scalar, rise_period_scalar, falling_candy_scalar;

    void Start () {
        difficulty_scalar = (expectedFinalPConstant - startingPConstant) / expectedDurationSeconds;
        rise_period_scalar = (endingRisePeriod - startingRisePeriod) / expectedDurationSeconds;
        falling_candy_scalar = (endingFallingGeneratorInterval - startingFallingGeneratorInterval) / expectedDurationSeconds;
        SetGamePaused(true);
    }

	void Update () {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            SetGamePausedWithScreen(!isCurrentlyPaused);
        }
        if (!isCurrentlyPaused)
        {

            duration += Time.deltaTime;
            candyManager.chainLengthProbabilityDecay = Mathf.Max(startingPConstant + difficulty_scalar * duration, expectedFinalPConstant);
            float rp = Mathf.Max(startingRisePeriod + rise_period_scalar * duration, endingRisePeriod);
            rp *= Modulator(candyManager.GetHighestHeight());
            candyManager.SetRisePeriod(rp);
            candyGen.generationInterval = Mathf.Max(startingFallingGeneratorInterval + falling_candy_scalar * duration, endingFallingGeneratorInterval);
            // tint screen
            if (candyManager.GetHighestHeight() >= deathHeight)
            {
                SetGamePaused(true);
                gameOverPanel.SetActive(true);
            }
        }
    }

    float Modulator(float h)
    {
        float modulation;
        if (h <= 4)
        {
            modulation = 0.25f;
        } else if (h <= 8)
        {
            modulation = 1.0f;
        } else
        {
            modulation = 2.0f;
        }
        return modulation;
    }

    public void SetGamePaused(bool isPaused)
    {
        if (isCurrentlyPaused == isPaused)
        {
            return;
        }
        isCurrentlyPaused = isPaused;
        muter.isPaused = isCurrentlyPaused;
        if (isCurrentlyPaused)
        {
            Time.timeScale = 0;
        } else
        {
            Time.timeScale = 1;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SetGamePausedWithScreen(true);
        }  
    }

    public void SetGamePausedWithScreen(bool isPaused)
    {
        if (isCurrentlyPaused == isPaused)
        {
            return;
        }
        pausePanel.SetActive(isPaused);
        SetGamePaused(isPaused);
    }
}
