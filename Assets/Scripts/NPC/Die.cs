using UnityEngine;
using UnityEngine.AI;

public class DieState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    private float dieSpeed;
    private float dieDistance;
    
    private bool hasDied = false;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player,
        float dieSpeed, float dieDistance)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.dieSpeed = dieSpeed;
        this.dieDistance = dieDistance;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        agent.speed = dieSpeed;
        hasDied = false;
        
        Speaker speaker = controller.GetComponent<Speaker>();
        if (speaker != null)
        {
            speaker.enabled = false;
        }
        
        PickEscapeDirection();
    }
    
    public void UpdateState()
    {
        if (hasDied) return;
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            PickEscapeDirection();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > dieDistance)
        {
            Die();
        }
    }
    
    void PickEscapeDirection()
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0;
        
        Vector3 escapePoint = transform.position + randomDirection * dieDistance;
        
        if (NavMesh.SamplePosition(escapePoint, out NavMeshHit hit, dieDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Invoke(nameof(PickEscapeDirection), 0.2f);
        }
    }
    
    void Die()
    {
        if (hasDied) return;
        
        hasDied = true;
        agent.isStopped = true;
        controller.gameObject.SetActive(false);
    }
}
