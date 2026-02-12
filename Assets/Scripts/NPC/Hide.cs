using UnityEngine;
using UnityEngine.AI;

public class HideState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    // Settings passed from main script
    private float hideSpeed;
    private float wanderRange;
    private float wobbleStrength;
    
    private Vector3 wobbleOffset;
    private float wobblePhase;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player, 
        float hideSpeed, float wanderRange, float wobbleStrength)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.hideSpeed = hideSpeed;
        this.wanderRange = wanderRange;
        this.wobbleStrength = wobbleStrength;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        agent.speed = hideSpeed;
        wobblePhase = Random.Range(0f, Mathf.PI * 2);
        PickNewDestination();
    }
    
    public void UpdateState()
    {
        // Add wobble to movement
        wobblePhase += Time.deltaTime * 2f;
        wobbleOffset = new Vector3(
            Mathf.Sin(wobblePhase) * wobbleStrength * Time.deltaTime,
            0,
            Mathf.Cos(wobblePhase * 0.7f) * wobbleStrength * Time.deltaTime
        );
        
        if (agent.velocity.magnitude > 0.1f)
        {
            transform.position += wobbleOffset;
        }
        
        // Pick new destination when reached current one
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            PickNewDestination();
        }
    }
    
    void PickNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += transform.position;
        
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
