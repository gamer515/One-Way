using TMPro;
using UnityEngine;
using System;

public class StatContainer : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI[] stat_Texts;

    public int[] stats = new int[4];

    [Header("Transition Settings")]
    [SerializeField] private int targetStatThreshold = 10;
    public event Action OnTargetStatReached;

    private void Start()
    {
        // 단순히 현재 stats 배열의 값을 UI에 반영합니다.
        // 저장된 데이터가 있다면 그 값이 유지됩니다.
        RefreshAllUI();
    }

    public void UpdateStat(int index)
    {
        if (index >= 0 && index < stats.Length)
        {
            stat_Texts[index].text = stats[index].ToString();

            // 목표 수치 도달 여부 체크
            if (stats[index] >= targetStatThreshold)
            {
                OnTargetStatReached?.Invoke();
            }
        }
    }

    public void RefreshAllUI()
    {
        for (int i = 0; i < stats.Length; i++)
        {
            stat_Texts[i].text = stats[i].ToString();
        }
    }
}