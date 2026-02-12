using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum NPCState
{
    Hiding,
    Running,
    Taunting,
    Disappearing
}

public class HideSeekNPC : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Dialogue dialogueAsset;
    [SerializeField] private ItemObject questItem;
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private GameObject rewardIcon;
    
    [Header("=== STATE SETTINGS ===")]
    public NPCState currentState = NPCState.Hiding;
    
    [Header("--- Hide State Settings ---")]
    [SerializeField] private float hideSpeed = 2f;
    [SerializeField] private float wanderRange = 10f;
    [SerializeField] private float wobbleStrength = 0.5f;
    
    [Header("--- Run State Settings ---")]
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jogSpeed = 4f;
    [SerializeField] private float sprintDuration = 2.5f;
    [SerializeField] private float runAwayDistance = 15f;
    
    [Header("--- Taunt State Settings ---")]
    [SerializeField] private float tauntSpeed = 5f;
    [SerializeField] private float tauntWobbleStrength = 0.8f;
    [SerializeField] private float tauntStopDistance = 3f;
    
    [Header("--- Disappear State Settings ---")]
    [SerializeField] private float disappearSpeed = 12f;
    [SerializeField] private float disappearDistance = 30f;
    [SerializeField] private bool disableGameObject = true;
    
    [Header("--- Detection Settings ---")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float safeDistance = 15f;
    [SerializeField] private float tauntingDelay = 30f;
    
    [Header("--- Quest Settings ---")]
    [SerializeField] private float itemSpawnOffsetZ = -2f;
    
    private float timeSinceLastFound;
    private bool hasGivenItem = false;
    private bool isInDialogue = false;
    private Transform player;
    
    // State scripts
    private HideState hideState;
    private RunState runState;
    private TauntState tauntState;
    private DisappearState disappearState;
    
    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Add state components
        hideState = gameObject.AddComponent<HideState>();
        runState = gameObject.AddComponent<RunState>();
        tauntState = gameObject.AddComponent<TauntState>();
        disappearState = gameObject.AddComponent<DisappearState>();
        
        // Initialize states with references
        hideState.Initialize(this, agent, player, hideSpeed, wanderRange, wobbleStrength);
        runState.Initialize(this, agent, player, sprintSpeed, jogSpeed, sprintDuration, runAwayDistance);
        tauntState.Initialize(this, agent, player, tauntSpeed, tauntWobbleStrength, tauntStopDistance);
        disappearState.Initialize(this, agent, player, disappearSpeed, disappearDistance, disableGameObject);
        
        if (rewardIcon != null)
            rewardIcon.SetActive(true);
            
        StartHiding();
    }
    
    void Update()
    {
        if (isInDialogue) return;
        
        timeSinceLastFound += Time.deltaTime;
        
        switch (currentState)
        {
            case NPCState.Hiding:
                hideState.UpdateState();
                CheckTransitionsFromHide();
                break;
            case NPCState.Running:
                runState.UpdateState();
                CheckTransitionsFromRun();
                break;
            case NPCState.Taunting:
                tauntState.UpdateState();
                CheckTransitionsFromTaunt();
                break;
            case NPCState.Disappearing:
                disappearState.UpdateState();
                break;
        }
    }
    
    void CheckTransitionsFromHide()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer < detectionRange)
        {
            StartRunning();
        }
        else if (timeSinceLastFound > tauntingDelay)
        {
            StartTaunting();
        }
    }
    
    void CheckTransitionsFromRun()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > safeDistance)
        {
            StartHiding();
        }
    }
    
    void CheckTransitionsFromTaunt()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer < detectionRange / 2)
        {
            StartRunning();
        }
    }
    
    public void StartHiding()
    {
        currentState = NPCState.Hiding;
        timeSinceLastFound = 0f;
        hideState.EnterState();
    }
    
    public void StartRunning()
    {
        currentState = NPCState.Running;
        timeSinceLastFound = 0f;
        runState.EnterState();
    }
    
    public void StartTaunting()
    {
        currentState = NPCState.Taunting;
        tauntState.EnterState();
    }
    
    public void StartDisappearing()
    {
        currentState = NPCState.Disappearing;
        disappearState.EnterState();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInDialogue && !hasGivenItem)
        {
            StartDialogueWithPlayer();
        }
    }
    
    void StartDialogueWithPlayer()
    {
        isInDialogue = true;
        agent.isStopped = true;
        
        if (dialogueAsset != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Хранитель артефакта", dialogueAsset.RootNode);
            StartCoroutine(WaitForDialogueEnd());
        }
        else
        {
            GiveQuestItem();
            Invoke(nameof(EndDialogue), 1f);
        }
    }
    
    IEnumerator WaitForDialogueEnd()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        
        if (!hasGivenItem)
        {
            GiveQuestItem();
        }
        
        EndDialogue();
    }
    
    void GiveQuestItem()
    {
        if (hasGivenItem) return;
        
        hasGivenItem = true;
        
        if (rewardIcon != null)
            rewardIcon.SetActive(false);
        
        if (droppedItemPrefab != null && questItem != null)
        {
            Vector3 spawnPosition = player.position + new Vector3(0, 0, itemSpawnOffsetZ);
                
            var droppedItem = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity)
                .GetComponent<DroppedItem>();
                
            if (droppedItem != null)
            {
                droppedItem.Initialize(questItem);
            }
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop");
        }
    }
    
    void EndDialogue()
    {
        isInDialogue = false;
        
        if (hasGivenItem)
        {
            StartDisappearing();
        }
        else
        {
            StartRunning();
        }
    }
    
    public bool HasGivenItem() => hasGivenItem;
    public Transform GetPlayer() => player;
}
