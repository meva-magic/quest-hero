using UnityEngine;
using UnityEngine.AI;

public class TauntState : MonoBehaviour
{
    private HideSeekNPC controller;
    private NavMeshAgent agent;
    private Transform player;
    
    // Settings passed from main script
    private float tauntSpeed;
    private float tauntWobbleStrength;
    private float stopDistance;
    
    private float wobblePhase;
    private Vector3 wobbleOffset;
    
    public void Initialize(HideSeekNPC controller, NavMeshAgent agent, Transform player,
        float tauntSpeed, float tauntWobbleStrength, float stopDistance)
    {
        this.controller = controller;
        this.agent = agent;
        this.player = player;
        this.tauntSpeed = tauntSpeed;
        this.tauntWobbleStrength = tauntWobbleStrength;
        this.stopDistance = stopDistance;
    }
    
    public void EnterState()
    {
        agent.isStopped = false;
        agent.speed = tauntSpeed;
        wobblePhase = Random.Range(0f, Mathf.PI * 2);
        agent.SetDestination(player.position);
    }
    
    public void UpdateState()
    {
        // Add wobble to movement
        wobblePhase += Time.deltaTime * 3f;
        wobbleOffset = new Vector3(
            Mathf.Sin(wobblePhase) * tauntWobbleStrength * Time.deltaTime,
            0,
            Mathf.Cos(wobblePhase * 0.7f) * tauntWobbleStrength * Time.deltaTime
        );
        
        if (agent.velocity.magnitude > 0.1f)
        {
            transform.position += wobbleOffset;
        }
        
        // Constantly chase player
        if (!agent.pathPending && agent.remainingDistance <= stopDistance * 2)
        {
            agent.SetDestination(player.position);
        }
    }
}
