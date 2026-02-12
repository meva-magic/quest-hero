using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Node", menuName = "Quest/Quest Node")]
public class QuestNode : ScriptableObject
{
    public string questName;

    [TextArea(3, 10)]
    public string questDesctiption;

    public Sprite questIcon;

    public string questItemID;

    public GameObject rewardPrefab;
}
