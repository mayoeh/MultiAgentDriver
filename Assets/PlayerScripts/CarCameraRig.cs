using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pins a camera rig (Meta XR OVRCameraRig, or any Transform holding a Camera) to the car
/// in first person, and detaches it for a free-look chase cam in third person.
///
/// First person: cameraRig is reparented to <see cref="firstPersonAnchor"/> (a child of the
/// car), so the car's motion carries it automatically — in play mode and in the editor.
///
/// Third person: cameraRig is detached to world space and follows the car at an offset
/// which the player can orbit with right stick / arrow keys / right-mouse drag.
///
/// Runs with ExecuteAlways so dragging CarRoot in the editor carries the rig along.
/// </summary>
[ExecuteAlways]
public class CarCameraRig : MonoBehaviour
{
    public enum View { FirstPerson, ThirdPerson }

    [Header("References")]
    [Tooltip("The rig to move. For Meta XR this is the OVRCameraRig / [BuildingBlock] Camera Rig transform.")]
    public Transform cameraRig;

    [Tooltip("Empty transform inside the car marking the driver's head pose for 1st person.")]
    public Transform firstPersonAnchor;

    [Tooltip("Optional: transform whose world pose seeds the seat (e.g. CenterEyeAnchor). " +
             "When assigned, firstPersonAnchor is snapped to this pose so the 1st-person view " +
             "matches wherever the head camera was manually placed in the editor.")]
    public Transform headReference;

    [Tooltip("Re-snap firstPersonAnchor to headReference's pose every time this component is enabled.")]
    public bool autoSnapToHeadReference = true;

    [Tooltip("Car root transform — used as the follow target in 3rd person.")]
    public Transform carRoot;

    [Tooltip("CenterEyeAnchor (or whichever child of cameraRig is the actual Camera). " +
             "If set, the rig is offset so the eye itself lands on firstPersonAnchor, " +
             "cancelling out the OVR-authored rig-to-eye local offset.")]
    public Transform centerEyeAnchor;

    [Header("Third Person")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 3f, -6.5f);
    [Tooltip("Higher = snappier follow. 0 = no smoothing.")]
    public float followSmoothing = 10f;
    public float yawSpeed = 140f;
    public float pitchSpeed = 80f;
    public Vector2 pitchClamp = new Vector2(-20f, 60f);

    [Header("Input")]
    public Key toggleKey = Key.C;
    public View startView = View.FirstPerson;

    public View Current { get; private set; }

    float _yaw;
    float _pitch = 12f;
    bool _eyeOffsetApplied;

    void OnEnable()
    {
        Current = startView;
        _eyeOffsetApplied = false;
        if (autoSnapToHeadReference) SnapSeatToHeadReference();
        if (cameraRig && carRoot) _yaw = carRoot.eulerAngles.y;
        ApplyView(snap: true);
    }

    [ContextMenu("Snap Driver Seat To Head Reference")]
    public void SnapSeatToHeadReference()
    {
        if (!firstPersonAnchor || !headReference) return;
        firstPersonAnchor.SetPositionAndRotation(headReference.position, headReference.rotation);
    }

    void LateUpdate()
    {
        if (Application.isPlaying && Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
            Toggle();

        if (Current == View.FirstPerson) KeepPinnedFirstPerson();
        else if (Application.isPlaying)  UpdateThirdPerson();
    }

    public void Toggle() => SetView(Current == View.FirstPerson ? View.ThirdPerson : View.FirstPerson);

    public void SetView(View v)
    {
        if (Current == v) return;
        Current = v;
        _eyeOffsetApplied = false;
        ApplyView(snap: true);
    }

    void ApplyView(bool snap)
    {
        if (!cameraRig) return;

        if (Current == View.FirstPerson)
        {
            KeepPinnedFirstPerson();
        }
        else
        {
            if (cameraRig.parent != null)
                cameraRig.SetParent(null, worldPositionStays: true);

            if (carRoot) _yaw = carRoot.eulerAngles.y;
            if (snap) SnapThirdPerson();
        }
    }

    void KeepPinnedFirstPerson()
    {
        if (!cameraRig || !firstPersonAnchor) return;

        if (cameraRig.parent != firstPersonAnchor)
        {
            cameraRig.SetParent(firstPersonAnchor, worldPositionStays: true);
            _eyeOffsetApplied = false;
        }

        if (centerEyeAnchor && !_eyeOffsetApplied)
        {
            // Place cameraRig (local to firstPersonAnchor) such that CenterEyeAnchor ends up
            // exactly on firstPersonAnchor. Cancels OVR's fixed rig-to-eye offset (TrackingSpace
            // + CenterEyeAnchor authored local pose). Applied once per enable / view switch so
            // HMD head motion still reaches the eye through the unchanged sub-hierarchy.
            Vector3 eyeLocalPos = cameraRig.InverseTransformPoint(centerEyeAnchor.position);
            Quaternion eyeLocalRot = Quaternion.Inverse(cameraRig.rotation) * centerEyeAnchor.rotation;

            Quaternion rigLocalRot = Quaternion.Inverse(eyeLocalRot);
            Vector3 rigLocalPos = rigLocalRot * -eyeLocalPos;
            cameraRig.localPosition = rigLocalPos;
            cameraRig.localRotation = rigLocalRot;
            _eyeOffsetApplied = true;
        }
    }

    void UpdateThirdPerson()
    {
        if (!carRoot || !cameraRig) return;

        Vector2 look = ReadLook();
        _yaw   += look.x * yawSpeed   * Time.deltaTime;
        _pitch  = Mathf.Clamp(_pitch - look.y * pitchSpeed * Time.deltaTime, pitchClamp.x, pitchClamp.y);

        Quaternion orbit = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos = carRoot.position + orbit * thirdPersonOffset;

        float k = followSmoothing <= 0f ? 1f : 1f - Mathf.Exp(-followSmoothing * Time.deltaTime);
        cameraRig.position = Vector3.Lerp(cameraRig.position, desiredPos, k);
        cameraRig.rotation = Quaternion.Slerp(cameraRig.rotation, orbit, k);
    }

    void SnapThirdPerson()
    {
        if (!carRoot || !cameraRig) return;
        Quaternion orbit = Quaternion.Euler(_pitch, _yaw, 0f);
        cameraRig.position = carRoot.position + orbit * thirdPersonOffset;
        cameraRig.rotation = orbit;
    }

    static Vector2 ReadLook()
    {
        Vector2 v = Vector2.zero;
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.isPressed)  v.x -= 1f;
            if (kb.rightArrowKey.isPressed) v.x += 1f;
            if (kb.upArrowKey.isPressed)    v.y += 1f;
            if (kb.downArrowKey.isPressed)  v.y -= 1f;
        }
        Mouse m = Mouse.current;
        if (m != null && m.rightButton.isPressed)
        {
            Vector2 d = m.delta.ReadValue();
            v.x += d.x * 0.05f;
            v.y += d.y * 0.05f;
        }
        Gamepad gp = Gamepad.current;
        if (gp != null) v += gp.rightStick.ReadValue();
        return v;
    }
}
