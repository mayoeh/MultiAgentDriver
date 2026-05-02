using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuToggle : MonoBehaviour
{
    public GameObject menuUI;
    public Transform playerCamera;

    public InputActionReference toggleMenuAction;

    public float distanceInFront = 2f;

    void OnEnable()
    {
        toggleMenuAction.action.Enable();
        toggleMenuAction.action.performed += OnToggleMenu;
    }

    void OnDisable()
    {
        toggleMenuAction.action.performed -= OnToggleMenu;
        toggleMenuAction.action.Disable();
    }

    void Start()
    {
        menuUI.SetActive(false);
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }

    void ToggleMenu()
    {
        if (!menuUI.activeSelf)
        {
            Vector3 spawnPos =
                playerCamera.position +
                playerCamera.forward * distanceInFront;

            // Keep menu level with player
            spawnPos.y = playerCamera.position.y;

            menuUI.transform.position = spawnPos;

            // Face the player
            Vector3 lookDirection =
                menuUI.transform.position - playerCamera.position;

            lookDirection.y = 0;

            menuUI.transform.rotation =
                Quaternion.LookRotation(lookDirection);

            menuUI.SetActive(true);
        }
        else
        {
            menuUI.SetActive(false);
        }
    }
}