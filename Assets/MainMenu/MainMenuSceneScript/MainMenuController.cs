using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO; // 파일 및 폴더 접근을 위해 반드시 추가해야 합니다.
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("패널 연결")]
    public GameObject mainMenuPanel;
    public GameObject biographyPanel;
    public GameObject settingsPanel;

    [Header("시작 연출 설정")]
    public GameObject burstImage; // 반짝이는 이펙트 이미지
    public BiographyController biographyController;

    private bool isAnimating = false;

    // [추가] 메인 메뉴가 켜질 때 초기화 로직 실행
    private void Start()
    {
        ResetGameData();
    }

    // [추가] 세이브 폴더를 비우고 BattleIndex를 초기화하는 함수
    private void ResetGameData()
    {
        // 1. BattleIndex를 1로 초기화 (PlayerPrefs 사용)
        PlayerPrefs.SetInt("CurrentBattleIndex", 1);
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] BattleIndex가 1로 초기화되었습니다.");

        // 2. Saves 폴더 경로 설정 (사용자 컴퓨터의 AppData 경로와 동일)
        string savesPath = Path.Combine(Application.persistentDataPath, "Saves");

        // 3. 폴더가 존재한다면 내부의 모든 파일 삭제
        if (Directory.Exists(savesPath))
        {
            string[] files = Directory.GetFiles(savesPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Debug.Log($"[MainMenu] Saves 폴더의 세이브 파일 {files.Length}개를 모두 삭제했습니다.");
        }
        else
        {
            Debug.Log("[MainMenu] Saves 폴더가 아직 존재하지 않아 삭제를 건너뜁니다.");
        }
    }

    // 1. 시작 버튼 클릭
    public void OnClickStart()
    {
        if (isAnimating) return;
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        isAnimating = true;

        // 반짝이는 이펙트 재생 (사용자가 작성한 연출 활용)
        yield return StartCoroutine(PlayBurstEffect());

        // 씬 전환
        SceneManager.LoadScene("DecisionScene");
    }

    private IEnumerator PlayBurstEffect()
    {
        if (burstImage != null)
        {
            burstImage.SetActive(true);
            Image burstImgComp = burstImage.GetComponent<Image>();
            RectTransform burstRect = burstImage.GetComponent<RectTransform>();

            float duration = 0.5f; // 반짝이는 시간
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

    // 2. 기록(Biography) 버튼 클릭
    public void OnClickBiography()
    {
        mainMenuPanel.SetActive(false);
        biographyPanel.SetActive(true);
        biographyController.OpenBiography();
    }

    // 3. 설정(Settings) 버튼 클릭
    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
    }

    // 4. 나가기(Quit) 버튼 클릭
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}