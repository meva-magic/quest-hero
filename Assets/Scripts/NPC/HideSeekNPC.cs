using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum NPCState
{
    Idle,
    Find,
    Run,
    Die
}

public class HideSeekNPC : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Dialogue dialogueAsset;
    [SerializeField] private ItemObject questItem;
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private GameObject rewardIcon;
    [SerializeField] private Collider npcCollider;
    
    [Header("=== BEHAVIOR ===")]
    public NPCState currentState = NPCState.Idle;
    
    [Header("Speeds")]
    [SerializeField] private float wanderSpeed = 6f;
    [SerializeField] private float approachSpeed = 10f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float dieSpeed = 13f;
    
    [Header("Distances")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float runTriggerDistance = 5f;
    [SerializeField] private float safeDistance = 30f;
    [SerializeField] private float wanderRadius = 18f;
    
    [Header("Timing")]
    [SerializeField] private float findDelay = 3f;
    [SerializeField] private float runFastDuration = 4f;
    
    [Header("Effects")]
    [SerializeField] private float wobbleAmount = 1f;
    
    private float timeSinceLastPlayerContact;
    private bool hasGivenItem = false;
    private bool isInDialogue = false;
    private Transform player;
    private float wobblePhase;
    private bool isRunningFast = true;
    private float runTimer;
    
    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        wobblePhase = Random.Range(0f, Mathf.PI * 2);
        
        agent.stoppingDistance = 0.5f;
        
        if (npcCollider == null)
            npcCollider = GetComponent<Collider>();
        
        if (rewardIcon != null)
            rewardIcon.SetActive(true);
            
        StartIdle();
    }
    
    void Update()
    {
        if (isInDialogue) return;
        
        timeSinceLastPlayerContact += Time.deltaTime;
        wobblePhase += Time.deltaTime * 2f;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        
        // CRITICAL: Always check for run trigger regardless of state
        if (distToPlayer < runTriggerDistance && currentState != NPCState.Die && currentState != NPCState.Run)
        {
            StartRun();
            return;
        }
        
        switch (currentState)
        {
            case NPCState.Idle:
                UpdateIdle();
                
                if (distToPlayer < detectionRange)
                {
                    StartRun();
                }
                else if (timeSinceLastPlayerContact > findDelay)
                {
                    StartFind();
                }
                break;
                
            case NPCState.Find:
                UpdateFind();
                break;
                
            case NPCState.Run:
                UpdateRun();
                
                if (distToPlayer > safeDistance)
                {
                    StartIdle();
                }
                break;
                
            case NPCState.Die:
                UpdateDie();
                break;
        }
    }
    
    void UpdateIdle()
    {
        agent.speed = wanderSpeed;
        
        // Enable collision in Idle
        if (npcCollider != null && !npcCollider.enabled)
            npcCollider.enabled = true;
        
        // Wobble in Idle
        Vector3 wobble = new Vector3(
            Mathf.Sin(wobblePhase) * wobbleAmount * Time.deltaTime,
            0,
            Mathf.Cos(wobblePhase * 0.7f) * wobbleAmount * Time.deltaTime
        );
        transform.position += wobble;
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
            randomPoint.y = transform.position.y;
            
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }
    
    void UpdateFind()
    {
        // DISABLE collision in Find state
        if (npcCollider != null && npcCollider.enabled)
            npcCollider.enabled = false;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        
        // If we're already too close, run immediately
        if (distToPlayer < runTriggerDistance)
        {
            StartRun();
            return;
        }
        
        // Move to exactly runTriggerDistance away from player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 targetPosition = player.position - (directionToPlayer * runTriggerDistance);
        
        agent.speed = approachSpeed;
        agent.SetDestination(targetPosition);
    }
    
    void UpdateRun()
    {
        if (isRunningFast)
        {
            runTimer += Time.deltaTime;
            agent.speed = runSpeed;
            
            // DISABLE collision during fast run
            if (npcCollider != null && npcCollider.enabled)
                npcCollider.enabled = false;
            
            if (runTimer > runFastDuration)
                isRunningFast = false;
        }
        else
        {
            agent.speed = runSpeed * 0.6f;
            
            // ENABLE collision when slows down (catchable)
            if (npcCollider != null && !npcCollider.enabled)
                npcCollider.enabled = true;
        }
        
        Vector3 runDir = (transform.position - player.position).normalized;
        if (runDir == Vector3.zero) runDir = transform.forward;
        
        Vector3 runPoint = transform.position + runDir * 20f;
        
        if (NavMesh.SamplePosition(runPoint, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    void UpdateDie()
    {
        agent.speed = dieSpeed;
        
        // Enable collision in Die
        if (npcCollider != null && !npcCollider.enabled)
            npcCollider.enabled = true;
        
        // Slight wobble in Die
        Vector3 wobble = new Vector3(
            Mathf.Sin(wobblePhase * 3f) * wobbleAmount * 0.5f * Time.deltaTime,
            0,
            Mathf.Cos(wobblePhase * 2f) * wobbleAmount * 0.5f * Time.deltaTime
        );
        transform.position += wobble;
        
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            Vector3 escapeDir = Random.insideUnitSphere.normalized;
            escapeDir.y = 0;
            Vector3 escapePoint = transform.position + escapeDir * 40f;
            
            if (NavMesh.SamplePosition(escapePoint, out NavMeshHit hit, 40f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
        
        if (Vector3.Distance(transform.position, player.position) > 50f)
            gameObject.SetActive(false);
    }
    
    void StartIdle()
    {
        if (currentState == NPCState.Die) return;
        currentState = NPCState.Idle;
        agent.isStopped = false;
        timeSinceLastPlayerContact = 0f;
    }
    
    void StartFind()
    {
        if (currentState == NPCState.Die) return;
        currentState = NPCState.Find;
        agent.isStopped = false;
        timeSinceLastPlayerContact = 0f;
    }
    
    void StartRun()
    {
        if (currentState == NPCState.Die) return;
        currentState = NPCState.Run;
        agent.isStopped = false;
        timeSinceLastPlayerContact = 0f;
        isRunningFast = true;
        runTimer = 0f;
        
        // Immediately set run destination
        Vector3 runDir = (transform.position - player.position).normalized;
        if (runDir == Vector3.zero) runDir = transform.forward;
        
        Vector3 runPoint = transform.position + runDir * 20f;
        
        if (NavMesh.SamplePosition(runPoint, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
    
    void StartDie()
    {
        currentState = NPCState.Die;
        agent.isStopped = false;
        Speaker speaker = GetComponent<Speaker>();
        if (speaker != null)
            speaker.enabled = false;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeSinceLastPlayerContact = 0f;
            
            // Only catch if during slow run with collision enabled
            if (!isInDialogue && !hasGivenItem && currentState == NPCState.Run && !isRunningFast)
            {
                if (npcCollider != null && npcCollider.enabled)
                    StartDialogue();
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            timeSinceLastPlayerContact = 0f;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isInDialogue && !hasGivenItem)
        {
            // Only catch during slow run with collision enabled
            if (currentState == NPCState.Run && !isRunningFast && npcCollider != null && npcCollider.enabled)
            {
                StartDialogue();
            }
        }
    }
    
    void StartDialogue()
    {
        isInDialogue = true;
        agent.isStopped = true;
        
        if (dialogueAsset != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Хранитель артефакта", dialogueAsset.RootNode);
            StartCoroutine(WaitForDialogue());
        }
        else
        {
            GiveReward();
        }
    }
    
    IEnumerator WaitForDialogue()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        GiveReward();
    }
    
    void GiveReward()
    {
        if (hasGivenItem) return;
        
        hasGivenItem = true;
        
        if (rewardIcon != null)
            rewardIcon.SetActive(false);
        
        if (droppedItemPrefab != null && questItem != null)
        {
            Vector3 spawnPos = player.position + new Vector3(0, 0, -2f);
            var dropped = Instantiate(droppedItemPrefab, spawnPos, Quaternion.identity).GetComponent<DroppedItem>();
            if (dropped != null)
                dropped.Initialize(questItem);
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop");
        }
        
        isInDialogue = false;
        StartDie();
    }
}
