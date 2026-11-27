using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;  

public class DoorExit : MonoBehaviour
{
    [Header("Settings")]
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

        // Check if Spacebar is pressed
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isTransitioning = true;
            SceneManager.LoadScene(sceneIndex);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger has a CharacterController
        // We use GetComponentInParent just in case the CharacterController is on a parent object
        if (other.GetComponent<CharacterController>() != null || other.GetComponentInParent<CharacterController>() != null)
        {
            playerNearby = true;
            if (prompt != null) prompt.SetActive(true);
            Debug.Log("Player (Character Controller) entered the trigger zone!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharacterController>() != null || other.GetComponentInParent<CharacterController>() != null)
        {
            playerNearby = false;
            if (prompt != null) prompt.SetActive(false);
            Debug.Log("Player (Character Controller) left the trigger zone.");
        }
    }
}