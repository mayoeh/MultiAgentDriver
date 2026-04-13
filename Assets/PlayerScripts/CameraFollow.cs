using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 4, -6);
    public float followSpeed = 8f;
    public float rotateSpeed = 10f;

    void Awake()
    {
        ResolveTargetIfNeeded();
    }

    void LateUpdate()
    {
        if (!ResolveTargetIfNeeded()) return;

        // position
        Vector3 desiredPos = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);

        // look at sphere
        Quaternion lookRot = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
    }

    bool ResolveTargetIfNeeded()
    {
        if (target) return true;

        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput)
        {
            target = playerInput.transform.root;
            return true;
        }

        GameObject carRoot = GameObject.Find("CarRoot");
        if (carRoot)
        {
            target = carRoot.transform;
            return true;
        }

        return false;
    }
}