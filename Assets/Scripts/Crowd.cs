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
        if (target == null) return;

        if (!navMeshAgent.pathPending &&
            navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            FindTarget();
        }
    }

    public void FindTarget()
    {
        AllTargets = GameObject.FindGameObjectsWithTag("Target");

        if (AllTargets.Length == 0) return;

        GameObject newTarget;

        do
        {
            newTarget = AllTargets[Random.Range(0, AllTargets.Length)];
        }
        while (AllTargets.Length > 1 && newTarget == target);

        target = newTarget;

        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(target.transform.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            navMeshAgent.SetDestination(target.transform.position);
        }
    }
}