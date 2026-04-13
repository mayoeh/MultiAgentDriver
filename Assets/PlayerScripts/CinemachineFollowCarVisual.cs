using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Drives Cinemachine to follow the car mesh orientation (first child under the drive root),
/// not only the root transform — important when the GLB has a local rotation vs. CarRoot.
/// CinemachineCamera's own Transform is always overwritten by Cinemachine; adjust framing with
/// Third Person Follow (distance, shoulder) in the inspector, not the Transform position.
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
public class CinemachineFollowCarVisual : MonoBehaviour
{
    [Tooltip("Object that PlayerMove moves (e.g. CarRoot). First child should be the car model.")]
    public Transform vehicleDriveRoot;

    CinemachineCamera _vcam;

    void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
        ApplyTargets();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying && enabled)
        {
            if (_vcam == null) _vcam = GetComponent<CinemachineCamera>();
            ApplyTargets();
        }
    }
#endif

    void ApplyTargets()
    {
        if (_vcam == null || vehicleDriveRoot == null || vehicleDriveRoot.childCount == 0)
            return;

        Transform visual = vehicleDriveRoot.GetChild(0);
        _vcam.Target.TrackingTarget = visual;
        _vcam.Target.LookAtTarget = visual;
    }
}
