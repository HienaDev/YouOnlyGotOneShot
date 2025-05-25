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
}
