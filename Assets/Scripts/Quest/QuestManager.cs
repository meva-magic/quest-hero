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

    private void Awake()
    {
        instance = this;
        goalAchieved = false;
        
        if (questUIParent != null)
            questUIParent.SetActive(false);
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
            return;
        }

        if (Inventory.instance != null)
        {
            bool removed = Inventory.instance.RemoveItemWithoutDrop(currentQuest.questItemID);
        }
        
        if (currentQuest.rewardPrefab != null)
        {
            GiveReward(currentQuest.rewardPrefab);
        }
        
        if (questUIParent != null)
            questUIParent.SetActive(false);
            
        currentQuest = null;
        goalAchieved = false;
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
                    // You might need to set the item from the QuestNode
                    // This assumes your reward prefab already has the item set
                }
            }
            else
            {
                droppedItem = rewardInstance.AddComponent<DroppedItem>();
                droppedItem.autoStart = true;
            }
            
            AudioManager.instance.Play("Reward");
        }
        
        catch (System.Exception e) { }
    }
}
