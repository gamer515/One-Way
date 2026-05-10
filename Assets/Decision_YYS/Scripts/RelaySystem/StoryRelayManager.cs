using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class StoryRelayManager : MonoBehaviour
{
    [SerializeField] private PromptData promptData;

    public void RelayMidChapter(List<Dialogue> history, int[] stats, int chapter)
    {
        // 1. 'change'가 "true"인 지문만 필터링
        List<Dialogue> filtered = history.FindAll(d => d.change != null && d.change.ToLower() == "true");
        
        // 2. 텍스트 요약 생성
        string summary = BuildSummary(filtered);
        
        // 3. 프롬프트 결합
        string finalPrompt = string.Format(promptData.midTransitionTemplate, summary);
        
        // 4. 패킷 생성
        StoryPacket packet = new StoryPacket("MidTransition", finalPrompt, filtered, stats, chapter);
        
        // 5. 전송 시뮬레이션
        SendPacket(packet);
    }

    public void RelayChapterEnd(List<Dialogue> history, int[] stats, int chapter)
    {
        // 1. 챕터 전체 지문 요약
        string summary = BuildSummary(history);
        
        // 2. 프롬프트 결합
        string finalPrompt = string.Format(promptData.chapterEndTemplate, summary);
        
        // 3. 패킷 생성 (전체 히스토리 포함)
        StoryPacket packet = new StoryPacket("ChapterEnd", finalPrompt, history, stats, chapter);
        
        // 4. 전송 시뮬레이션
        SendPacket(packet);
    }

    private string BuildSummary(List<Dialogue> dialogs)
    {
        if (dialogs == null || dialogs.Count == 0) return "(기록 없음)";
        
        StringBuilder sb = new StringBuilder();
        foreach (var d in dialogs)
        {
            sb.AppendLine($"- [{d.character}] {d.text}");
        }
        return sb.ToString();
    }

    private void SendPacket(StoryPacket packet)
    {
        Debug.Log($"<color=cyan>[StoryRelay]</color> External Packet Sent: {packet.triggerType}");
        Debug.Log($"<color=white><b>Final Prompt:</b></color>\n{packet.finalPrompt}");
        
        // JSON으로 직렬화해서 출력 (외부 모듈이 받게 될 실제 데이터 형태 확인용)
        string json = JsonUtility.ToJson(packet, true);
        Debug.Log($"<color=grey><b>Raw JSON Payload:</b></color>\n{json}");
    }
}
