using UnityEngine;

public class Battle3DAttackGaugeManager : MonoBehaviour
{
    public Battle3DBattleStateMachine stateMachine;
    public RectTransform striker;
    public RectTransform targetCenter;
    public float moveSpeed = 10f;
    public float limitX = 600f;

    [Header("Judgment Settings")]
    public float perfectDistance = 5f;  // ���� 50f -> 20f (�� ����!)
    public float goodDistance = 5f;     // ���� 200f -> 80f (�� ����!)


    private bool isMoving = false;
    private int direction = 1;

    public void StartGauge()
    {
        // gameObject.SetActive(true); <-- ����! (BattleManager�� �׻� �����־�� ��)
        striker.anchoredPosition = new Vector2(-limitX, 0);
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        float currentX = striker.anchoredPosition.x;
        currentX += moveSpeed * direction * Time.deltaTime * 100f;

        if (currentX > limitX) direction = -1;
        else if (currentX < -limitX) direction = 1;

        striker.anchoredPosition = new Vector2(currentX, 0);

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
        {
            StopAndCalculate();
        }
    }

    private void StopAndCalculate()
    {
        isMoving = false;

        float distance = Mathf.Abs(striker.anchoredPosition.x - targetCenter.anchoredPosition.x);
        float damage = 0;

        if (distance < perfectDistance)
        {
            Debug.Log("����: Perfect! 1 ������");
            damage = 1f;
        }
        else if (distance < goodDistance)
        {
            Debug.Log("����: Good! 1 ������");
            damage = 1f;
        }
        else
        {
            Debug.Log("����: Miss! ������ ����");
            damage = 0f;
        }

        // gameObject.SetActive(false); <-- ����! (���⼭ ���� ���� ����)

        // ��� ����
        stateMachine.OnPlayerAttackComplete(damage);
    }
}
