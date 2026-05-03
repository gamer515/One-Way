using System;
using System.Collections.Generic;

[Serializable]
public class OmnibusData
{
    public List<MainStoryInfo> MainStories;
    public List<SideStoryInfo> SideStories;
}

[Serializable]
public class MainStoryInfo
{
    public string Chapter;
    public List<string> Title;
}

[Serializable]
public class SideStoryInfo
{
    public List<string> Title;
}
