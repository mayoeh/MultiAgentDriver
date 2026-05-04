using UnityEngine;
using Fusion;

public class DetermineCamera : NetworkBehaviour
{
    [SerializeField] private GameObject normalCamera;
    [SerializeField] private GameObject ovrCamera;

    public override void Spawned()
    {
        if (Runner.IsSharedModeMasterClient)
        {
            ovrCamera.SetActive(true);
            Debug.Log("Master Client: Using OVR Camera");
        }
        else
        {
            normalCamera.SetActive(true);
            Debug.Log("Client: Using Normal Camera");
        }
    }
}