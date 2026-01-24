using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    public static Movement instance;

    private enum Behaviours
    {
        Patrol,
        Listen
    }

    [SerializeField] Behaviours currentState;

    private NavMeshAgent agent;
    private GameObject player;

    private Vector3 target;

    [SerializeField] private float stoppingDistance = 0.1f;

    [SerializeField] private List<Transform> waypoints;

    private int currentWaypoint;
    private bool stateChanged;

    [SerializeField] private bool pauseAtPoint;
    [SerializeField] private float minPause, maxPause;

    private Coroutine moveCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");

        currentState = Behaviours.Patrol;
        stateChanged = true;

        if (waypoints.Count != 0)
        {
            currentWaypoint = 0;
        }

        StartPatrolling();
    }
/*
    private void Update()
    {
        switch (currentState)
        {
            case Behaviours.Patrol:
                // No need for state change check in Update since we handle it in triggers
                break;

            case Behaviours.Listen:
                // Just stay in listen mode, movement is already stopped
                break;
        }
    }
*/
    private void StartPatrolling()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        
        moveCoroutine = StartCoroutine(Move());
    }

    private void StopPatrolling()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        agent.isStopped = true;
    }

    private IEnumerator Move()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            yield break;
        }

        agent.isStopped = false;
        
        while (currentState == Behaviours.Patrol)
        {    
            if (waypoints[currentWaypoint] != null)
            {
                agent.SetDestination(waypoints[currentWaypoint].position);
                
                while (agent.pathPending || agent.remainingDistance > stoppingDistance)
                {
                    yield return null;
                    
                    if (currentState != Behaviours.Patrol)
                        yield break;
                }
                
                if (pauseAtPoint)
                {
                    float pauseTime = Random.Range(minPause, maxPause);
                    float timer = 0f;
                    
                    while (timer < pauseTime && currentState == Behaviours.Patrol)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                }
                
                UpdateNextWaypoint();
            }

            else
            {
                yield break;
            }
        }
    }

    private void UpdateNextWaypoint()
    {
        if (currentWaypoint == 0)
        {
            currentWaypoint = waypoints.Count - 1;
        }
        else
        {
            currentWaypoint--;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = Behaviours.Listen;
            StopPatrolling();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentState = Behaviours.Patrol;
            StartPatrolling();
        }
    }
}
