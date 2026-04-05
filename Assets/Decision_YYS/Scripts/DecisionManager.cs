using UnityEngine;
using TMPro;
using static Constants;

public class DecisionManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    private IJsonSerializer jsonManager;
    ScenarioData scenarioData;

    private void Start()
    {
        jsonManager = new JsonManager();
        scenarioData = jsonManager.LoadData<ScenarioData>("TempStory");

        if(scenarioData.Chapter == (int)chapter.ÀÚ±Ø)
        {
            text.text = scenarioData.Story[0].text;
            Debug.Log($"Text È®ÀÎ: {text.text}");
        }

    }
}
