using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class StoryRelayManager : MonoBehaviour
{
    [SerializeField] private PromptData promptData;

    /// <summary>
    /// 이야기 데이터를 필터링하고 요약하여 외부로 전송합니다.
    /// </summary>
    /// <param name="triggerType">"MidTransition" 또는 "ChapterEnd"</param>
    public void Relay(string triggerType, List<Dialogue> history, int[] stats, int chapter)
    {
        // 1. 'change'가 "true"인 지문만 필터링 (핵심 지문만 압축)
        List<Dialogue> filtered = history.FindAll(d => d.change != null && d.change.ToLower() == "true");
        
        // 2. 텍스트 요약 생성
        string summary = BuildSummary(filtered);
        
        // 3. 트리거 타입에 따른 템플릿 선택 및 프롬프트 결합
        string template = (triggerType == "MidTransition") 
            ? promptData.midTransitionTemplate 
            : promptData.chapterEndTemplate;
            
        string finalPrompt = string.Format(template, summary);
        
        // 4. 패킷 생성
        StoryPacket packet = new StoryPacket(triggerType, finalPrompt, filtered, stats, chapter);
        
        // 5. 전송 시뮬레이션
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
