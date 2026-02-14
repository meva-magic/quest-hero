using UnityEngine;
using UnityEngine.AI;

public class IdleState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    
    private float idleSpeed;
    private float wanderRange;
    private float wobbleStrength;
    
    private Vector3 wobbleOffset;
    private float wobblePhase;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, 
        float idleSpeed, float wanderRange, float wobbleStrength)
    {
        this.controller = controller;
        this.agent = agent;
        this.idleSpeed = idleSpeed;
        this.wanderRange = wanderRange;
        this.wobbleStrength = wobbleStrength;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        agent.speed = idleSpeed;
        wobblePhase = Random.Range(0f, Mathf.PI * 2);
        PickRandomDestination();
    }
    
    public void UpdateState()
    {
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
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            PickRandomDestination();
        }
    }
    
    void PickRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += transform.position;
        
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
