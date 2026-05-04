using UnityEngine;

public class PlayerCollisionMenuTrigger : MonoBehaviour
{
    public VRMenuToggle menu;

    private int trafficCarLayer;
    private int obstacleLayer;

    void Start()
    {
        trafficCarLayer = LayerMask.NameToLayer("Traffic");
        obstacleLayer = LayerMask.NameToLayer("Obstacle");
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    void HandleCollision(GameObject hit)
    {
        int layer = hit.layer;

        // Ignore road or safe objects if needed
        if (layer == LayerMask.NameToLayer("Road"))
            return;

        // Trigger menu on crash-worthy objects
        if (layer == trafficCarLayer || layer == obstacleLayer)
        {
            menu.OpenMenuFromCollision();
        }
    }
}