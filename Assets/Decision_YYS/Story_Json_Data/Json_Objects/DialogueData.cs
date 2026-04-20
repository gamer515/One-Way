using System;

[Serializable]
public class DialogueData
{
    public int id;
    public string text;
    public string type;
    public string[] options;
    public int[] Figure;
    public bool isTransition;
    public string backgroundName;
}
