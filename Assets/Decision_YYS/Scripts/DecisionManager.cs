using UnityEngine;
using TMPro;
using static Constants;
using System.Collections;
using UnityEngine.UI;

public enum GameState { ShowingStory, WaitingForChoice, Transitioning }

public class DecisionManager : MonoBehaviour
{
    [SerializeField] private StatContainer statContainer;

    [SerializeField] private RectTransform cardFront;
    [SerializeField] private RectTransform cardBack;

    [SerializeField] private TextMeshProUGUI front_Dialogue_Text;
    [SerializeField] private TextMeshProUGUI back_Dialogue_Text;
    [SerializeField] private TextMeshProUGUI option_Text;

    private int story_Index = 0;
    private GameState currentState;

    private IJsonSerializer jsonManager;
    ScenarioData scenarioData;

    private void Awake()
    {
        currentState = GameState.ShowingStory;
    }

    private void Start()
    {
        jsonManager = new JsonManager();
        scenarioData = jsonManager.LoadData<ScenarioData>("TempStory");

        if(scenarioData.Chapter == (int)chapter.자극)
        {
            front_Dialogue_Text.text = scenarioData.Story[0].text;
            back_Dialogue_Text.text = "";

            option_Text.gameObject.SetActive(false);
            Debug.Log($"Text 확인: {front_Dialogue_Text.text}");
        }
    }

    private void DisplayCurrentStory()
    {
        var currentStory = scenarioData.Story[story_Index];
        front_Dialogue_Text.text = currentStory.text;

        currentState = GameState.ShowingStory;
        option_Text.gameObject.SetActive(false);
    }

    public void OnScreenClicked()
    {
        if (currentState == GameState.Transitioning) return;

        var currentStory = scenarioData.Story[story_Index];

        if (currentState == GameState.ShowingStory)
        {
            if (currentStory.type == "Choice")
            {
                currentState = GameState.WaitingForChoice;
                option_Text.gameObject.SetActive(true);

                option_Text.text = "선택지를 선택하세요.";
            }
            else
            {
                ProceedToNextStory();
            }
        }
    }

    public void ConfirmChoice(int gear)
    {
        if (currentState != GameState.WaitingForChoice) return;

        var currentStory = scenarioData.Story[story_Index];
        int optionIndex = GetOptionIndexFromGear(gear);
        
        if (optionIndex >= 0 && optionIndex < currentStory.Figure.Length)
        {
            statContainer.stats[optionIndex] += currentStory.Figure[optionIndex];
            statContainer.UpdateStat(optionIndex);

            Debug.Log($"[{currentStory.options[optionIndex]}] 선택됨! 스탯 변경 적용.");
        }

        ProceedToNextStory();
    }

    private void ProceedToNextStory()
    {
        story_Index++;  
        if (story_Index < scenarioData.Story.Count)
        {
            var nextStory = scenarioData.Story[story_Index];
            option_Text.gameObject.SetActive(false);

            if (nextStory.isTransition)
            {
                currentState = GameState.Transitioning;
                StartCoroutine(SwipeTransition(nextStory));
            }
            else
            {
                front_Dialogue_Text.text = nextStory.text;
                back_Dialogue_Text.text = "";
                currentState = GameState.ShowingStory;
            }
        }
        else
        {
            Debug.Log("스토리의 끝에 도달했습니다.");
        }
    }

    public void ShowOptionText(int gear)
    {
        if(story_Index >= scenarioData.Story.Count) return;
        if (scenarioData.Story[story_Index].type != "Choice") return;

        int index = GetOptionIndexFromGear(gear);
        if (index >= 0 && index < scenarioData.Story[story_Index].options.Length)
        {
            option_Text.text = scenarioData.Story[story_Index].options[index];
        }
    }

    private int GetOptionIndexFromGear(int gear)
    {
        if (gear == (int)Constants.gear.악찬) return 0;
        if (gear == (int)Constants.gear.악반) return 1;
        if (gear == (int)Constants.gear.선찬) return 2;
        if (gear == (int)Constants.gear.선반) return 3;
        return -1;
    }

    private IEnumerator SwipeTransition(DialogueData nextStory)
    {
        back_Dialogue_Text.text = nextStory.text;
        string bgData = nextStory.backgroundName;

        if (!string.IsNullOrEmpty(bgData) && bgData.ToLower() != "none")
        {
            Image dgImg = cardBack.GetComponent<Image>();
            Color customColor;

            if (ColorUtility.TryParseHtmlString(bgData, out customColor))
            {
                Debug.Log($"Color detected: {bgData}, applying color: {customColor}");
                dgImg.sprite = null;
                dgImg.color = customColor;
            }
            else
            {
                Debug.Log($"Not a color, trying to load resource: {bgData}");
                Sprite loadedSprite = Resources.Load<Sprite>(bgData);
                if (loadedSprite != null)
                {
                    dgImg.sprite = loadedSprite;
                    dgImg.color = Color.white;
                }
            }
        }

        float duration = 0.5f;
        float elasped = 0f;
        Vector2 startPos = cardFront.anchoredPosition;
        Quaternion startRot = cardFront.localRotation;

        Vector2 targetPos = startPos + new Vector2(-1000f, -200f);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, 30f);

        while (elasped < duration)
        {
            elasped += Time.deltaTime;
            float t = Mathf.Clamp01(elasped / duration);

            cardFront.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            cardFront.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        front_Dialogue_Text.text = nextStory.text;

        Image frontImg = cardFront.GetComponent<Image>();
        Image backImg = cardBack.GetComponent<Image>();

        if(frontImg != null && backImg != null)
        {
            frontImg.sprite = backImg.sprite;
            frontImg.color = backImg.color;
        }

        cardFront.anchoredPosition = startPos;
        cardFront.localRotation = startRot;

        currentState = GameState.ShowingStory;
    }
}
