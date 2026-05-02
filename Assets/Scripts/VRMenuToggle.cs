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

    public void ToggleMenu()
    {
        if (menuUI.activeSelf)
        {
            menuUI.SetActive(false);
            return;
        }

        ShowMenu();
    }

    public void ShowMenu()
    {
        // Position in front of player
        Vector3 spawnPos =
            playerCamera.position +
            playerCamera.forward * distanceInFront;

        spawnPos.y = playerCamera.position.y;

        menuUI.transform.position = spawnPos;

        // FIXED: correct facing direction
        Vector3 lookDir =
            menuUI.transform.position - playerCamera.position;

        lookDir.y = 0;

        menuUI.transform.rotation =
            Quaternion.LookRotation(lookDir);

        menuUI.SetActive(true);
    }

    public void OpenMenuFromCollision()
    {
        if (!menuUI.activeSelf)
        {
            ShowMenu();
        }
    }
}