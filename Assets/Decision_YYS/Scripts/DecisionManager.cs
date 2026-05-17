using UnityEngine;
using TMPro;
using static Constants;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class DecisionManager : MonoBehaviour
{
    [SerializeField] private StatContainer statContainer;

    // [추가] 스탯 변화에 따른 전투 씬 진입을 관리하기 위한 이벤트 구독 및 처리 메서드를 클래스로 구분해야 함.
    #region CardAnimation
    [SerializeField] private RectTransform cardFront;
    [SerializeField] private RectTransform cardBack;

    [SerializeField] private TextMeshProUGUI front_Dialogue_Text;
    [SerializeField] private TextMeshProUGUI back_Dialogue_Text;
    [SerializeField] private TextMeshProUGUI option_Text;
    #endregion

    private GameState currentState;

    private IJsonSerializer jsonManager;

    // [추가] 저장된 진행도 및 스탯을 관리하는 클래스로 구분해야 함.
    #region Data
    ScenarioData scenarioData;

    private OmnibusData currentOmnibus;
    // 인트로, 무협~
    private int chapterIndex = 0;
    // initial_1, initial_2~, martial_1~
    private int episodeIndex = 0;
    // 각 에피소드 내에서 지문 하나하나
    private int storyIndex = 0;
    #endregion

    // [추가] 현재 챕터에서 플레이어가 읽은 모든 지문 기록을 클래스로 구분해야 함.
    #region
    private List<Dialogue> playedHistory = new List<Dialogue>();
    #endregion

    // 과한 coupling을 줄여야 하는데, 일단은 편의상 DecisionManager에서 직접 참조하는 중.
    // 추후에 필요하면 별도의 Manager 클래스로 분리해야 함.
    [SerializeField] private JoystickLikeGear gearController;
    [SerializeField] private StoryRelayManager relayManager;

    // 추후에 전투 씬도 추가한 후에는, 전투 씬과 관련된 데이터 관리 및 저장 기능도 별도의 클래스로 구분하는 것을 권장.
    private SaveDataManager saveDataManager;

    private void Awake()
    {
        currentState = GameState.ShowingStory;
        saveDataManager = new SaveDataManager(new SaveManager());
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
        if (chapterIndex == 0) return;

        Debug.Log("전투 발생! 현재 진행 상황을 저장하고 전투 씬으로 이동합니다.");

        // [중요] 전투 씬으로 넘어가기 직전에 현재 챕터 결과 기록 및 다음 챕터 준비
        if (currentOmnibus != null && chapterIndex < currentOmnibus.MainStories.Count)
        {
            // 1. 현재 완료된 챕터의 최고 스탯 결과 기록
            int bestStatIndex = 0;
            int maxValue = -1;
            int[] currentStats = statContainer.stats;
            for (int i = 0; i < currentStats.Length; i++)
            {
                if (currentStats[i] > maxValue)
                {
                    maxValue = currentStats[i];
                    bestStatIndex = i;
                }
            }
            saveDataManager.RecordChapterResult(chapterIndex, bestStatIndex, maxValue);

            // 2. 외부 데이터 전송 (이미 필터링된 핵심 데이터 전송)
            if (relayManager != null)
            {
                relayManager.Relay("MidTransition", playedHistory, statContainer.stats, chapterIndex);
            }

            // 3. 다음 챕터로 인덱스 준비
            chapterIndex++;
            episodeIndex = 0;
            storyIndex = 0;

            // 4. 저장 (씬이 다시 로드될 때 여기서부터 시작하기 위함)
            saveDataManager.SaveProgress(chapterIndex, episodeIndex, storyIndex);
            
            // 5. 스탯 초기화 및 초기화된 스탯 저장
            statContainer.ResetAllStats();
            saveDataManager.SaveStats(statContainer.stats);

            // 6. 전투 씬으로 전환
            currentState = GameState.Transitioning;
            UnityEngine.SceneManagement.SceneManager.LoadScene("TempAttackScene");
        }
    }

    private void Start()
    {
        // 싱글톤으로 할 지, 말 지는 추후에 결정. 일단은 편의상 DecisionManager에서 직접 참조하는 중.
        jsonManager = new JsonManager();
        // 이 부분은 추후에 n 회차일 경우 Omnibus_02, Omnibus_03 등으로 변경할 지 말지는 선택.
        currentOmnibus = jsonManager.LoadData<OmnibusData>("Omnibus_01");

        LoadGame();
    }

    private void LoadGame()
    {
        // 1. 스탯 복구
        var savedStats = saveDataManager.LoadStats();
        if (savedStats != null && savedStats.stats != null)
        {
            statContainer.SetStats(savedStats.stats);
        }

        // 2. 진행도 복구
        var progress = saveDataManager.LoadProgress();
        if (progress != null)
        {
            chapterIndex = progress.chapterIndex;
            episodeIndex = progress.episodeIndex;
            storyIndex = progress.storyIndex;

            Debug.Log($"[Load] 저장된 지점에서 재시작: Chapter {chapterIndex}, Episode {episodeIndex}, Story {storyIndex}");
        }

        LoadNextStory();
    }

    private void MoveToNextChapter()
    {
        // 1. 현재 챕터 결과 기록
        int bestStatIndex = 0;
        int maxValue = -1;
        int[] currentStats = statContainer.stats;
        for (int i = 0; i < currentStats.Length; i++)
        {
            if (currentStats[i] > maxValue)
            {
                maxValue = currentStats[i];
                bestStatIndex = i;
            }
        }

        saveDataManager.RecordChapterResult(chapterIndex, bestStatIndex, maxValue);

        // [추가] 챕터 종료 데이터 전송 (전체 히스토리)
        if (relayManager != null)
        {
            relayManager.Relay("ChapterEnd", playedHistory, statContainer.stats, chapterIndex);
        }

        // 2. 다음 챕터로 인덱스 변경
        chapterIndex++;
        episodeIndex = 0;
        storyIndex = 0;

        // [추가] 챕터가 바뀌었으므로 플레이 기록 초기화
        playedHistory.Clear();

        // 3. 스탯 초기화
        statContainer.ResetAllStats();

        // 4. 저장 및 다음 스토리 로드
        saveDataManager.SaveProgress(chapterIndex, episodeIndex, storyIndex);
        saveDataManager.SaveStats(statContainer.stats);

        LoadNextStory();
    }

    private void LoadNextStory()
    {
        if (currentOmnibus == null || currentOmnibus.MainStories == null || chapterIndex >= currentOmnibus.MainStories.Count)
        {
            Debug.Log("모든 메인 스토리가 종료되었습니다.");
            return;
        }

        var mainStory = currentOmnibus.MainStories[chapterIndex];

        if (episodeIndex >= mainStory.Title.Count)
        {
            MoveToNextChapter();
            return;
        }

        string folder = mainStory.Chapter;
        string file = mainStory.Title[episodeIndex];
        string fullPath = $"{folder}/{file}";

        scenarioData = jsonManager.LoadData<ScenarioData>(fullPath);
        
        if (scenarioData != null)
        {
            if (storyIndex >= scenarioData.MainStory.Count) storyIndex = 0;
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

        var currentStory = scenarioData.MainStory[storyIndex];
        front_Dialogue_Text.text = currentStory.text;

        // [추가] 플레이어가 읽은 지문을 기록 리스트에 추가 (중복 방지: 이미 마지막 항목과 같으면 패스)
        if (playedHistory.Count == 0 || playedHistory[playedHistory.Count - 1] != currentStory)
        {
            playedHistory.Add(currentStory);
        }

        // 배경 설정 적용
        ApplyBackground(cardFront, currentStory.background);

        if (currentStory.type == "Choice")
        {
            EnterChoiceState();
        }
        else
        {
            currentState = GameState.ShowingStory;
            option_Text.gameObject.SetActive(false);
        }
    }

    private void EnterChoiceState()
    {
        currentState =  GameState.WaitingForChoice;
        option_Text.gameObject.SetActive(true);

        // [수정] 캐시된 CurrentGear 대신 직접 현재 물리적 위치를 확인하여 즉시 반영
        int currentGear = (gearController != null) ? gearController.GetCurrentGearDirectly() : 0;

        if (currentGear != 0)
        {
            ShowOptionText(currentGear);
        }
        else
        {
            // [수정] 중앙(0)일 때는 안내 문구로 복구
            option_Text.text = "선택지를 선택하세요.";
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
        if (currentState ==  GameState.Transitioning || scenarioData == null || scenarioData.MainStory == null) return;
        if (storyIndex < 0 || storyIndex >= scenarioData.MainStory.Count) return;

        var currentStory = scenarioData.MainStory[storyIndex];

        if (currentState ==  GameState.ShowingStory)
        {
            if (currentStory.type == "Choice")
            {
                EnterChoiceState();
            }
            else
            {
                ProceedToNextStory();
            }
        }
    }

    public void ConfirmChoice(int gear)
    {
        if (currentState ==  GameState.ShowingStory)
        {
            OnScreenClicked();
            return;
        }

        if (currentState !=  GameState.WaitingForChoice || scenarioData == null) return;
        if (storyIndex < 0 || storyIndex >= scenarioData.MainStory.Count) return;

        var currentStory = scenarioData.MainStory[storyIndex];
        int optionIndex = GetOptionIndexFromGear(gear);
        
        if (optionIndex >= 0 && optionIndex < currentStory.figure.Length)
        {
            // Initial 챕터(인덱스 0)가 아닐 때만 스탯을 증가시킵니다.
            if (chapterIndex > 0)
            {
                statContainer.AddStat(optionIndex, currentStory.figure[optionIndex]);
                saveDataManager.SaveStats(statContainer.stats);
            }

            Debug.Log($"[{currentStory.option[optionIndex]}] 선택됨!");
        }

        if (currentState !=  GameState.Transitioning)
        {
            ProceedToNextStory();
        }
    }

    private void ProceedToNextStory()
    {
        storyIndex++;  
        if (scenarioData != null && scenarioData.MainStory != null && storyIndex < scenarioData.MainStory.Count)
        {
            var nextStory = scenarioData.MainStory[storyIndex];

            if (nextStory.isTransition)
            {
                option_Text.gameObject.SetActive(false);
                currentState =  GameState.Transitioning;
                StartCoroutine(SwipeTransition(nextStory));
            }
            else
            {
                DisplayCurrentStory();
            }

            saveDataManager.SaveProgress(chapterIndex, episodeIndex, storyIndex);
        }
        else
        {
            // 현재 에피소드가 끝났으므로 다음 스토리 로드
            episodeIndex++;
            storyIndex = 0;
            saveDataManager.SaveProgress(chapterIndex, episodeIndex, storyIndex);
            LoadNextStory();
        }
    }

    public void ShowOptionText(int gear)
    {
        if (scenarioData == null || scenarioData.MainStory == null || storyIndex < 0 || storyIndex >= scenarioData.MainStory.Count) return;
        if (scenarioData.MainStory[storyIndex].type != "Choice") return;

        // [추가] 기어가 중앙(0)이면 안내 문구로 복구
        if (gear == 0)
        {
            option_Text.text = "선택지를 선택하세요.";
            return;
        }

        int index = GetOptionIndexFromGear(gear);
        if (index >= 0 && index < scenarioData.MainStory[storyIndex].option.Length)
        {
            option_Text.text = scenarioData.MainStory[storyIndex].option[index];
        }
    }

    private int GetOptionIndexFromGear(int gear)
    {
        if (gear == (int) Gear.EvilGood) return 0;
        if (gear == (int) Gear.EvilBad) return 1;
        if (gear == (int) Gear.GoodGood) return 2;
        if (gear == (int) Gear.GoodBad) return 3;
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
