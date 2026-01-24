using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuestNPC : MonoBehaviour
{
    public static QuestNPC instance;

    private enum Behaviours
    {
        Patrol,
        Listen,
        Chase
    }

    private enum PathMode
    {
        Random,
        Looping,
        ReverseLooping
    }

    [SerializeField] Behaviours currentState;
    [SerializeField] PathMode currentPathMode;

    private NavMeshAgent agent;
    private GameObject player;

    private Vector3 target;
    private Vector3 startingPos;

    [SerializeField] private float stoppingDistance = 0.3f;

    [SerializeField] private List<Transform> waypoints;

    private int currentWaypoint = 0;
    private bool isWaiting = false;
    private bool stateChanged;
    private bool inRange;

    [SerializeField] private bool pauseAtEnd, pauseAtEachPoint;
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
        target = startingPos;
        stateChanged = true;

        // Start patrolling
        if (currentState == Behaviours.Patrol)
        {
            moveCoroutine = StartCoroutine(Move());
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case Behaviours.Patrol:
                if (stateChanged)
                {
                    if (moveCoroutine != null)
                        StopCoroutine(moveCoroutine);
                    
                    moveCoroutine = StartCoroutine(Move());
                    stateChanged = false;
                }
                break;

            case Behaviours.Listen:
                if (stateChanged)
                {
                    // Stop movement
                    agent.isStopped = true;
                    if (moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                        moveCoroutine = null;
                    }
                    stateChanged = false;
                }
                break;
                
            case Behaviours.Chase:
                if (stateChanged)
                {
                    // Stop patrol coroutine
                    if (moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                        moveCoroutine = null;
                    }
                    stateChanged = false;
                }
                target = player.transform.position;
                agent.SetDestination(target);
                break;
        }
    }

    private IEnumerator Move()
    {
        // Make sure waypoints exist
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogWarning("No waypoints assigned to QuestNPC!");
            yield break;
        }

        agent.isStopped = false;
        
        while (currentState == Behaviours.Patrol)
        {    
            // Set destination to current waypoint
            if (waypoints[currentWaypoint] != null)
            {
                agent.SetDestination(waypoints[currentWaypoint].position);
                
                // Wait until we reach the waypoint
                while (agent.pathPending || agent.remainingDistance > stoppingDistance)
                {
                    yield return null;
                    
                    // Check if state changed while moving
                    if (currentState != Behaviours.Patrol)
                        yield break;
                }
                
                // We've reached the waypoint
                if (pauseAtEachPoint || (pauseAtEnd && currentWaypoint == waypoints.Count - 1 && currentPathMode == PathMode.Looping))
                {
                    float pauseTime = Random.Range(minPause, maxPause);
                    float timer = 0f;
                    
                    while (timer < pauseTime && currentState == Behaviours.Patrol)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                }
                
                // Update to next waypoint based on path mode
                UpdateNextWaypoint();
            }
            else
            {
                Debug.LogWarning($"Waypoint {currentWaypoint} is null!");
                yield break;
            }
        }
    }

    private void UpdateNextWaypoint()
    {
        switch (currentPathMode)
        {
            case PathMode.Looping:
                currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
                break;
                
            case PathMode.ReverseLooping:
                if (currentWaypoint == 0)
                {
                    currentWaypoint = waypoints.Count - 1;
                }
                else
                {
                    currentWaypoint--;
                }
                break;
                
            case PathMode.Random:
                int newWaypoint;
                // Make sure we don't get the same waypoint (unless there's only one)
                do
                {
                    newWaypoint = Random.Range(0, waypoints.Count);
                } 
                while (newWaypoint == currentWaypoint && waypoints.Count > 1);
                
                currentWaypoint = newWaypoint;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            currentState = Behaviours.Listen;
            stateChanged = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            currentState = Behaviours.Patrol;
            stateChanged = true;
        }
    }

    // Helper method to debug waypoints
    private void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                    if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }
        }
    }
}
