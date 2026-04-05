using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickButton : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField]
    private RectTransform joystick_Button;
    [SerializeField]
    private Image joystick_Image;
    [SerializeField]
    private RectTransform pivot;

    private Vector2 localCursor;
    private bool isDragging;

    public void OnDrag(PointerEventData eventData)
    {
        //// 마우스 위치를 Background 기준의 로컬 좌표로 변환 (중요!)
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(pivot, eventData.position, eventData.pressEventCamera, out localCursor);

        //float x = localCursor.x;
        //float y = localCursor.y;

        //// 1. 가로축 범위 제한 (H의 가로 막대)
        //x = Mathf.Clamp(x, -200, 200);

        //// 2. 세로축 이동 조건 (특정 X축 지점에서만 위아래 이동 허용)
        //if (Mathf.Abs(x) > 180)
        //{ // 양 끝에 도달했을 때
        //    y = Mathf.Clamp(y, -100, 100);
        //}
        //else if (Mathf.Abs(x) < 20)
        //{ // 정중앙일 때 (추가 경로가 있다면)
        //    y = Mathf.Clamp(y, -100, 100);
        //}
        //else
        //{
        //    y = 0; // 통로가 아닌 곳에선 Y축 고정
        //}

        //joystick_Button.anchoredPosition = new Vector2(x, y);

        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pivot, eventData.position, eventData.pressEventCamera, out localCursor);

        float tx = Mathf.Clamp(localCursor.x, -200, 200);
        float ty = localCursor.y;

        // --- [핵심] 곡선 통로 로직 ---
        // 양 끝(180~200 사이)에 가까워질수록 '여유 공간'을 계산합니다.
        float edgeThreshold = 170f; // 여기서부터 서서히 Y축이 풀리기 시작함
        float freedom = 0f;

        if (Mathf.Abs(tx) > edgeThreshold)
        {
            // 끝에 가까워질수록 0에서 1사이로 값이 커짐
            freedom = (Mathf.Abs(tx) - edgeThreshold) / (200f - edgeThreshold);
        }
        //else if (Mathf.Abs(tx) < 30f) // 중앙 통로도 부드럽게 하고 싶다면 추가
        //{
        //    freedom = 1f - (Mathf.Abs(tx) / 30f);
        //}

        // freedom이 0이면 중앙 가로선에 딱 붙고, 1이면 위아래로 완전히 자유로움
        ty = Mathf.Clamp(ty, -150 * freedom, 150 * freedom);

        joystick_Button.anchoredPosition = new Vector2(tx, ty);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystick_Button.anchoredPosition = Vector2.zero;
    }
}
