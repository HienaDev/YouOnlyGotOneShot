using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetCurrentLevel : MonoBehaviour
{



    private void OnEnable()
    {
        // Ensure the time scale is set to a slower speed when the script is enabled
        Time.timeScale = 0.2f;
    }

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
        Time.timeScale = 1f; // Reset time scale to normal
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
