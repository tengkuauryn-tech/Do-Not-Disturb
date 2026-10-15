using System.Collections.Generic;

[System.Serializable]
public class DialogueNode
{
    public int id;
    public string sceneID;
    public string nodeType;
    public string speaker;
    public string dialogueText;
    public string triggerAction;
    public string sfx;
    public string nextSceneID;

    // Untuk Pilihan (Choices)
    public List<string> choices = new List<string>();
    public List<string> targetScenes = new List<string>();
}