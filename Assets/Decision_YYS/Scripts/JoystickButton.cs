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

    Vector2 localCursor;

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치를 Background 기준의 로컬 좌표로 변환 (중요!)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pivot, eventData.position, eventData.pressEventCamera, out localCursor);

        float x = localCursor.x;
        float y = localCursor.y;

        // 1. 가로축 범위 제한 (H의 가로 막대)
        x = Mathf.Clamp(x, -200, 200);

        // 2. 세로축 이동 조건 (특정 X축 지점에서만 위아래 이동 허용)
        if (Mathf.Abs(x) > 180)
        { // 양 끝에 도달했을 때
            y = Mathf.Clamp(y, -100, 100);
        }
        else if (Mathf.Abs(x) < 20)
        { // 정중앙일 때 (추가 경로가 있다면)
            y = Mathf.Clamp(y, -100, 100);
        }
        else
        {
            y = 0; // 통로가 아닌 곳에선 Y축 고정
        }

        joystick_Button.anchoredPosition = new Vector2(x, y);
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
