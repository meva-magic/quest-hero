using UnityEngine;
using UnityEngine.AI;

public class FindState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    private float findSpeed;
    private float wobbleStrength;
    private float stopDistance;  // How close to get before stopping
    
    private float wobblePhase;
    private Vector3 wobbleOffset;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player,
        float findSpeed, float wobbleStrength, float stopDistance)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.findSpeed = findSpeed;
        this.wobbleStrength = wobbleStrength;
        this.stopDistance = stopDistance;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        agent.speed = findSpeed;
        wobblePhase = Random.Range(0f, Mathf.PI * 2);
        agent.SetDestination(player.position);
    }
    
    public void UpdateState()
    {
        // Add wobble to movement
        wobblePhase += Time.deltaTime * 3f;
        wobbleOffset = new Vector3(
            Mathf.Sin(wobblePhase) * wobbleStrength * Time.deltaTime,
            0,
            Mathf.Cos(wobblePhase * 0.7f) * wobbleStrength * Time.deltaTime
        );
        
        if (agent.velocity.magnitude > 0.1f)
        {
            transform.position += wobbleOffset;
        }
        
        // Only update destination if we're NOT close to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > stopDistance)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.SetDestination(player.position);
            }
        }
    }
}
