using UnityEngine;
using UnityEngine.AI;

public class Crowd : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject target;
    public GameObject[] AllTargets;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        GetComponent<Animator>().SetInteger("Mode", 1);
        FindTarget();
    }

    void Update()
    {
        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.transform.position) <= 0.5f)
            {
                FindTarget();
            }
        }
    }

    public void FindTarget()
    {
        // Find all available targets
        AllTargets = GameObject.FindGameObjectsWithTag("Target");

        if (AllTargets.Length == 0)
            return;

        // Pick random target
        target = AllTargets[Random.Range(0, AllTargets.Length)];

        // Move agent
        navMeshAgent.SetDestination(target.transform.position);
    }
}