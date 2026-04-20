using UnityEngine;
using UnityEngine.UI;

public class JoystickLikeGear : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform joystick_Button;
    [SerializeField] private RectTransform pivot;

    [Header("Follow Settings")]
    // 마우스 감지 범위
    [SerializeField] private float detectionRange = 300f; 
    [SerializeField] private float horizontalRange = 200f;
    [SerializeField] private float verticalRange = 150f;
    // 수직 슬롯 진입 허용폭
    [SerializeField] private float slotWidth = 60f;
    // 따라오는 속도
    [SerializeField] private float smoothTime = 0.08f;     

    [SerializeField] DecisionManager decisionManager;

    private Vector2 targetPosition;
    private Vector2 currentVelocity;
    // 현재 버튼이 위치한 슬롯 (0: 중립, 1~4: 기어)
    private int currentGearSlot = 0; 

    private void Update()
    {
        if (joystick_Button == null || pivot == null) return;

        // 클릭 감지 로직 추가
        HandleSelectionClick();
        // 클릭과 동시에 stat과 선택지가 선택이 되는 듯함.
        CheckGearState();
        HandleMouseProximity();
    }

    private void FixedUpdate()
    {
        MoveGearSmoothly();
    }

    private void HandleMouseProximity()
    {
        Vector2 localMousePos;
        // 화면 좌표를 피벗의 로컬 좌표로 변환.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pivot, Input.mousePosition, null, out localMousePos);

        // 벡터의 크기로 마우스가 감지 범위 내에 있는지 판단.
        float distance = localMousePos.magnitude;

        // 상하좌우 슬롯 범위 내에서 마우스 위치에 따라 버튼이 따라오도록 설정.
        if (distance < detectionRange)
        {
            float tx = Mathf.Clamp(localMousePos.x, -horizontalRange, horizontalRange);
            float ty = 0;

            float edgeThreshold = horizontalRange - slotWidth;
            
            if (Mathf.Abs(tx) > edgeThreshold)
            {
                ty = Mathf.Clamp(localMousePos.y, -verticalRange, verticalRange);
                
                // 마우스가 슬롯 깊숙이 있으면 수평 위치를 끝단으로 고정
                if (Mathf.Abs(ty) > verticalRange * 0.4f)
                {
                    tx = (tx > 0) ? horizontalRange : -horizontalRange;
                }
            }
            else
            {
                ty = 0;
            }

            targetPosition = new Vector2(tx, ty);
        }
        else
        {
            targetPosition = Vector2.zero;
        }
    }

    private void MoveGearSmoothly()
    {
        joystick_Button.anchoredPosition = Vector2.SmoothDamp(
            joystick_Button.anchoredPosition, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime
        );
    }

    // 특정 방향으로 선택 및 클릭 전 기어 슬롯에 들어가 있는지 판정하는 함수.
    private void CheckGearState()
    {
        // 빠르게 누르면 gear가 0으로 인식하는 경우가 있음.
        int gear = 0;
        Vector2 pos = joystick_Button.anchoredPosition;

        // 판정 범위 완화: 슬롯 끝단 근처(약 75% 이상 도달 시)로 판정하여 조작감 개선
        float xThreshold = horizontalRange * 0.75f;
        float yThreshold = verticalRange * 0.75f;

        if (pos.x < -xThreshold && pos.y > yThreshold) gear = 1;
        else if (pos.x < -xThreshold && pos.y < -yThreshold) gear = 2;
        else if (pos.x > xThreshold && pos.y > yThreshold) gear = 3;
        else if (pos.x > xThreshold && pos.y < -yThreshold) gear = 4;

        if (gear != currentGearSlot)
        {
            currentGearSlot = gear;
            if (gear != 0)
            {
                decisionManager.ShowOptionText(gear);
            }
        }
    }

    private void HandleSelectionClick()
    {
        // 마우스 왼쪽 버튼을 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            if (currentGearSlot != 0)
            {
                // 1. 기어 슬롯(1~4)에 들어간 상태라면 -> 선택 확정!
                OnGearSelected(currentGearSlot);
            }
            else
            {
                // 2. 기어가 중립(0) 상태라면 -> 평상시 이야기 진행!
                decisionManager.OnScreenClicked();
            }
        }
    }

    private void OnGearSelected(int gear)
    {
        // 최종 결정 시 실행될 로직
        Debug.Log($"<color=cyan><b>Final Decision: Gear {gear} Selected!</b></color>");
        
        // 선택 확정 처리 (스탯 반영 및 스토리 진행)
        decisionManager.ConfirmChoice(gear); 
        
        // 버튼 리셋
        joystick_Button.anchoredPosition = Vector2.zero; 
        targetPosition = Vector2.zero; // 부드러운 이동 목표도 초기화
    }
}
