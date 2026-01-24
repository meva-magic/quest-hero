using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewNPCDialogue", menuName = "NPC Dialogue")]

public class NPCScriptableObject : ScriptableObject
{
    public string nametag;
    public Sprite portrait;
    public AudioClip voice;

    //[SerializeField] private float voicePitch = 1f;
    
    public string[] dialogueLines;
    public bool[] skipLines;
    public float skipDelay = 1.5f;

    public float typingSpeed = 0.05f;    
}
