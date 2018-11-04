using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public void LoadByIndex(int sceneIndex)
    {
        Time.timeScale = 1; // Fix to unpause a game that left on game-over
        SceneManager.LoadScene(sceneIndex);
    }
}