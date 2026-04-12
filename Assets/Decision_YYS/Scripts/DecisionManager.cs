using UnityEngine;
using TMPro;
using static Constants;

public class DecisionManager : MonoBehaviour
{
    [SerializeField] private StatContainer statContainer;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI option_Text;

    private int story_Index = 0;

    private IJsonSerializer jsonManager;
    ScenarioData scenarioData;

    private void Start()
    {
        jsonManager = new JsonManager();
        scenarioData = jsonManager.LoadData<ScenarioData>("TempStory");

        if(scenarioData.Chapter == (int)chapter.자극)
        {
            text.text = scenarioData.Story[0].text;
            Debug.Log($"Text 확인: {text.text}");
        }
    }

    public void ChooseOption(int gear)
    {
        if(story_Index < scenarioData.Story.Count)
        {
            story_Index++;
            // 스토리 인덱스가 유효한 범위 내에 있는지 확인.
            text.text = scenarioData.Story[story_Index].text;
            Debug.Log($"{story_Index} 업데이트");
        }
        else
        {
            Debug.Log("스토리의 끝에 도달했습니다.");
            return;
        }

        if (scenarioData.Story[story_Index].type == "Next")
        {
            Debug.Log("Next 타입 선택됨");
        }
        else if (scenarioData.Story[story_Index].type == "Choice")
        {
            Debug.Log("Choice 타입 선택됨");
            string option = "";
            if(gear == (int)Constants.gear.악찬)
            {
                option = scenarioData.Story[story_Index].options[0];
                Debug.Log($"악찬 선택됨: {option}");
            }
            else if(gear == (int)Constants.gear.악반)
            {
                    option = scenarioData.Story[story_Index].options[1];
                Debug.Log($"악반 선택됨: {option}");
            }
            else if(gear == (int)Constants.gear.선찬)
            {
                    option = scenarioData.Story[story_Index].options[2];
                Debug.Log($"선찬 선택됨: {option}");
            }
            else if(gear == (int)Constants.gear.선반)
            {
                    option = scenarioData.Story[story_Index].options[3];
                Debug.Log($"선반 선택됨: {option}");
            }
        }

    }

    // 특정 함수는 다른 클래스에 위치하도록 하자.
    public void ShowOptionText(int gear)
    {
        if(story_Index >= scenarioData.Story.Count)
        {
            Debug.Log("스토리의 끝에 도달했습니다. 옵션 텍스트를 표시할 수 없습니다.");
            return;
        }

        if (scenarioData.Story[story_Index].type != "Choice") return;

        if (gear == (int)Constants.gear.악찬)
        {
            option_Text.text = scenarioData.Story[story_Index].options[0];
        }
        else if (gear == (int)Constants.gear.악반)
        {
            option_Text.text = scenarioData.Story[story_Index].options[1];
        }
        else if (gear == (int)Constants.gear.선찬)
        {
            option_Text.text = scenarioData.Story[story_Index].options[2];
        }
        else if (gear == (int)Constants.gear.선반)
        {
            option_Text.text = scenarioData.Story[story_Index].options[3];
        }
    }

    // 스탯 접근 제한을 생각해야 함.
    public void SetStat(int index)
    {
        if (scenarioData.Story[story_Index].type == "Next") return;

        index -= 1; // 0-based index로 변환
        statContainer.stats[index] += scenarioData.Story[story_Index].Figure[index];
        statContainer.UpdateStat(index);
    }
}
