using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCheats : MonoBehaviour
{
    [Header("Cheat Settings")]
    [SerializeField] private string sampleScene = "SampleScene";
    [SerializeField] private string level1 = "Level1";
    [SerializeField] private string level2 = "Level2";
    [SerializeField] private string level3 = "Level3";
    [SerializeField] private string mainMenu = "MainMenu";

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.F1))
                SceneManager.LoadScene(sampleScene);

            if (Input.GetKeyDown(KeyCode.F2))
                SceneManager.LoadScene(level1);

            if (Input.GetKeyDown(KeyCode.F3))
                SceneManager.LoadScene(level2);

            if (Input.GetKeyDown(KeyCode.F4))
                SceneManager.LoadScene(level3);

            if (Input.GetKeyDown(KeyCode.F5))
                SceneManager.LoadScene(mainMenu);
        }
    }
}
