using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Front wheels steer as whole assemblies.
/// Submeshes 0 and 1 stay visible and stationary.
/// Submeshes 2 and 3 roll as a child visual.
/// Rear wheels only roll (their roots stay at initial rotation).
/// </summary>
public class CarVisualAnimator : MonoBehaviour
{
    [Header("Input (optional — falls back to PlayerInput / 'Driving/Steer')")]
    public InputActionReference steerAction;

    [Header("Steering Wheel")]
    public Transform steeringWheel;
    public Vector3 steeringWheelAxis = Vector3.forward;
    public float steeringWheelMaxDegrees = 450f;
    public bool invertSteeringWheel = false;
    public float steeringWheelSmoothing = 12f;

    [Header("Wheel Roots (whole assembly)")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("Wheel Steering")]
    public float frontWheelMaxSteerDegrees = 30f;
    public float frontWheelSteerSmoothing = 12f;
    public Vector3 wheelSteerAxis = Vector3.up;

    [Header("Wheel Rolling")]
    public Vector3 wheelRollAxis = Vector3.right;
    public float wheelRadius = 0.35f;

    [Header("Speed Source")]
    public Transform carBody;

    [Header("Generated Child Names")]
    public string staticVisualName = "StaticVisual";
    public string spinVisualName = "SpinVisual";

    private InputAction _steer;
    private bool _ownsSteer;

    private Quaternion _steeringWheelInit;
    private Quaternion _flRootInit, _frRootInit, _rlRootInit, _rrRootInit;
    private Quaternion _flSpinInit, _frSpinInit, _rlSpinInit, _rrSpinInit;

    private Transform _flSpin, _frSpin, _rlSpin, _rrSpin;

    private float _wheelDegrees;
    private float _wheelSteerDeg;
    private float _rollDeg;

    private Vector3 _lastBodyPos;

    void Awake()
    {
        if (!carBody) carBody = transform;
        _lastBodyPos = carBody.position;

        _flSpin = SetupWheelVisuals(frontLeftWheel);
        _frSpin = SetupWheelVisuals(frontRightWheel);
        _rlSpin = SetupWheelVisuals(rearLeftWheel);
        _rrSpin = SetupWheelVisuals(rearRightWheel);

        if (steeringWheel) _steeringWheelInit = steeringWheel.localRotation;

        if (frontLeftWheel)  _flRootInit = frontLeftWheel.localRotation;
        if (frontRightWheel) _frRootInit = frontRightWheel.localRotation;
        if (rearLeftWheel)   _rlRootInit = rearLeftWheel.localRotation;
        if (rearRightWheel)  _rrRootInit = rearRightWheel.localRotation;

        if (_flSpin) _flSpinInit = _flSpin.localRotation;
        if (_frSpin) _frSpinInit = _frSpin.localRotation;
        if (_rlSpin) _rlSpinInit = _rlSpin.localRotation;
        if (_rrSpin) _rrSpinInit = _rrSpin.localRotation;

        if (steerAction != null)
        {
            _steer = steerAction.action;
            _ownsSteer = true;
            return;
        }

        PlayerInput pi = GetComponentInChildren<PlayerInput>();
        if (pi && pi.actions)
        {
            InputActionMap map = pi.actions.FindActionMap("Driving", throwIfNotFound: false);
            if (map != null) _steer = map.FindAction("Steer", throwIfNotFound: false);
        }
    }

    void OnEnable()
    {
        if (_ownsSteer) _steer?.Enable();
    }

    void OnDisable()
    {
        if (_ownsSteer) _steer?.Disable();
    }

    void Update()
    {
        float steer = _steer != null ? Mathf.Clamp(_steer.ReadValue<float>(), -1f, 1f) : 0f;

        float targetWheel = (invertSteeringWheel ? -1f : 1f) * steer * steeringWheelMaxDegrees;
        _wheelDegrees = Mathf.Lerp(_wheelDegrees, targetWheel, 1f - Mathf.Exp(-steeringWheelSmoothing * Time.deltaTime));
        if (steeringWheel)
        {
            Vector3 axis = steeringWheelAxis.sqrMagnitude > 0f ? steeringWheelAxis.normalized : Vector3.forward;
            steeringWheel.localRotation = _steeringWheelInit * Quaternion.AngleAxis(_wheelDegrees, axis);
        }

        float targetSteer = steer * frontWheelMaxSteerDegrees;
        _wheelSteerDeg = Mathf.Lerp(_wheelSteerDeg, targetSteer, 1f - Mathf.Exp(-frontWheelSteerSmoothing * Time.deltaTime));

        Vector3 pos = carBody.position;
        Vector3 delta = pos - _lastBodyPos;
        _lastBodyPos = pos;

        if (Time.deltaTime > 0f)
        {
            float signedSpeed = Vector3.Dot(delta, carBody.forward) / Time.deltaTime;
            float rollDelta = (signedSpeed * Time.deltaTime) / Mathf.Max(wheelRadius, 1e-3f) * Mathf.Rad2Deg;
            _rollDeg = Mathf.Repeat(_rollDeg + rollDelta, 360f);
        }

        Vector3 steerAx = wheelSteerAxis.sqrMagnitude > 0f ? wheelSteerAxis.normalized : Vector3.up;
        Vector3 rollAx = wheelRollAxis.sqrMagnitude > 0f ? wheelRollAxis.normalized : Vector3.right;

        Quaternion steerRot = Quaternion.AngleAxis(_wheelSteerDeg, steerAx);
        Quaternion rollRot = Quaternion.AngleAxis(_rollDeg, rollAx);

        if (frontLeftWheel)  frontLeftWheel.localRotation = _flRootInit * steerRot;
        if (frontRightWheel) frontRightWheel.localRotation = _frRootInit * steerRot;
        if (rearLeftWheel)   rearLeftWheel.localRotation = _rlRootInit;
        if (rearRightWheel)  rearRightWheel.localRotation = _rrRootInit;

        if (_flSpin) _flSpin.localRotation = _flSpinInit * rollRot;
        if (_frSpin) _frSpin.localRotation = _frSpinInit * rollRot;
        if (_rlSpin) _rlSpin.localRotation = _rlSpinInit * rollRot;
        if (_rrSpin) _rrSpin.localRotation = _rrSpinInit * rollRot;
    }

    Transform SetupWheelVisuals(Transform wheelRoot)
    {
        if (!wheelRoot) return null;

        Transform existingSpin = wheelRoot.Find(spinVisualName);
        if (existingSpin) return existingSpin;

        MeshFilter mf = wheelRoot.GetComponent<MeshFilter>();
        MeshRenderer mr = wheelRoot.GetComponent<MeshRenderer>();
        if (!mf || !mr) return null;

        Mesh src = mf.sharedMesh;
        Material[] mats = mr.sharedMaterials;
        if (src == null || mats == null) return null;
        if (src.subMeshCount <= 3 || mats.Length <= 3) return null;

        CreateVisualGroup(wheelRoot, staticVisualName, src, mats, 0, 1);
        Transform spin = CreateVisualGroup(wheelRoot, spinVisualName, src, mats, 2, 3);

        mr.enabled = false;

        return spin;
    }

    Transform CreateVisualGroup(Transform wheelRoot, string groupName, Mesh src, Material[] mats, int submeshA, int submeshB)
    {
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(wheelRoot, false);
        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        group.transform.localScale = Vector3.one;

        CreateSubmeshChild(group.transform, src, mats[submeshA], submeshA, "Part" + submeshA);
        CreateSubmeshChild(group.transform, src, mats[submeshB], submeshB, "Part" + submeshB);

        return group.transform;
    }

    void CreateSubmeshChild(Transform parent, Mesh src, Material mat, int submeshIndex, string name)
    {
        int[] triangles = src.GetTriangles(submeshIndex);
        if (triangles == null || triangles.Length == 0)
        {
            Debug.LogWarning($"{parent.name}: submesh {submeshIndex} has no triangles");
            return;
        }

        Mesh m = new Mesh();
        m.name = $"{parent.name}_{name}_Submesh{submeshIndex}";
        m.vertices = src.vertices;
        m.normals = src.normals;
        m.tangents = src.tangents;
        m.uv = src.uv;
        m.uv2 = src.uv2;
        m.colors = src.colors;
        m.triangles = triangles;
        m.RecalculateBounds();

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        MeshFilter childMF = child.AddComponent<MeshFilter>();
        MeshRenderer childMR = child.AddComponent<MeshRenderer>();

        childMF.sharedMesh = m;
        childMR.sharedMaterial = mat;
    }
}