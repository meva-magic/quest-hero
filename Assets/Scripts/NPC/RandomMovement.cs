using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range; 
    public Transform centrePoint; 
    
    private enum Behaviours
    {
        Patrol,
        Listen
    }

    [SerializeField] private Behaviours currentState;
    private GameObject player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        currentState = Behaviours.Patrol;
    }
    
    void Update()
    {
        switch (currentState)
        {
            case Behaviours.Patrol:
                UpdatePatrol();
                break;
            case Behaviours.Listen:
                UpdateListen();
                break;
        }
    }

    void UpdatePatrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance) 
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point)) 
            {
                agent.SetDestination(point);
            }
        }
    }

    void UpdateListen()
    {
        // Стоим на месте
        agent.isStopped = true;
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        { 
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = Behaviours.Listen;
            agent.isStopped = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = Behaviours.Patrol;
            agent.isStopped = false;
        }
    }
}
