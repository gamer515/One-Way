using System;
using System.Collections.Generic;

[Serializable]
public class ScenarioData
{
    public List<Dialogue> MainStory;
    public List<Dialogue> SideStory;
}

[Serializable]
public class Dialogue
{
    public int id;
    public string text;
    public string type;
    public string[] options;
    public int[] Figure;
    public bool isTransition;
    public string backgroundName;
}
