using TMPro;
using UnityEngine;

// MonoBehaviour이 아닌 일반 클래스로 사용.
public class StatContainer : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI[] stat_Texts; // UI 텍스트 배열

    public int[] stats = new int[4];

    private void Start()
    {
        UpdateSInnitioaltatText();
    }

    private void UpdateSInnitioaltatText()
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i] = 0; // 초기 스탯 값 설정
            stat_Texts[i].text = stats[i].ToString();
        }
    }

    public void UpdateStat(int index)
    {
        if (index >= 0 && index < stats.Length)
        {
            stat_Texts[index].text = stats[index].ToString();
        }
    }
}