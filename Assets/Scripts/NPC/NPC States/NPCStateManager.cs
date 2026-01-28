using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPCState
{
    Hiding,          // Прячется
    Running,         // Убегает от игрока
    Teasing,         // Дразнит игрока
    Transition       // Переход с диалогом (короткое состояние)
}

public class HideSeekNPC : MonoBehaviour
{
    // Компоненты
    private NavMeshAgent agent;
    private Transform player;
    
    // Настройки
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float teasingDelay = 30f;
    [SerializeField] private float dialogueDuration = 2f;
    
    // Таймеры
    private float timeSinceLastFound;
    private float dialogueTimer;
    
    // Текущее состояние
    private NPCState currentState;
    private Vector3 lastDestination;
    
    // Диалоги по состояниям
    [SerializeField] private string[] hidingDialogue;
    [SerializeField] private string[] runningDialogue;
    [SerializeField] private string[] teasingDialogue;
    [SerializeField] private string[] transitionDialogue;
    
    // Ссылка на UI
    [SerializeField] private DialoguePanel dialoguePanel;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartHiding();
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCState.Hiding:
                UpdateHiding();
                break;
            case NPCState.Running:
                UpdateRunning();
                break;
            case NPCState.Teasing:
                UpdateTeasing();
                break;
            case NPCState.Transition:
                UpdateTransition();
                break;
        }
        
        // Обновляем таймер для перехода в дразнение
        timeSinceLastFound += Time.deltaTime;
        if (timeSinceLastFound > teasingDelay && currentState == NPCState.Hiding)
        {
            StartTeasing();
        }
    }

    void UpdateHiding()
    {
        // Движение к случайной точке
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (RandomPoint(transform.position, 10f, out Vector3 point))
            {
                lastDestination = point;
                StartTransition(hidingDialogue, point);
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
        else if (Vector3.Distance(agent.destination, player.position) < detectionRange)
        {
            // Меняем направление если игрок близко
            StartTransition(runningDialogue, runPoint);
        }
    }

    void UpdateTeasing()
    {
        // Приближение к игроку для дразнения
        if (Vector3.Distance(transform.position, player.position) < detectionRange / 2)
        {
            StartTransition(teasingDialogue, transform.position);
            StartRunning(); // После дразнения убегаем
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    void UpdateTransition()
    {
        // Ожидание окончания диалога
        dialogueTimer -= Time.deltaTime;
        if (dialogueTimer <= 0)
        {
            dialoguePanel.Hide();
            // Возвращаемся в предыдущее состояние
            agent.SetDestination(lastDestination);
            currentState = GetPreviousState();
        }
    }

    // Методы переключения состояний
    void StartHiding()
    {
        currentState = NPCState.Hiding;
        timeSinceLastFound = 0f;
    }

    void StartRunning()
    {
        currentState = NPCState.Running;
        timeSinceLastFound = 0f;
        ShowRandomDialogue(runningDialogue);
    }

    void StartTeasing()
    {
        currentState = NPCState.Teasing;
        ShowRandomDialogue(teasingDialogue);
    }

    void StartTransition(string[] dialogueArray, Vector3 nextDestination)
    {
        currentState = NPCState.Transition;
        lastDestination = nextDestination;
        dialogueTimer = dialogueDuration;
        ShowRandomDialogue(dialogueArray);
    }

    // Вспомогательные методы
    void ShowRandomDialogue(string[] dialogueArray)
    {
        if (dialogueArray.Length > 0)
        {
            string randomLine = dialogueArray[Random.Range(0, dialogueArray.Length)];
            dialoguePanel.Show(randomLine, dialogueDuration);
        }
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

    NPCState GetPreviousState()
    {
        // Логика определения предыдущего состояния
        // Можно добавить стэк состояний для точного отслеживания
        return NPCState.Hiding;
    }
}
