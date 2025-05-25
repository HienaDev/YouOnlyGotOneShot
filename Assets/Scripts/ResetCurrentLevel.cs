using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetCurrentLevel : MonoBehaviour
{

    private void Update()
    {
        // Check for the "R" key press to reset the level
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }
    }

    public void ResetLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
