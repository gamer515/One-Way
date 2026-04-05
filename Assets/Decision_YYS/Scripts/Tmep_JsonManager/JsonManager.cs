using UnityEngine;
using System.IO;

public class JsonManager : IJsonSerializer
{
    public T LoadData<T>(string fileName)
    {
        //프로젝트에서 파일 경로 정리를 한 번 해야 할 듯.
        string path = Path.Combine(Application.dataPath, "Decision_YYS", "Story_Json_Data", fileName + ".json");
        if(!File.Exists(path))
        {
            Debug.LogError($"File not found at path: {path}");
            return default;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    public void SaveData<T>(T data, string fileName)
    {
        //추후 빌드를 할 경우, 이 방식은 읽기 파일에 접근이 불가하므로 resources 폴더에 저장하는 방식으로 변경해야할듯.
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.dataPath, "Decision_YYS", "Story_Json_Data", fileName + ".json");
        File.WriteAllText(path, json);
    }
}
