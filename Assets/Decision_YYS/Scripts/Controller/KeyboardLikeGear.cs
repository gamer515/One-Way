using UnityEngine;
using UnityEngine.UI;

public class KeyboardLikeGear : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform keyboard;
    [SerializeField] private RectTransform pivot;

    [Header("Gear Settings")]
    [SerializeField] private float horizontalRange = 200f;
    [SerializeField] private float verticalRange = 150f;
    [SerializeField] private float smoothTime = 0.04f;      // 키보드 대응을 위해 더 빠른 반응속도 (0.04~0.06 추천)

    private Vector2 targetPosition;
    private Vector2 currentVelocity;
    
    // 현재 기어 상태 (물리적 위치 기반이 아닌 논리적 상태)
    private int gearX = 0; // -1: 좌측, 0: 중앙(중립통로), 1: 우측
    private int gearY = 0; // -1: 아래, 0: 중앙, 1: 위

    private int lastGear = 0;

    private void Update()
    {
        HandleKeyboardInput();
        MoveGearSmoothly();
        CheckGearState();
    }

    private void HandleKeyboardInput()
    {
        // 1. 좌우 이동 (기어가 위/아래로 들어가 있지 않을 때만 가능)
        if (gearY == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (gearX == 1) gearX = 0;      // 우측 -> 중앙
                else if (gearX == 0) gearX = -1; // 중앙 -> 좌측
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (gearX == -1) gearX = 0;     // 좌측 -> 중앙
                else if (gearX == 0) gearX = 1;  // 중앙 -> 우측
            }
        }

        // 2. 상하 이동 (기어가 좌측 또는 우측 끝에 있을 때만 가능)
        if (gearX != 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (gearY == -1) gearY = 0;      // 아래 -> 중앙
                else if (gearY == 0) gearY = 1;  // 중앙 -> 위
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (gearY == 1) gearY = 0;       // 위 -> 중앙
                else if (gearY == 0) gearY = -1; // 중앙 -> 아래
            }
        }

        // 목표 위치 계산
        targetPosition = new Vector2(gearX * horizontalRange, gearY * verticalRange);
    }

    private void MoveGearSmoothly()
    {
        if (keyboard != null)
        {
            // SmoothDamp가 키보드 입력에도 "기계가 움직이는 듯한" 묵직함을 줍니다.
            keyboard.anchoredPosition = Vector2.SmoothDamp(
                keyboard.anchoredPosition, 
                targetPosition, 
                ref currentVelocity, 
                smoothTime
            );
        }
    }

    private void CheckGearState()
    {
        int currentGear = 0;

        // 논리적 기어 판정
        if (gearX == -1 && gearY == 1) currentGear = 1;
        else if (gearX == -1 && gearY == -1) currentGear = 2;
        else if (gearX == 1 && gearY == 1) currentGear = 3;
        else if (gearX == 1 && gearY == -1) currentGear = 4;
        else currentGear = 0; // Neutral

        if (currentGear != lastGear)
        {
            if (currentGear != 0)
            {
                OnGearEngaged(currentGear);
            }
            else
            {
                Debug.Log("Gear: Neutral");
            }
            lastGear = currentGear;
        }
    }

    private void OnGearEngaged(int gear)
    {
        // 기어가 "탁" 하고 걸리는 순간 실행될 로직
        Debug.Log($"Gear {gear} Engaged! (Keyboard)");
        
        // 여기에 변속 효과음이나 화면 흔들림 등을 추가하면 손맛이 극대화됩니다.
    }
}
