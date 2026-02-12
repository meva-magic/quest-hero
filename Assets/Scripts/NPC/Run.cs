using UnityEngine;
using UnityEngine.AI;

public class RunState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    // Settings passed from main script
    private float sprintSpeed;
    private float jogSpeed;
    private float sprintDuration;
    private float runAwayDistance;
    
    private float sprintTimer;
    private bool isSprinting = true;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player,
        float sprintSpeed, float jogSpeed, float sprintDuration, float runAwayDistance)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.sprintSpeed = sprintSpeed;
        this.jogSpeed = jogSpeed;
        this.sprintDuration = sprintDuration;
        this.runAwayDistance = runAwayDistance;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        isSprinting = true;
        sprintTimer = sprintDuration;
        agent.speed = sprintSpeed;
        
        Vector3 runDirection = (transform.position - player.position).normalized;
        Vector3 runPoint = transform.position + runDirection * runAwayDistance;
        
        if (NavMesh.SamplePosition(runPoint, out NavMeshHit hit, runAwayDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    public void UpdateState()
    {
        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
            if (sprintTimer <= 0)
            {
                isSprinting = false;
                agent.speed = jogSpeed;
            }
        }
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 runDirection = (transform.position - player.position).normalized;
            Vector3 runPoint = transform.position + runDirection * runAwayDistance;
            
            if (NavMesh.SamplePosition(runPoint, out NavMeshHit hit, runAwayDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
    
    public bool IsSprinting() => isSprinting;
}
