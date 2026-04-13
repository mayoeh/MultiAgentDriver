using UnityEngine;

/// <summary>
/// Runs before Cinemachine: applies a one-time local Y rotation to the first child (car mesh) only.
/// CarRoot transform stays unchanged so driving axes are unaffected.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class CarMeshStartRotation : MonoBehaviour
{
    [Tooltip("Added to the mesh's local rotation around Y (degrees), once in Awake.")]
    public float addLocalYawDegrees = 90f;

    void Awake()
    {
        if (transform.childCount == 0) return;

        Transform mesh = transform.GetChild(0);
        mesh.localRotation = mesh.localRotation * Quaternion.Euler(0f, addLocalYawDegrees, 0f);
    }
}
