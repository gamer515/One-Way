using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    
    public GameObject biographyPanel;
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;

    
    public BiographyController biographyController;

    public bool isLoadMode = false;

    /*public void OnClickStart()
    {
        if (isLoadMode) Debug.Log("이어하기 시작");
        else Debug.Log("새 게임 시작");
        // SceneManager.LoadScene("GameScene");
    }*/
    public RectTransform buttonFrame;
    public RectTransform startButton;
    public RectTransform[] cornerButtons;
    public GameObject burstImage;

    [Header("애니메이션 설정")]
    public float animDuration = 1.2f;     // 확장되는 시간 (조금 더 늘려 여유있게)
    public float waitTime = 0.5f;         // 중앙에서 회전하며 대기하는 시간
    public float rotationSpeed = 720f;

    private bool isAnimating = false;

    public void OnClickStart()
    {
        if (isAnimating) return;
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        isAnimating = true;

        // 1. 초기값 계산
        Vector2 fullFrameSize = buttonFrame.sizeDelta;
        Vector2 targetSize = new Vector2(fullFrameSize.x / 2f, fullFrameSize.y / 2f);

        // 각 버튼의 시작 크기를 저장해둡니다 (예: 100x100)
        Vector2[] initialSizes = new Vector2[cornerButtons.Length];
        TextMeshProUGUI[] buttonTexts = new TextMeshProUGUI[cornerButtons.Length];

        for (int i = 0; i < cornerButtons.Length; i++)
        {
            initialSizes[i] = cornerButtons[i].sizeDelta;
            buttonTexts[i] = cornerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        Vector2 startBtnInitialPos = startButton.anchoredPosition;
        float elapsed = 0f;

        // 2. 확장 및 이동 단계 (중단 없는 부드러운 성장)
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            // SmoothStep을 사용해 시작은 부드럽게, 끝은 딱 맞게 감속하며 들어갑니다.
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            // A. 버튼 확장: 시작 크기 -> 목표 크기(1/4)까지 쭉 커짐
            for (int i = 0; i < cornerButtons.Length; i++)
            {
                cornerButtons[i].sizeDelta = Vector2.Lerp(initialSizes[i], targetSize, t);

                // 텍스트는 커지는 동안 서서히 사라짐
                if (buttonTexts[i] != null)
                    buttonTexts[i].alpha = Mathf.Lerp(1f, 0f, t);
            }

            // B. Start 버튼: 회전하며 중앙(0,0)으로 이동
            startButton.anchoredPosition = Vector2.Lerp(startBtnInitialPos, Vector2.zero, t);
            startButton.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            yield return null;
        }

        // 3. 중앙 대기 단계 (start 버튼만 계속 회전)
        float waitTimer = 0f;
        while (waitTimer < waitTime)
        {
            waitTimer += Time.deltaTime;
            startButton.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        // 4. 새로운 이미지와 함께 반짝이는 이펙트
        yield return StartCoroutine(PlayBurstEffect());

        // 5. 씬 전환
        //UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuTestGameScene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("DecisionScene");
    }

    private IEnumerator PlayBurstEffect()
    {
        if (burstImage != null)
        {
            burstImage.SetActive(true);
            Image burstImgComp = burstImage.GetComponent<Image>();
            RectTransform burstRect = burstImage.GetComponent<RectTransform>();

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 이미지가 확 커지면서 투명해지는 연출
                burstRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 5f, t);
                burstImgComp.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0f, t));

                yield return null;
            }
            burstImage.SetActive(false);
        }
    }


    //------------------------------------------------------------

    public void OnClickLoad()
    {
        isLoadMode = true;
        Debug.Log("불러오기 모드 활성화");
    }

    // 기록 버튼 클릭 시
    public void OnClickBiography()
    {
        mainMenuPanel.SetActive(false); // 메인 메뉴 끄기
        biographyPanel.SetActive(true); // 기록 패널 켜기
        biographyController.OpenBiography(); // 데이터 로드 실행
    }

    // 설정 버튼 클릭 시
    public void OnClickSettings()
    {
        settingsPanel.SetActive(true); // 설정 패널 켜기 (메인은 켜둘 수도, 꺼둘 수도 있음)
    }

    //public void OnClickQuit() => Application.Quit(); 실제 게임에서 사용할 코드 아래는 유니티 테스트용

    public void OnClickQuit()
    {
        // 빌드된 게임에서 실행될 때
        #if UNITY_STANDALONE    //유니티 애디터 전용 실제 완성할때는 위에 OnClickQuit사용하기
            Application.Quit();
        #endif

        // 유니티 에디터에서 실행 중일 때 (Play 모드 종료)
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}