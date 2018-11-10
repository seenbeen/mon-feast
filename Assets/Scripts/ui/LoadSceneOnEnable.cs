using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadSceneOnEnable : MonoBehaviour
{
    public int sceneIndex;
    public void OnEnable()
    {
        Time.timeScale = 1; // Fix to unpause a game that left on game-over
        SceneManager.LoadScene(sceneIndex);
    }
}