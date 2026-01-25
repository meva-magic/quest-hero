using UnityEngine;
 
public class Speaker : MonoBehaviour
{
    public string Name;
    public Dialogue Dialogue;
 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpeakTo();
        }
    }
 
    // Trigger dialogue for this actor
    public void SpeakTo()
    {
        DialogueManager.Instance.StartDialogue(Name, Dialogue.RootNode);
    }
}