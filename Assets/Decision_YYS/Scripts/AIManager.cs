using UnityEngine;
using System.Text;

public class AIManager : MonoBehaviour
{
    private static AIManager _instance;
    public static AIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<AIManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AIManager");
                    _instance = go.AddComponent<AIManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 전달받은 패킷의 모든 내용을 색상별로 구분하여 출력합니다.
    /// </summary>
    public void ProcessPacket(StoryPacket packet)
    {
        StringBuilder sb = new StringBuilder();
        
        // 1. 헤더 및 트리거 정보 (연녹색)
        sb.AppendLine($"<color=#42f590><b>[AI SYSTEM - NEW PACKET RECEIVED]</b></color>");
        sb.AppendLine($"<b>Trigger Type:</b> {packet.triggerType}");
        sb.AppendLine($"<b>Target File:</b> {packet.fileName}");
        sb.AppendLine($"<b>Target Chapter Index:</b> {packet.chapterIndex}");
        
        // 2. 현재 스탯 상태 (연두색 계열)
        sb.AppendLine($"\n<color=#a2f542><b>[STATS STATUS]</b></color>");
        if (packet.stats != null && packet.stats.Length >= 4)
        {
            sb.AppendLine($"무력: {packet.stats[0]} | 지력: {packet.stats[1]} | 매력: {packet.stats[2]} | 명성: {packet.stats[3]}");
        }

        // 3. 필터링된 히스토리 요약 (오렌지색)
        sb.AppendLine($"\n<color=#f5a442><b>[STORY HISTORY]</b></color>");
        if (packet.storyHistory != null && packet.storyHistory.Count > 0)
        {
            foreach (var d in packet.storyHistory)
            {
                sb.AppendLine($"- <color=white>[{d.character}]</color> {d.text}");
            }
        }
        else
        {
            sb.AppendLine("<i>(No significant changes recorded)</i>");
        }

        // 4. 최종 프롬프트 내용 (노란색)
        sb.AppendLine($"\n<color=#f5e642><b>[FINAL PROMPT]</b></color>");
        sb.AppendLine(packet.finalPrompt);

        // 5. 원본 JSON 데이터 (회색)
        sb.AppendLine($"\n<color=grey><b>[RAW JSON]</b> {JsonUtility.ToJson(packet)}</color>");

        // [임시 시뮬레이션] AI가 수정을 완료했다고 가정하고 NewStory 파일을 생성합니다.
        SimulateAIAndSave(packet);

        // 최종 통합 로그 출력
        Debug.Log(sb.ToString());
    }

    private void SimulateAIAndSave(StoryPacket packet)
    {
        if (string.IsNullOrEmpty(packet.fileName)) return;

        JsonManager jsonManager = new JsonManager();
        // 1. 원본 데이터 로드
        ScenarioData originalData = jsonManager.LoadData<ScenarioData>(packet.fileName);
        
        if (originalData == null || originalData.MainStory == null) return;

        // 2. 패킷에 포함된 히스토리(수정 대상)들의 텍스트를 "AI가 수정한 것 처럼" 변경
        foreach (var historyItem in packet.storyHistory)
        {
            // 원본 데이터에서 해당 ID를 가진 지문을 찾아 수정
            var targetDialogue = originalData.MainStory.Find(d => d.id == historyItem.id);
            if (targetDialogue != null)
            {
                // 간단한 시뮬레이션: { }로 감싸진 부분을 찾아 [AI수정됨]을 덧붙입니다.
                // 실제 AI는 프롬프트에 따라 문장 전체를 자연스럽게 바꾸겠지만, 테스트를 위해 표식을 남깁니다.
                targetDialogue.text = targetDialogue.text.Replace("{", "{[AI수정됨] ");
                Debug.Log($"[AI Simulation] ID {targetDialogue.id} 지문을 가상으로 수정했습니다.");
            }
        }

        // 3. "NewStory_" 접두사를 붙여서 persistentDataPath에 저장
        // 예: MartialArts/MartialArts_01 -> NewStory_MartialArts_01
        string saveFileName = "NewStory_" + packet.fileName.Replace("/", "_");
        jsonManager.SaveData(originalData, saveFileName);
        
        Debug.Log($"<color=cyan><b>[AI Simulation]</b> 임시 수정본이 저장되었습니다: {saveFileName}</color>");
    }
}
