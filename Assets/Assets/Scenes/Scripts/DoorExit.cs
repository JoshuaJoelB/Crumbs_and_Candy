using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;  

public class DoorExit : MonoBehaviour
{
    [SerializeField] private int sceneIndex = 1;
    [SerializeField] private GameObject prompt;

    private bool playerNearby;
    private bool isTransitioning;

    private void Start()
    {
        playerNearby = false;
        isTransitioning = false;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerNearby || isTransitioning)
            return;

        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            isTransitioning = true;

            //Debug.Log("E pressed. Loading scene index: " + sceneIndex);

            SceneManager.LoadScene(sceneIndex);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        //Debug.Log("Player entered door trigger.");

        if (prompt != null)
        {
            prompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        //Debug.Log("Player exited door trigger.");

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }
}
