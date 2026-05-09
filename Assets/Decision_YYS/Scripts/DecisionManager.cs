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

    private OmnibusData currentOmnibus;
    private int currentChapterIndex = 0;
    private int currentEpisodeIndex = 0;

    [SerializeField] private JoystickLikeGear gearController;

    private ISaveSystem saveManager;

    private void Awake()
    {
        currentState = GameState.ShowingStory;
        saveManager = new SaveManager();
    }

    private void OnEnable()
    {
        if (statContainer != null)
        {
            statContainer.OnTargetStatReached += HandleTargetStatReached;
        }
    }

    private void OnDisable()
    {
        if (statContainer != null)
        {
            statContainer.OnTargetStatReached -= HandleTargetStatReached;
        }
    }

    private void HandleTargetStatReached()
    {
        // Initial 챕터(인덱스 0)일 때는 무시합니다.
        if (currentChapterIndex == 0) return;

        Debug.Log("전투 씬 진입 (2초간 시뮬레이션)");
        currentState = GameState.Transitioning;
        StartCoroutine(SimulateCombatAndResume());
    }

    private IEnumerator SimulateCombatAndResume()
    {
        // 2초 동안 대기 (전투 씬 시뮬레이션)
        yield return new WaitForSeconds(2.0f);

        Debug.Log("전투 종료! 현재 에피소드를 중단하고 다음 챕터로 즉시 이동합니다.");

        // 현재 진행 중인 에피소드를 강제로 종료하고 다음 챕터로 인덱스 변경
        if (currentOmnibus != null && currentChapterIndex < currentOmnibus.MainStories.Count - 1)
        {
            currentChapterIndex++;
            currentEpisodeIndex = 0; // 다음 챕터의 첫 번째 에피소드부터 시작
            story_Index = 0;

            AutoSaveProgress();
            LoadNextStory();
        }
        else
        {
            Debug.Log("더 이상 진행할 다음 챕터가 없습니다.");
        }
    }

    private void Start()
    {
        jsonManager = new JsonManager();
        // 이 부분은 추후에 n 회차일 경우 Omnibus_02, Omnibus_03 등으로 변경할 지 말지는 선택.
        currentOmnibus = jsonManager.LoadData<OmnibusData>("Omnibus_01");

        LoadGame();
    }

    private void LoadGame()
    {
        // 1. 스탯 복구
        if (saveManager.Exists("Stats"))
        {
            var savedStats = saveManager.Load<PlayerStats>("Stats");
            if (savedStats != null && savedStats.stats != null)
            {
                statContainer.stats = savedStats.stats;
                statContainer.RefreshAllUI();
            }
        }

        // 2. 진행도 복구
        if (saveManager.Exists("Progress"))
        {
            var progress = saveManager.Load<GameProgress>("Progress");
            if (progress != null)
            {
                currentChapterIndex = progress.chapterIndex;
                currentEpisodeIndex = progress.episodeIndex;
                story_Index = progress.storyIndex;

                Debug.Log($"[Load] 저장된 지점에서 재시작: Chapter {currentChapterIndex}, Episode {currentEpisodeIndex}, Story {story_Index}");
            }
        }

        LoadNextStory();
    }

    private void AutoSaveProgress()
    {
        GameProgress progress = new GameProgress
        {
            chapterIndex = currentChapterIndex,
            episodeIndex = currentEpisodeIndex,
            storyIndex = story_Index
        };
        saveManager.Save("Progress", progress);
    }

    private void AutoSaveStats()
    {
        PlayerStats stats = new PlayerStats
        {
            stats = statContainer.stats
        };
        saveManager.Save("Stats", stats);
    }

    private void LoadNextStory()
    {
        if (currentOmnibus == null || currentOmnibus.MainStories == null || currentChapterIndex >= currentOmnibus.MainStories.Count)
        {
            Debug.Log("모든 메인 스토리가 종료되었습니다.");
            return;
        }

        var mainStory = currentOmnibus.MainStories[currentChapterIndex];

        if (currentEpisodeIndex >= mainStory.Title.Count)
        {
            currentChapterIndex++;
            currentEpisodeIndex = 0;
            LoadNextStory();
            return;
        }

        string folder = mainStory.Chapter;
        string file = mainStory.Title[currentEpisodeIndex];
        string fullPath = $"{folder}/{file}";

        scenarioData = jsonManager.LoadData<ScenarioData>(fullPath);
        
        if (scenarioData != null)
        {
            if (story_Index >= scenarioData.MainStory.Count) story_Index = 0;
            DisplayCurrentStory();
        }
        else
        {
            Debug.LogError($"스토리를 불러올 수 없습니다: {fullPath}");
        }
    }

    private void DisplayCurrentStory()
    {
        if (scenarioData == null || scenarioData.MainStory == null || scenarioData.MainStory.Count == 0) return;

        var currentStory = scenarioData.MainStory[story_Index];
        front_Dialogue_Text.text = currentStory.text;

        // 배경 설정 적용
        ApplyBackground(cardFront, currentStory.background);

        if (currentStory.type == "Choice")
        {
            currentState = GameState.WaitingForChoice;
            option_Text.gameObject.SetActive(true);

            // [수정] 기어가 이미 꺾여 있다면 즉시 해당 지문 표시
            if (gearController != null && gearController.CurrentGear != 0)
            {
                ShowOptionText(gearController.CurrentGear);
            }
            else
            {
                option_Text.text = "선택지를 선택하세요.";
            }
        }
        else
        {
            currentState = GameState.ShowingStory;
            option_Text.gameObject.SetActive(false);
        }
    }

    private void ApplyBackground(RectTransform card, string bgData)
    {
        if (string.IsNullOrEmpty(bgData) || bgData.ToLower() == "none") return;

        Image dgImg = card.GetComponent<Image>();
        if (dgImg == null) return;

        Color customColor;
        if (ColorUtility.TryParseHtmlString(bgData, out customColor))
        {
            dgImg.sprite = null;
            dgImg.color = customColor;
        }
        else
        {
            Sprite loadedSprite = Resources.Load<Sprite>(bgData);
            if (loadedSprite != null)
            {
                dgImg.sprite = loadedSprite;
                dgImg.color = Color.white;
            }
        }
    }


    public void OnScreenClicked()
    {
        if (currentState == GameState.Transitioning || scenarioData == null || scenarioData.MainStory == null) return;
        if (story_Index < 0 || story_Index >= scenarioData.MainStory.Count) return;

        var currentStory = scenarioData.MainStory[story_Index];

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
        if (currentState == GameState.ShowingStory)
        {
            OnScreenClicked();
            return;
        }

        if (currentState != GameState.WaitingForChoice || scenarioData == null) return;
        if (story_Index < 0 || story_Index >= scenarioData.MainStory.Count) return;

        var currentStory = scenarioData.MainStory[story_Index];
        int optionIndex = GetOptionIndexFromGear(gear);
        
        if (optionIndex >= 0 && optionIndex < currentStory.figure.Length)
        {
            // Initial 챕터(인덱스 0)가 아닐 때만 스탯을 증가시킵니다.
            if (currentChapterIndex > 0)
            {
                statContainer.stats[optionIndex] += currentStory.figure[optionIndex];
                statContainer.UpdateStat(optionIndex);
                
                AutoSaveStats();
            }

            Debug.Log($"[{currentStory.option[optionIndex]}] 선택됨!");
        }

        if (currentState != GameState.Transitioning)
        {
            ProceedToNextStory();
        }
    }

    private void ProceedToNextStory()
    {
        story_Index++;  
        if (scenarioData != null && scenarioData.MainStory != null && story_Index < scenarioData.MainStory.Count)
        {
            var nextStory = scenarioData.MainStory[story_Index];

            if (nextStory.isTransition)
            {
                option_Text.gameObject.SetActive(false);
                currentState = GameState.Transitioning;
                StartCoroutine(SwipeTransition(nextStory));
            }
            else
            {
                DisplayCurrentStory();
            }

            AutoSaveProgress();
        }
        else
        {
            // 현재 에피소드가 끝났으므로 다음 스토리 로드
            currentEpisodeIndex++;
            story_Index = 0;
            AutoSaveProgress();
            LoadNextStory();
        }
    }

    public void ShowOptionText(int gear)
    {
        if (scenarioData == null || scenarioData.MainStory == null || story_Index < 0 || story_Index >= scenarioData.MainStory.Count) return;
        if (scenarioData.MainStory[story_Index].type != "Choice") return;

        // [추가] 기어가 중앙(0)이면 안내 문구로 복구
        if (gear == 0)
        {
            option_Text.text = "선택지를 선택하세요.";
            return;
        }

        int index = GetOptionIndexFromGear(gear);
        if (index >= 0 && index < scenarioData.MainStory[story_Index].option.Length)
        {
            option_Text.text = scenarioData.MainStory[story_Index].option[index];
        }
    }

    private int GetOptionIndexFromGear(int gear)
    {
        if (gear == (int)Constants.gear.EvilGood) return 0;
        if (gear == (int)Constants.gear.EvilBad) return 1;
        if (gear == (int)Constants.gear.GoodGood) return 2;
        if (gear == (int)Constants.gear.GoodBad) return 3;
        return -1;
    }

    private IEnumerator SwipeTransition(Dialogue nextStory)
    {
        back_Dialogue_Text.text = nextStory.text;
        string bgData = nextStory.background;

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

        DisplayCurrentStory();
    }
}
