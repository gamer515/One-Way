using UnityEngine;

[CreateAssetMenu(fileName = "PromptData", menuName = "ScriptableObjects/PromptData", order = 1)]
public class PromptData : ScriptableObject
{
    [TextArea(5, 10)]
    [Tooltip("중간 전환 시(스탯 도달) 사용할 프롬프트 템플릿입니다.")]
    public string midTransitionTemplate = "중간에 이야기가 끊겼습니다. 지금까지 변화가 있었던 지문들은 다음과 같습니다:\n{0}";

    [TextArea(5, 10)]
    [Tooltip("챕터 종료 시 사용할 프롬프트 템플릿입니다.")]
    public string chapterEndTemplate = "챕터가 종료되었습니다. 전체 요약은 다음과 같습니다:\n{0}";
}
