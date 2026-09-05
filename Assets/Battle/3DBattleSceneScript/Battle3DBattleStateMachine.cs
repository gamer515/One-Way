using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Battle3DBattleStateMachine : MonoBehaviour
{
    public enum BattleState { Intro, EnemyTurn, PlayerTurn, MidDialogue, End }
    public BattleState currentState;

    public static int BattleIndex
    {
        get => PlayerPrefs.GetInt("CurrentBattleIndex", 1);
        set => PlayerPrefs.SetInt("CurrentBattleIndex", value);
    }

    //  [�߰�] �ν����Ϳ��� ���� ���� ������ '��� ��Ʈ' ����ü�Դϴ�.
    [System.Serializable]
    public struct BattleDialogueSet
    {
        [TextArea(2, 3)] public string introText; // ���� ���
        [TextArea(2, 3)] public string midText;   // �߰� ���
        [TextArea(2, 3)] public string endText;   // ��(������) ���
    }

    [Header("References")]
    public Battle3DBattleSceneBattleBoxController boxController;
    public Battle3DPlayerController player;
    public Battle3DAttackPatternManager attackManager;
    public TypewriterEffect typewriter;
    public Battle3DAttackGaugeManager gaugeManager;

    //  [����] ������ �ִ� introText, randomTexts, winText ���� 3���� �����ּ���!

    //  [�߰�] B1, B2, B3 ��縦 ���� 3ĭ¥�� �迭�� ����ϴ�.
    [Header("Stage Dialogues")]
    public BattleDialogueSet[] stageDialogues = new BattleDialogueSet[3];

    [Header("Enemy Status")]
    public float enemyHp = 3f;

    private float inputTimer = 0f;

    

    void Start()
    {
        // [Ȯ�ο� �α�] ���� �� ���簡 �� ��° ���� ������ �ֿܼ� ����մϴ�.
        Debug.Log($"[Battle System] ���� ������ ���� ��������: B{BattleIndex}");
        ChangeState(BattleState.Intro);
    }

    void Update()
    {
        if (inputTimer > 0) inputTimer -= Time.deltaTime;

        if (IsDialogueState() && !typewriter.IsTyping && inputTimer <= 0)
        {
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
            {
                AdvanceFromDialogue();
            }
        }
    }

    private bool IsDialogueState() => currentState == BattleState.Intro || currentState == BattleState.MidDialogue || currentState == BattleState.End;

    private BattleDialogueSet GetCurrentDialogue()
    {
        // BattleIndex�� 1���� �����ϹǷ�, �迭 �ε���(0, 1, 2)�� ���߱� ���� 1�� ���ݴϴ�.
        // Mathf.Clamp�� �Ἥ �Ǽ��� �ε����� ������ ����� ������ ���� �ʰ� ����մϴ�.
        int index = Mathf.Clamp(BattleIndex - 1, 0, stageDialogues.Length - 1);
        return stageDialogues[index];
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;

        if (IsDialogueState()) inputTimer = 1.0f;
        else inputTimer = 0.2f;

        // ���� ��Ʋ ��������(B1, B2, B3)�� �´� ��� �ٷ��̸� �ҷ��ɴϴ�.
        BattleDialogueSet currentDiag = GetCurrentDialogue();

        switch (currentState)
        {
            case BattleState.Intro:
                player.SetVisible(false);
                // ����: ���� ���������� ���� ��� ���
                boxController.SetDialogueMode(0.5f, currentDiag.introText);
                break;

            case BattleState.EnemyTurn:
                typewriter.StopAndClear();
                player.SetVisible(true);
                attackManager.StartEnemyTurnSequence();
                break;

            case BattleState.PlayerTurn:
                typewriter.StopAndClear();
                player.SetVisible(false);
                boxController.SetGaugeMode(0.3f);
                gaugeManager.StartGauge();
                break;

            case BattleState.MidDialogue:
                player.SetVisible(false);
                // ����: ���� ��� ���� ���������� �߰� ��� ���
                boxController.SetDialogueMode(0.3f, currentDiag.midText);
                break;

            case BattleState.End:
                player.SetVisible(false);
                // ����: ���� ���������� ������ ��� ���
                boxController.SetDialogueMode(0.5f, currentDiag.endText);
                break;
        }
    }

    private void AdvanceFromDialogue()
    {
        if (currentState == BattleState.Intro || currentState == BattleState.MidDialogue)
        {
            ChangeState(BattleState.EnemyTurn);
        }
        else if (currentState == BattleState.End)
        {
            // [����] ������ ��簡 ������ �ڷ�ƾ�� ���� �̾߱� ������ �����մϴ�.
            StartCoroutine(ReturnToDecisionScene());
        }
    }

    // [�߰�] ���� ���� �� ���� �� �������� ī��Ʈ ���� ����
    private IEnumerator ReturnToDecisionScene()
    {
        Debug.Log("���� �¸�! 1.5�� �� �̾߱� ������ ���ư��ϴ�.");
        yield return new WaitForSeconds(1.5f);

        // ���� ���� ������ ������ ���� B2, B3�� �ǵ��� �ε����� 1 ������Ű�� �����մϴ�.
        BattleIndex++;

        // �̾߱� ��(������ DecisionScene)���� ��ȯ�մϴ�.
        SceneManager.LoadScene("TempDecisionScene");
    }

    public void OnPlayerAttackComplete(float damage)
    {
        enemyHp -= damage;
        if (enemyHp <= 0)
        {
            ChangeState(BattleState.End);
        }
        else
        {
            ChangeState(BattleState.MidDialogue);
        }
    }
}

