using UnityEngine;

public class NPCBaseZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HideSeekNPC targetNPC; // NPC, который активируется
    [SerializeField] private Transform basePoint; // Точка базы (если не задана, используется позиция этого объекта)
    
    [Header("Settings")]
    [SerializeField] private float baseReturnDistance = 30f; // Дистанция возврата к базе
    [SerializeField] private float playerDistanceCheck = 20f; // Дистанция до игрока для возврата
    [SerializeField] private bool showGizmos = true;
    
    private Transform player;
    
    void Start()
    {
        // Находим игрока
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Если не назначен, ищем компонент на сцене
        if (targetNPC == null)
            targetNPC = FindObjectOfType<HideSeekNPC>();
        
        // Если база не задана, используем позицию этого объекта
        if (basePoint == null)
            basePoint = transform;
    }
    
    void Update()
    {
        if (targetNPC == null || player == null) return;
        
        // Получаем расстояния
        float distToBase = Vector3.Distance(targetNPC.transform.position, basePoint.position);
        float distToPlayer = Vector3.Distance(targetNPC.transform.position, player.position);
        
        // Передаем значения в NPC
        targetNPC.SetBaseReturnConditions(distToBase, distToPlayer, baseReturnDistance, playerDistanceCheck);
    }
    
    // Для визуализации в редакторе
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Рисуем радиус возврата к базе
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(basePoint != null ? basePoint.position : transform.position, baseReturnDistance);
        
        // Рисуем радиус проверки игрока
        Gizmos.color = Color.cyan;
        if (player != null)
            Gizmos.DrawWireSphere(player.position, playerDistanceCheck);
        else
            Gizmos.DrawWireSphere(transform.position, playerDistanceCheck);
        
        // Линия от NPC до базы
        if (targetNPC != null && basePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(targetNPC.transform.position, basePoint.position);
        }
    }
}
