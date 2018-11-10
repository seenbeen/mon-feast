using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameOnEnable : MonoBehaviour {
    [SerializeField]
    ZeMasterMind GM;

    private void OnEnable()
    {
        GM.SetGamePaused(false);
    }
}
