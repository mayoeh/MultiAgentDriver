using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuToggle : MonoBehaviour
{
    public GameObject menuUI;
    public Transform playerCamera;

    public InputActionReference toggleMenuAction;
    public float distanceInFront = 2f;
    private Transform originalParent;

    private bool menuActive = false;


    // void OnEnable()
    // {
    //     toggleMenuAction.action.Enable();
    //     toggleMenuAction.action.performed += OnToggleMenu;
    // }

    // void OnDisable()
    // {
    //     toggleMenuAction.action.performed -= OnToggleMenu;
    //     toggleMenuAction.action.Disable();
    // }

    void Start()
    {
        menuUI.SetActive(false);
        originalParent = transform.parent;
        gameObject.SetActive(false);
    }

    // void Update()
    // {
    //     // Keep menu in front of player while active
    //     if (menuActive)
    //     {
    //         PositionMenuInFront();
    //     }
    // }

    // private void OnToggleMenu(InputAction.CallbackContext context)
    // {
    //     ToggleMenu();
    // }

    // public void ToggleMenu()
    // {
    //     if (menuUI.activeSelf)
    //     {
    //         menuUI.SetActive(false);
    //         return;
    //     }

    //     ShowMenu();
    // }

    // public void ShowMenu()
    // {
    //     // Position in front of player
    //     Vector3 spawnPos =
    //         playerCamera.position +
    //         playerCamera.forward * distanceInFront;

    //     spawnPos.y = playerCamera.position.y;

    //     menuUI.transform.position = spawnPos;

    //     // FIXED: correct facing direction
    //     Vector3 lookDir =
    //         menuUI.transform.position - playerCamera.position;

    //     lookDir.y = 0;

    //     menuUI.transform.rotation =
    //         Quaternion.LookRotation(lookDir);

    //     menuUI.SetActive(true);
    // }

    // void PositionMenuInFront()
    // {
    //     // Position menu in front of camera
    //     transform.position = playerCamera.position + playerCamera.forward * distanceInFront;

    //     // Face the player
    //     transform.LookAt(playerCamera);

    //     // Flip so front side faces player
    //     transform.Rotate(0, 180, 0);
    // }

    // public void OpenMenuFromCollision()
    // {
    //     if (menuActive) return;

    //     menuActive = true;
    //     gameObject.SetActive(true);

    //     PositionMenuInFront();
    // }

    // public void ResetMenu()
    // {
    //     menuActive = false;
    //     gameObject.SetActive(false);
    // }

    public void OpenMenuFromCollision()
    {
        if (menuActive) return;

        menuActive = true;

        // Parent to camera so it follows naturally
        transform.SetParent(playerCamera);

        // Position in front of camera
        transform.localPosition = new Vector3(0, 0, distanceInFront);

        // Face the player
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(true);
    }

    public void ResetMenu()
    {
        Debug.Log("Tried to click");
        menuActive = false;

        // Unparent if needed
        transform.SetParent(originalParent);

        gameObject.SetActive(false);
    }
}
