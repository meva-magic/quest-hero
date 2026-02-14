using UnityEngine;
using UnityEngine.AI;

public class RunState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    private float runFastSpeed;
    private float runSlowSpeed;
    private float runFastDuration;
    private float runAwayDistance;
    
    private float runFastTimer;
    private bool isRunningFast = true;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player,
        float runFastSpeed, float runSlowSpeed, float runFastDuration, float runAwayDistance)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.runFastSpeed = runFastSpeed;
        this.runSlowSpeed = runSlowSpeed;
        this.runFastDuration = runFastDuration;
        this.runAwayDistance = runAwayDistance;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        isRunningFast = true;
        runFastTimer = runFastDuration;
        agent.speed = runFastSpeed;
        
        PickRunDestination();
    }
    
    public void UpdateState()
    {
        if (isRunningFast)
        {
            runFastTimer -= Time.deltaTime;
            if (runFastTimer <= 0)
            {
                isRunningFast = false;
                agent.speed = runSlowSpeed;
            }
        }
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            PickRunDestination();
        }
    }
    
    void PickRunDestination()
    {
        Vector3 runDirection = (transform.position - player.position).normalized;
        Vector3 runPoint = transform.position + runDirection * runAwayDistance;
        
        if (NavMesh.SamplePosition(runPoint, out NavMeshHit hit, runAwayDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    public bool IsRunningFast() => isRunningFast;
}
