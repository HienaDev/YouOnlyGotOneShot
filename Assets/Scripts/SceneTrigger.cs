using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [Tooltip("Name of the scene to load when triggered")]
    [SerializeField] private string targetSceneName = "NextScene";

    [Tooltip("Only objects with this tag will trigger the scene change")]
    [SerializeField] private string triggeringTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerCharacter>() != null)
        {
            // If the player character enters the trigger, load the target scene
            SceneManager.LoadScene(targetSceneName);
        }

    }

    public void LoadScene()
    {
        // This method can be called to load the scene programmatically
        SceneManager.LoadScene(targetSceneName);
    }

    public void ExitGame()
    {
        // This method can be called to exit the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop playing in the editor
#else
        Application.Quit(); // Quit the application in a build
#endif
    }
}
