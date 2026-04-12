using UnityEngine;
using UnityEngine.UI;

public class JoystickLikeGear : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform joystick_Button;
    [SerializeField] private RectTransform pivot;

    [Header("Follow Settings")]
    [SerializeField] private float detectionRange = 300f;   // 마우스 감지 범위
    [SerializeField] private float horizontalRange = 200f;
    [SerializeField] private float verticalRange = 150f;
    [SerializeField] private float slotWidth = 60f;         // 수직 슬롯 진입 허용폭
    [SerializeField] private float smoothTime = 0.08f;      // 따라오는 속도

    [SerializeField] DecisionManager decisionManager;

    private Vector2 targetPosition;
    private Vector2 currentVelocity;
    private int currentGearSlot = 0; // 현재 버튼이 위치한 슬롯 (0: 중립, 1~4: 기어)

    private void Update()
    {
        if (joystick_Button == null || pivot == null) return;

        HandleMouseProximity();
        MoveGearSmoothly();
        CheckGearState();
        HandleSelectionClick(); // 클릭 감지 로직 추가
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

    private void CheckGearState()
    {
        int gear = 0;
        Vector2 pos = joystick_Button.anchoredPosition;

        // 실제 버튼이 슬롯 끝에 거의 도달했는지 판정 (임계값 15 내외)
        if (pos.x < -horizontalRange + 15 && pos.y > verticalRange - 15)
        { 
            gear = 1;
            decisionManager.ShowOptionText(gear);
        }
        else if (pos.x < -horizontalRange + 15 && pos.y < -verticalRange + 15)
        {
            gear = 2;
            decisionManager.ShowOptionText(gear);
        }
        else if (pos.x > horizontalRange - 15 && pos.y > verticalRange - 15)
        {
            gear = 3;
            decisionManager.ShowOptionText(gear);
        }
        else if (pos.x > horizontalRange - 15 && pos.y < -verticalRange + 15)
        {
            gear = 4;
            decisionManager.ShowOptionText(gear);
        }

        if (gear != currentGearSlot)
        {
            if (gear != 0) Debug.Log($"Gear {gear} Hovering (Ready to Select)");
            currentGearSlot = gear;
        }
    }

    private void HandleSelectionClick()
    {
        // 마우스 왼쪽 버튼을 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            // 현재 기어 슬롯(1~4)에 버튼이 들어가 있다면 선택 완료!
            if (currentGearSlot != 0)
            {
                int gear = OnGearSelected(currentGearSlot);
                decisionManager.ChooseOption(gear);
                decisionManager.SetStat(gear);
            }
            else
            {
                Debug.Log("Clicked on Neutral - No selection made.");
            }
        }
    }

    private int OnGearSelected(int gear)
    {
        // 최종 결정 시 실행될 로직
        Debug.Log($"<color=cyan><b>Final Decision: Gear {gear} Selected!</b></color>");
        
        return gear;
        // 여기서 DecisionManager 등에 선택한 기어 번호를 전달하면 됩니다.
        // 예: FindObjectOfType<DecisionManager>().ConfirmChoice(gear);
    }
}
