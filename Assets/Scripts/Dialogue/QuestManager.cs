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
            
        Debug.Log($"Quest activated: {quest.questName}");
    }

    public bool CheckGoal()
    {
        if (currentQuest == null)
        {
            goalAchieved = false;
            return false;
        }

        // Use the public HasItem method
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

        // Give reward if available
        if (currentQuest.rewardPrefab != null)
        {
            GiveReward(currentQuest.rewardPrefab);
        }
        else
        {
            Debug.Log("No reward prefab set for this quest");
        }
        
        // Remove quest item from inventory using public method
        if (Inventory.instance != null)
        {
            Inventory.instance.RemoveItem(currentQuest.questItemID);
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
        if (rewardPrefab == null) return;
        
        Instantiate(rewardPrefab, transform.position, Quaternion.identity);

        if (AudioManager.instance != null)
            AudioManager.instance.Play("Reward");
            
        Debug.Log("Reward given to player");
    }
}
