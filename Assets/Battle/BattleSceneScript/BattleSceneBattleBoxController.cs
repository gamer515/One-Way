using UnityEngine;
using System.Collections;

public partial class BattleSceneBattleBoxController : MonoBehaviour
{
    
    [Header("Wall Transforms")]
    public Transform topWall;
    public Transform bottomWall;
    public Transform leftWall;
    public Transform rightWall;

    [Header("Box Configurations")]
    // 스크린샷의 샌즈 대화창과 유사한 기본값 (유니티 유닛 단위)
    public Vector2 dialogueSize = new Vector2(14f, 4f);
    public Vector2 dialoguePos = new Vector2(0f, -2.5f);
    public float wallThickness = 0.15f;

    [Header("UI Reference")]
    public GameObject dialogueContent; // 대화 텍스트가 담긴 UI 오브젝트
    private Coroutine resizeCoroutine;
    public TypewriterEffect typewriter; // 새로 추가: 인스펙터에서 연결해주세요!
    public string textToSay = "* 당신은 죄악이 등을 타고\n  오르는 것을 느꼈다."; // 테스트용 텍스트

    // 대화창 모드로 전환하는 함수
    
    public void SetDialogueMode(float duration, string text) // text 파라미터 추가
    {
        textToSay = text;
        if (resizeCoroutine != null) StopCoroutine(resizeCoroutine);

        // 1. 상자 크기 조절 시작
        resizeCoroutine = StartCoroutine(AnimateBox(dialogueSize, dialoguePos, duration));

        // 2. 상자가 어느 정도 커졌을 때 텍스트를 활성화 (약간의 딜레이)
        StartCoroutine(ShowTextDelayed(duration * 0.8f));
    }

    private IEnumerator ShowTextDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dialogueContent != null)
        {
            dialogueContent.SetActive(true);
        }

        // 텍스트 UI가 켜진 직후에 타이핑 효과 시작!
        if (typewriter != null)
        {
            typewriter.PlayText(textToSay);
        }
    }

    private IEnumerator AnimateBox(Vector2 targetSize, Vector2 targetPos, float duration)
    {
        Vector2 startSize = GetCurrentSize();
        Vector2 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            Vector2 currentSize = Vector2.Lerp(startSize, targetSize, t);
            transform.position = Vector2.Lerp(startPos, targetPos, t);

            UpdateWalls(currentSize);
            yield return null;
        }

        UpdateWalls(targetSize);
        transform.position = targetPos;
    }

    private void UpdateWalls(Vector2 size)
    {
        float halfW = size.x / 2f;
        float halfH = size.y / 2f;

        topWall.localPosition = new Vector3(0, halfH, 0);
        topWall.localScale = new Vector3(size.x + wallThickness, wallThickness, 1);

        bottomWall.localPosition = new Vector3(0, -halfH, 0);
        bottomWall.localScale = new Vector3(size.x + wallThickness, wallThickness, 1);

        leftWall.localPosition = new Vector3(-halfW, 0, 0);
        leftWall.localScale = new Vector3(wallThickness, size.y + wallThickness, 1);

        rightWall.localPosition = new Vector3(halfW, 0, 0);
        rightWall.localScale = new Vector3(wallThickness, size.y + wallThickness, 1);
    }

    private Vector2 GetCurrentSize()
    {
        return new Vector2(rightWall.localPosition.x * 2f, topWall.localPosition.y * 2f);
    }

    public void ChangeBox(Vector2 targetSize, Vector2 targetPos, float duration)
    {
        if (resizeCoroutine != null) StopCoroutine(resizeCoroutine);

        // 대화 내용이 있다면 상자가 바뀔 때 꺼줍니다. (전투 모드로 전환 대비)
        if (dialogueContent != null) dialogueContent.SetActive(false);

        resizeCoroutine = StartCoroutine(AnimateBox(targetSize, targetPos, duration));
    }
}