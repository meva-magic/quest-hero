using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public bool goalAchieved;
    public QuestNode currentQuest;
    
    public GameObject questUIParent;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDescriptionText;
    public Image questIconImage;
    
    // Событие для обновления состояния квеста
    public delegate void QuestUpdateHandler();
    public event QuestUpdateHandler OnQuestUpdated;

    private void Awake()
    {
        instance = this;
        goalAchieved = false;
        
        if (questUIParent != null)
            questUIParent.SetActive(false);
    }
    
    private void OnEnable()
    {
        // Подписываемся на события изменения инвентаря
        if (Inventory.instance != null)
        {
            // В реальной реализации нужно добавить событие в Inventory.cs
            // и подписаться на него здесь
        }
    }

    public void ActivateQuest(QuestNode quest)
    {
        currentQuest = quest;
        goalAchieved = false;
        
        if (questUIParent != null)
            questUIParent.SetActive(true);
        
        if (questNameText != null)
            questNameText.text = quest.questName;
            
        if (questDescriptionText != null)
            questDescriptionText.text = quest.questDesctiption;
            
        if (questIconImage != null && quest.questIcon != null)
            questIconImage.sprite = quest.questIcon;
            
        // Вызываем событие обновления
        OnQuestUpdated?.Invoke();
    }

    public bool CheckGoal()
    {
        if (currentQuest == null)
        {
            goalAchieved = false;
            return false;
        }

        if (Inventory.instance != null)
        {
            goalAchieved = Inventory.instance.HasItem(currentQuest.questItemID);
        }
        else
        {
            goalAchieved = false;
        }
        
        return goalAchieved;
    }

    public void FinishQuest()
    {
        if (currentQuest == null)
        {
            return;
        }

        if (!CheckGoal())
        {
            Debug.LogWarning("Нельзя завершить квест - цель не достигнута");
            return;
        }

        if (Inventory.instance != null)
        {
            bool removed = Inventory.instance.RemoveItemWithoutDrop(currentQuest.questItemID);
            if (!removed)
            {
                Debug.LogWarning("Не удалось удалить квестовый предмет из инвентаря");
                return;
            }
        }
        
        if (currentQuest.rewardPrefab != null)
        {
            GiveReward(currentQuest.rewardPrefab);
        }
        
        if (questUIParent != null)
            questUIParent.SetActive(false);
            
        currentQuest = null;
        goalAchieved = false;
        
        // Вызываем событие обновления
        OnQuestUpdated?.Invoke();
        
        Debug.Log("Квест успешно завершен!");
    }

    public void GiveReward(GameObject rewardPrefab)
    {
        if (rewardPrefab == null) 
        {
            return;
        }
        
        try
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPosition;
            
            if (player != null)
            {
                spawnPosition = player.transform.position + player.transform.forward * 2f + Vector3.up;
            }
            else
            {
                spawnPosition = transform.position + Vector3.forward * 2f;
            }
            
            GameObject rewardInstance = Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
            
            DroppedItem droppedItem = rewardInstance.GetComponent<DroppedItem>();
         
            if (droppedItem != null)
            {
                if (!droppedItem.autoStart && droppedItem.item == null)
                {
                    // Если у награды есть компонент DroppedItem, но он не настроен,
                    // можно попытаться настроить его из QuestNode
                    // В данном случае просто отмечаем как автостарт
                    droppedItem.autoStart = true;
                }
            }
            else
            {
                // Добавляем компонент DroppedItem если его нет
                droppedItem = rewardInstance.AddComponent<DroppedItem>();
                droppedItem.autoStart = true;
            }
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Reward");
        }
        catch (System.Exception e) 
        { 
            Debug.LogError($"Ошибка при выдаче награды: {e.Message}");
        }
    }
    
    // Метод для проверки и обновления состояния квеста
    public void UpdateQuestStatus()
    {
        if (currentQuest != null)
        {
            CheckGoal();
        }
    }
}
