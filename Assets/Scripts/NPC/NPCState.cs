using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum NPCState
{
    Idle,
    Find,
    Run,
    Die,
    Base
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
    
    [Header("=== БАЗА ===")]
    [SerializeField] private Transform basePoint;
    [SerializeField] private float baseReturnDistance = 30f;
    [SerializeField] private float playerDistanceCheck = 20f;
    
    [Header("=== BEHAVIOR ===")]
    public NPCState currentState = NPCState.Base;
    
    [Header("State Dialogue Lines")]
    [SerializeField] private List<string> idleLines = new List<string>();
    [SerializeField] private List<string> findLines = new List<string>();
    [SerializeField] private List<string> runLines = new List<string>();
    [SerializeField] private List<string> dieLines = new List<string>();
    
    [Header("Dialogue Settings")]
    [SerializeField] private float dialogueInterval = 3f;
    [SerializeField] private bool showStateDialogue = true;
    
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
    
    [Header("Reward Effects")]
    [SerializeField] private GameObject rewardParticlePrefab;
    [SerializeField] private string rewardSoundName = "Reward";
    
    private float timeSinceLastPlayerContact;
    private bool hasGivenItem = false;
    private bool isInDialogue = false;
    private Transform player;
    private float wobblePhase;
    private bool isRunningFast = true;
    private float runTimer;
    
    private Coroutine dialogueRoutine;

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
        
        if (basePoint == null)
        {
            GameObject baseObj = new GameObject(name + "_BasePoint");
            baseObj.transform.position = transform.position;
            basePoint = baseObj.transform;
        }
        
        StartBase();
    }
    
    void Update()
    {
        if (isInDialogue) return;
        
        timeSinceLastPlayerContact += Time.deltaTime;
        wobblePhase += Time.deltaTime * 2f;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        float distToBase = Vector3.Distance(transform.position, basePoint.position);
        
        bool tooFarFromBase = distToBase > baseReturnDistance;
        bool tooFarFromPlayer = distToPlayer > playerDistanceCheck;
        
        if ((tooFarFromBase || tooFarFromPlayer) && 
            currentState != NPCState.Die && 
            currentState != NPCState.Base)
        {
            StartReturnToBase();
            return;
        }
        
        if (distToPlayer < runTriggerDistance && currentState != NPCState.Die && currentState != NPCState.Run && currentState != NPCState.Base)
        {
            StartRun();
            return;
        }
        
        switch (currentState)
        {
            case NPCState.Base:
                UpdateBase();
                if (distToPlayer < detectionRange * 2 && distToBase < baseReturnDistance * 0.7f)
                {
                    StartIdle();
                }
                break;
                
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
    
    void StartReturnToBase()
    {
        if (currentState == NPCState.Die || currentState == NPCState.Base) return;
        
        currentState = NPCState.Base;
        agent.isStopped = false;
        agent.SetDestination(basePoint.position);
        
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }
    }
    
    void UpdateBase()
    {
        float distToBase = Vector3.Distance(transform.position, basePoint.position);
        
        if (distToBase > 1f)
        {
            agent.speed = wanderSpeed;
            agent.SetDestination(basePoint.position);
        }
        else
        {
            agent.isStopped = true;
            
            Vector3 wobble = new Vector3(
                Mathf.Sin(wobblePhase) * 0.2f * Time.deltaTime,
                0,
                Mathf.Cos(wobblePhase * 0.7f) * 0.2f * Time.deltaTime
            );
            transform.position += wobble;
            
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer < detectionRange * 2)
            {
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }
    
    void UpdateIdle()
    {
        agent.speed = wanderSpeed;
        agent.isStopped = false;
        
        // Коллайдер всегда включен в Idle
        if (npcCollider != null && !npcCollider.enabled)
            npcCollider.enabled = true;
        
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
        // Коллайдер включен в Find (нет неуязвимости)
        if (npcCollider != null && !npcCollider.enabled)
            npcCollider.enabled = true;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distToPlayer < runTriggerDistance)
        {
            StartRun();
            return;
        }
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 targetPosition = player.position - (directionToPlayer * runTriggerDistance);
        
        agent.speed = approachSpeed;
        agent.SetDestination(targetPosition);
    }
    
    void UpdateRun()
    {
        // Обе фазы бега: коллайдер всегда включен (нет временной неуязвимости)
        if (isRunningFast)
        {
            runTimer += Time.deltaTime;
            agent.speed = runSpeed;
            
            if (npcCollider != null && !npcCollider.enabled)
                npcCollider.enabled = true;
            
            if (runTimer > runFastDuration)
                isRunningFast = false;
        }
        else
        {
            agent.speed = runSpeed * 0.6f;
            
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
        
        if (npcCollider != null && !npcCollider.enabled)
            npcCollider.enabled = true;
        
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
    
    void StartBase()
    {
        currentState = NPCState.Base;
        agent.isStopped = true;
        timeSinceLastPlayerContact = 0f;
    }
    
    void StartIdle()
    {
        if (currentState == NPCState.Die) return;
        
        NPCState previousState = currentState;
        currentState = NPCState.Idle;
        agent.isStopped = false;
        timeSinceLastPlayerContact = 0f;
        
        if (showStateDialogue && previousState != currentState)
            StartStateDialogue(NPCState.Idle);
    }
    
    void StartFind()
    {
        if (currentState == NPCState.Die) return;
        
        NPCState previousState = currentState;
        currentState = NPCState.Find;
        agent.isStopped = false;
        timeSinceLastPlayerContact = 0f;
        
        if (showStateDialogue && previousState != currentState)
            StartStateDialogue(NPCState.Find);
    }
    
    void StartRun()
    {
        if (currentState == NPCState.Die) return;
        
        NPCState previousState = currentState;
        currentState = NPCState.Run;
        agent.isStopped = false;
        timeSinceLastPlayerContact = 0f;
        isRunningFast = true;
        runTimer = 0f;
        
        if (showStateDialogue && previousState != currentState)
            StartStateDialogue(NPCState.Run);
        
        Vector3 runDir = (transform.position - player.position).normalized;
        if (runDir == Vector3.zero) runDir = transform.forward;
        
        Vector3 runPoint = transform.position + runDir * 20f;
        
        if (NavMesh.SamplePosition(runPoint, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
    
    void StartDie()
    {
        NPCState previousState = currentState;
        currentState = NPCState.Die;
        agent.isStopped = false;
        
        if (showStateDialogue && previousState != currentState)
            StartStateDialogue(NPCState.Die);
        
        Speaker speaker = GetComponent<Speaker>();
        if (speaker != null)
            speaker.enabled = false;
    }
    
    void StartStateDialogue(NPCState state)
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);
        
        switch (state)
        {
            case NPCState.Idle:
                dialogueRoutine = StartCoroutine(StateDialogueRoutine(idleLines, state));
                break;
            case NPCState.Find:
                dialogueRoutine = StartCoroutine(StateDialogueRoutine(findLines, state));
                break;
            case NPCState.Run:
                dialogueRoutine = StartCoroutine(StateDialogueRoutine(runLines, state));
                break;
            case NPCState.Die:
                dialogueRoutine = StartCoroutine(StateDialogueRoutine(dieLines, state));
                break;
        }
    }
    
    IEnumerator StateDialogueRoutine(List<string> lines, NPCState expectedState)
    {
        if (lines == null || lines.Count == 0 || NPCDialogueUI.Instance == null)
            yield break;
        
        while (currentState == expectedState && !isInDialogue)
        {
            string randomLine = lines[Random.Range(0, lines.Count)];
            NPCDialogueUI.Instance.ShowDialogue(randomLine, dialogueInterval);
            yield return new WaitForSeconds(dialogueInterval);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timeSinceLastPlayerContact = 0f;
            
            // Поймать можно ВСЕГДА, если коллайдер включен
            if (!isInDialogue && !hasGivenItem && npcCollider != null && npcCollider.enabled)
            {
                if (dialogueRoutine != null)
                {
                    StopCoroutine(dialogueRoutine);
                    dialogueRoutine = null;
                }
                
                if (NPCDialogueUI.Instance != null)
                    NPCDialogueUI.Instance.HideDialogueImmediate();
                
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
            if (npcCollider != null && npcCollider.enabled)
            {
                if (dialogueRoutine != null)
                {
                    StopCoroutine(dialogueRoutine);
                    dialogueRoutine = null;
                }
                
                if (NPCDialogueUI.Instance != null)
                    NPCDialogueUI.Instance.HideDialogueImmediate();
                
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
            
            PlayRewardEffects(spawnPos);
        }
        
        isInDialogue = false;
        StartDie();
    }
    
    private void PlayRewardEffects(Vector3 position)
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(rewardSoundName))
            AudioManager.instance.Play(rewardSoundName);
        
        if (rewardParticlePrefab != null)
        {
            Instantiate(rewardParticlePrefab, position, Quaternion.identity);
        }
    }
    
    public void SetBaseReturnConditions(float distToBase, float distToPlayer, float baseReturnDist, float playerCheckDist)
    {
        bool tooFarFromBase = distToBase > baseReturnDist;
        bool tooFarFromPlayer = distToPlayer > playerCheckDist;
        
        if ((tooFarFromBase || tooFarFromPlayer) && 
            currentState != NPCState.Die && 
            currentState != NPCState.Base)
        {
            StartReturnToBase();
        }
    }
}
