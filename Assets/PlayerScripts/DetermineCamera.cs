using UnityEngine;
using UnityEngine.XR;
using Fusion;

public class DetermineCamera : NetworkBehaviour
{
    [SerializeField] private GameObject normalCamera;
    [SerializeField] private GameObject ovrCamera;

    public override void Spawned()
    {
        bool isLocal = HasInputAuthority;
        bool useVR = isLocal && XRSettings.isDeviceActive;

        if (ovrCamera) ovrCamera.SetActive(isLocal && useVR);
        if (normalCamera) normalCamera.SetActive(isLocal && !useVR);
    }
}