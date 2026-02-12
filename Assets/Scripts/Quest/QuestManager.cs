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
    
    [SerializeField] private GameObject wallToDisable;
    
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
            
        DisableWall();
            
        OnQuestUpdated?.Invoke();
    }

    private void DisableWall()
    {
        if (wallToDisable != null)
        {
            wallToDisable.SetActive(false);
        }
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
        if (currentQuest == null) return;

        if (!CheckGoal()) return;

        if (Inventory.instance != null)
        {
            bool removed = Inventory.instance.RemoveItemWithoutDrop(currentQuest.questItemID);
            if (!removed) return;
        }
        
        if (currentQuest.rewardPrefab != null)
        {
            GiveReward(currentQuest.rewardPrefab);
        }
        
        if (questUIParent != null)
            questUIParent.SetActive(false);
            
        currentQuest = null;
        goalAchieved = false;
        
        OnQuestUpdated?.Invoke();
    }

    public void GiveReward(GameObject rewardPrefab)
    {
        if (rewardPrefab == null) return;
        
        try
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 spawnPosition;

            if (player != null)
            {
                spawnPosition = player.transform.position + new Vector3(0, 0, -2f);
            }
            else
            {
                spawnPosition = transform.position + new Vector3(0, 0, -2f);
            }

            GameObject rewardInstance = Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
            
            DroppedItem droppedItem = rewardInstance.GetComponent<DroppedItem>();
         
            if (droppedItem != null)
            {
                if (!droppedItem.autoStart && droppedItem.item == null)
                {
                    droppedItem.autoStart = true;
                }
            }
            else
            {
                droppedItem = rewardInstance.AddComponent<DroppedItem>();
                droppedItem.autoStart = true;
            }
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Reward");
        }
        catch
        {
        }
    }
    
    public void UpdateQuestStatus()
    {
        if (currentQuest != null)
        {
            CheckGoal();
        }
    }
}
