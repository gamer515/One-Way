using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro ����� ���� �ʿ�

public class Battle3DPlayerUIManager : MonoBehaviour
{
    [Header("References")]
    public Battle3DPlayerController player; // �÷��̾� ��ũ��Ʈ ����
    public Image hpForeground;      // ����� ü�¹� �̹���
    public TextMeshProUGUI hpText;  // ü�� �ؽ�Ʈ (��: 3 / 3)

    void Update()
    {
        // �÷��̾ ����Ǿ� �ִٸ� �� ������ UI�� ������Ʈ�մϴ�.
        if (player != null)
        {
            // 1. ü�¹� ������ ���� (����ü�� / �ִ�ü�� ����)
            hpForeground.fillAmount = player.currentHp / player.maxHp;

            // 2. �ؽ�Ʈ ������Ʈ (�Ҽ��� ���� ������ ǥ��, 0 ���Ϸ� �������� �ʰ� ����)
            int currentHpInt = Mathf.Max(0, (int)player.currentHp);
            int maxHpInt = (int)player.maxHp;

            hpText.text = $"{currentHpInt} / {maxHpInt}";
        }
    }
}

