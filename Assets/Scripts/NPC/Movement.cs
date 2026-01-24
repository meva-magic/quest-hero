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
        Listen,
    }

    [SerializeField] Behaviours currentState;

    private NavMeshAgent agent;
    private GameObject player;

    private Vector3 target;
    private Vector3 startingPos;

    [SerializeField] private float stoppingDistance = 0.1f;

    [SerializeField] private List<Transform> waypoints;

    private int currentWaypoint;
    private bool stateChanged;

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

        if (waypoints.Count != 0)
        {
            currentWaypoint = 0;

            player.transform.position = waypoints[currentWaypoint].position;
        }

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
                    agent.isStopped = true;
                    if (moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                        moveCoroutine = null;
                    }
                    stateChanged = false;
                }
                break;
        }
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
                
                if (pauseAtEachPoint || (pauseAtEnd && currentWaypoint == waypoints.Count - 1))
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
            stateChanged = true;

            currentState = Behaviours.Listen;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            stateChanged = true;

            currentState = Behaviours.Patrol;
        }
    }
}