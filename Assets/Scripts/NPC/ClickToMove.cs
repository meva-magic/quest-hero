using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class ClickToMove : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private float moveSpeed = 10f;
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float sampleDistance = 1.0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        
        // If you forget to set groundLayer in the inspector,
        // this will set it to only include the Ground layer (layer 3)
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, sampleDistance, NavMesh.AllAreas))
                {
                    agent.SetDestination(navMeshHit.position);
                }
            }
        }
    }
}
