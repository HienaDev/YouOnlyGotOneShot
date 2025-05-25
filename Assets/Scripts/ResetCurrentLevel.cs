using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetCurrentLevel : MonoBehaviour
{
    public void ResetLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
