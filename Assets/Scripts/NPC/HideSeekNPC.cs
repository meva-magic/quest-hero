using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public enum NPCState
{
    Hiding,          // Прячется
    Running,         // Убегает от игрока
    Taunting         // Подходит поближе
}

public class HideSeekNPC : MonoBehaviour
{
    // Компоненты
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    private Transform player;
    
    // Состояния
    [Header("States")]
    [SerializeField] private NPCState currentState = NPCState.Hiding;
    
    // Настройки
    [Header("Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float tauntingDelay = 30f;
    
    // Диалог
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogueAsset;
    
    // UI для награды
    [Header("UI")]
    [SerializeField] private GameObject rewardIcon; // UI Image (активен пока есть награда)
    
    // Таймеры
    private float timeSinceLastFound;
    
    // Квестовый предмет
    [Header("Quest")]
    [SerializeField] private ItemObject questItem;
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private Transform itemDropPoint;
    
    // Флаги
    private bool hasGivenItem = false;
    private bool isInDialogue = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Активируем UI награды по умолчанию
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
        
        // Обновляем таймер для перехода в таунтинг
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
        // Движение к случайной точке
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (RandomPoint(transform.position, 10f, out Vector3 point))
            {
                agent.SetDestination(point);
            }
        }
        
        // Проверка на обнаружение игроком
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
        {
            StartRunning();
        }
    }

    void UpdateRunning()
    {
        // Бег от игрока
        Vector3 runDirection = (transform.position - player.position).normalized;
        Vector3 runPoint = transform.position + runDirection * 15f;
        
        if (Vector3.Distance(transform.position, player.position) > detectionRange * 1.5f)
        {
            StartHiding();
        }
        else
        {
            // Проверяем, нужно ли сменить точку бегства
            float distanceToRunPoint = Vector3.Distance(agent.destination, player.position);
            if (distanceToRunPoint < detectionRange)
            {
                agent.SetDestination(runPoint);
            }
        }
    }

    void UpdateTaunting()
    {
        // Приближение к игроку для дразнения
        if (Vector3.Distance(transform.position, player.position) < detectionRange / 2)
        {
            StartRunning(); // После приближения убегаем
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    // Методы переключения состояний
    void StartHiding()
    {
        currentState = NPCState.Hiding;
        timeSinceLastFound = 0f;
        agent.isStopped = false;
        
        // Ищем новую точку
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
        
        // Бежим от игрока
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
    
    // Вспомогательные методы
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
    
    // Диалог с игроком
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
            // Если диалог не настроен, просто выдаем предмет
            GiveQuestItem();
            Invoke(nameof(EndDialogue), 1f);
        }
    }
    
    IEnumerator WaitForDialogueEnd()
    {
        yield return new WaitWhile(() => DialogueManager.Instance.IsDialogueActive());
        
        // После диалога выдаем предмет
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
        
        // Скрываем UI награды
        if (rewardIcon != null)
            rewardIcon.SetActive(false);
        
        // Создаем выброшенный предмет
        if (droppedItemPrefab != null && questItem != null)
        {
            var droppedItem = Instantiate(droppedItemPrefab, itemDropPoint.position + Vector3.up, Quaternion.identity).GetComponent<DroppedItem>();
                
            if (droppedItem != null)
            {
                droppedItem.Initialize(questItem);
            }
            
            // Воспроизводим звук
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Drop");
        }
    }
    
    void EndDialogue()
    {
        isInDialogue = false;
        StartRunning(); // После диалога убегаем
    }
    
    public bool HasGivenItem() => hasGivenItem;
    
    // Для отладки в редакторе
    void OnDrawGizmosSelected()
    {
        // Отображаем радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Отображаем текущее состояние
        switch (currentState)
        {
            case NPCState.Hiding:
                Gizmos.color = Color.green;
                break;
            case NPCState.Running:
                Gizmos.color = Color.red;
                break;
            case NPCState.Taunting:
                Gizmos.color = Color.magenta;
                break;
        }
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
