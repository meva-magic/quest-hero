using UnityEngine;
using UnityEngine.AI;

public class DisappearState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    private float disappearSpeed;
    private float disappearDistance;
    private bool disableGameObject;
    
    private Vector3 escapeDirection;
    private bool hasDisabled = false;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player,
        float disappearSpeed, float disappearDistance, bool disableGameObject)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.disappearSpeed = disappearSpeed;
        this.disappearDistance = disappearDistance;
        this.disableGameObject = disableGameObject;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        agent.speed = disappearSpeed;
        hasDisabled = false;
        
        PickRandomEscapeDirection();
    }
    
    public void UpdateState()
    {
        if (hasDisabled) return;
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            PickRandomEscapeDirection();
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > disappearDistance)
        {
            Disappear();
        }
    }
    
    void PickRandomEscapeDirection()
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0;
        
        Vector3 escapePoint = transform.position + randomDirection * disappearDistance;
        
        if (NavMesh.SamplePosition(escapePoint, out NavMeshHit hit, disappearDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            escapeDirection = randomDirection;
        }
        else
        {
            Invoke(nameof(PickRandomEscapeDirection), 0.2f);
        }
    }
    
    void Disappear()
    {
        if (hasDisabled) return;
        
        hasDisabled = true;
        agent.isStopped = true;
        
        if (disableGameObject)
        {
            controller.gameObject.SetActive(false);
        }
        
        else
        {
            Destroy(controller.gameObject);
        }
    }
    
    public bool HasDisappeared() => hasDisabled;
}
