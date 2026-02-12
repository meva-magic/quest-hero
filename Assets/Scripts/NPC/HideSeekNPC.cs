using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum NPCState
{
    Hiding,
    Running,
    Taunting
}

public class HideSeekNPC : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    private Transform player;
    [SerializeField] private NPCState currentState = NPCState.Hiding;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float tauntingDelay = 30f;
    [SerializeField] private Dialogue dialogueAsset;
    [SerializeField] private GameObject rewardIcon;
    private float timeSinceLastFound;
    [SerializeField] private ItemObject questItem;
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private Transform itemDropPoint;
    private bool hasGivenItem = false;
    private bool isInDialogue = false;
    Vector3 spawnPosition;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (rewardIcon != null)
            rewardIcon.SetActive(true);
            
        StartHiding();
    }

    void Update()
    {
        if (isInDialogue) return;
        
        switch (currentState)
        {
            case NPCState.Hiding:
                UpdateHiding();
                break;
            case NPCState.Running:
                UpdateRunning();
                break;
            case NPCState.Taunting:
                UpdateTaunting();
                break;
        }
        
        timeSinceLastFound += Time.deltaTime;
        if (timeSinceLastFound > tauntingDelay && currentState == NPCState.Hiding)
        {
            StartTaunting();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInDialogue && !hasGivenItem)
        {
            StartDialogueWithPlayer();
        }
    }
    
    void UpdateHiding()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (RandomPoint(transform.position, 10f, out Vector3 point))
            {
                agent.SetDestination(point);
            }
        }
        
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            StartRunning();
        }
    }

    void UpdateRunning()
    {
        Vector3 runDirection = (transform.position - player.position).normalized;
        Vector3 runPoint = transform.position + runDirection * 15f;
        
        if (Vector3.Distance(transform.position, player.position) > detectionRange * 1.5f)
        {
            StartHiding();
        }
        else
        {
            float distanceToRunPoint = Vector3.Distance(agent.destination, player.position);
            if (distanceToRunPoint < detectionRange)
            {
                agent.SetDestination(runPoint);
            }
        }
    }

    void UpdateTaunting()
    {
        if (Vector3.Distance(transform.position, player.position) < detectionRange / 2)
        {
            StartRunning();
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    void StartHiding()
    {
        currentState = NPCState.Hiding;
        timeSinceLastFound = 0f;
        agent.isStopped = false;
        
        if (RandomPoint(transform.position, 10f, out Vector3 point))
        {
            agent.SetDestination(point);
        }
    }

    void StartRunning()
    {
        currentState = NPCState.Running;
        timeSinceLastFound = 0f;
        agent.isStopped = false;
        
        Vector3 runDirection = (transform.position - player.position).normalized;
        Vector3 runPoint = transform.position + runDirection * 15f;
        agent.SetDestination(runPoint);
    }

    void StartTaunting()
    {
        currentState = NPCState.Taunting;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }
    
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
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
            spawnPosition = player.transform.position + new Vector3(0, 0, -2f);
            var droppedItem = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity).GetComponent<DroppedItem>();
                
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
        StartRunning();
    }
    
    public bool HasGivenItem() => hasGivenItem;
}
