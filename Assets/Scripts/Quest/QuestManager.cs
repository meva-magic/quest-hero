using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    public bool goalAchieved;
    public QuestNode currentQuest;
    
    public GameObject questUIParent;
    public Text questNameText;
    public Text questDescriptionText;
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
            
        Debug.Log($"Quest activated: {quest.questName}");
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
            Debug.LogWarning("No active quest to finish");
            return;
        }

        // Check if goal is achieved
        if (!CheckGoal())
        {
            Debug.Log("Quest cannot be finished - goal not achieved");
            return;
        }

        Debug.Log($"Finishing quest: {currentQuest.questName}");
        
        // Remove quest item from inventory (without dropping it)
        if (Inventory.instance != null)
        {
            bool removed = Inventory.instance.RemoveItemWithoutDrop(currentQuest.questItemID);
            Debug.Log($"Quest item removed: {removed}");
        }
        
        // Give reward
        if (currentQuest.rewardPrefab != null)
        {
            Debug.Log($"Spawning reward: {currentQuest.rewardPrefab.name}");
            GiveReward(currentQuest.rewardPrefab);
        }
        else
        {
            Debug.LogWarning("No reward prefab set for this quest!");
        }
        
        // Hide quest UI
        if (questUIParent != null)
            questUIParent.SetActive(false);
            
        // Reset current quest
        currentQuest = null;
        goalAchieved = false;
        
        Debug.Log("Quest finished successfully");
    }

    public void GiveReward(GameObject rewardPrefab)
    {
        if (rewardPrefab == null) 
        {
            Debug.LogError("Reward prefab is null!");
            return;
        }
        
        try
        {
            // Find player position to spawn near player
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
            
            Debug.Log($"Spawning reward at position: {spawnPosition}");
            
            // Instantiate the reward prefab
            GameObject rewardInstance = Instantiate(rewardPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Reward instantiated: {rewardInstance.name}");
            
            // Check if it has DroppedItem component
            DroppedItem droppedItem = rewardInstance.GetComponent<DroppedItem>();
            if (droppedItem != null)
            {
                Debug.Log($"Reward has DroppedItem component: {droppedItem.item?.name}");
                
                // If autoStart is false, initialize it
                if (!droppedItem.autoStart && droppedItem.item == null)
                {
                    // You might need to set the item from the QuestNode
                    // This assumes your reward prefab already has the item set
                }
            }
            else
            {
                Debug.LogWarning("Reward prefab doesn't have DroppedItem component! Adding one...");
                // Add DroppedItem component if missing
                droppedItem = rewardInstance.AddComponent<DroppedItem>();
                droppedItem.autoStart = true;
                // You might need to set the item here
            }
            
            if (AudioManager.instance != null)
                AudioManager.instance.Play("Reward");
                
            Debug.Log("Reward successfully spawned!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning reward: {e.Message}");
        }
    }
}
