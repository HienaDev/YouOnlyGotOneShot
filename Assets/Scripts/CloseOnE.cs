using UnityEngine;

public class CloseOnE : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            gameObject.SetActive(false);
        }
    }
}
