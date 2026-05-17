using UnityEngine;

public class BattleStateMachine : MonoBehaviour
{
    public enum BattleState { Intro, EnemyTurn, PlayerTurn, MidDialogue, End }
    public BattleState currentState;

    [Header("References")]
    public BattleSceneBattleBoxController boxController;
    public PlayerController player;
    public AttackPatternManager attackManager;
    public TypewriterEffect typewriter;
    public AttackGaugeManager gaugeManager;

    [Header("Dialogues")]
    public string introText = "* 당신은 죄악이 등을 타고\n  오르는 것을 느꼈다.";
    public string[] randomTexts = {
        "* 샌즈가 당신에게 자비를\n  베풀고 있다.",
        "* 공기가 차갑다.",
        "* 넌 오늘 여기서 끝이다.",
        "* (침묵이 흐른다.)",
        "* 아직도 포기하지 않았나?",
        "* 다음은 더 아플 거다.",
        "* 빗나갔군.",
        "* ...헤헤."
    };
    public string winText = "* ...결국 이렇게 되는군.\n* 가서 밥이나 먹어야겠어.";

    [Header("Enemy Status")]
    public float enemyHp = 3f;
    //private bool isEnemyDead = false;

    // 입력 중복 방지를 위한 타이머
    private float inputTimer = 0f;

    void Start() => ChangeState(BattleState.Intro);

    void Update()
    {
        // 타이머 차감
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

    public void ChangeState(BattleState newState)
    {
        currentState = newState;
        inputTimer = 0.2f;

        switch (currentState)
        {
            case BattleState.Intro:
                player.SetVisible(false); // 대화(Intro) 시작 시 숨기기
                boxController.SetDialogueMode(0.5f, introText);
                break;

            case BattleState.EnemyTurn:
                player.SetVisible(true);  // 적 공격 패턴(회피) 시작 시 보이기
                attackManager.StartEnemyTurnSequence();
                break;

            case BattleState.PlayerTurn:
                player.SetVisible(true);  // 내 공격 턴(게이지바) 시작 시 보이기
                boxController.SetGaugeMode(0.3f);
                gaugeManager.StartGauge();
                break;

            case BattleState.MidDialogue:
                player.SetVisible(false); // 중간 대화 시작 시 숨기기
                string randomText = randomTexts[Random.Range(0, randomTexts.Length)];
                boxController.SetDialogueMode(0.3f, randomText);
                break;

            case BattleState.End:
                player.SetVisible(false); // 마무리 대화 시 숨기기
                boxController.SetDialogueMode(0.5f, winText);
                break;
        }
    }

    private void AdvanceFromDialogue()
    {
        if (currentState == BattleState.Intro || currentState == BattleState.MidDialogue)
            ChangeState(BattleState.EnemyTurn);
        else if (currentState == BattleState.End)
            Debug.Log("전투 승리!");
    }

    public void OnPlayerAttackComplete(float damage)
    {
        enemyHp -= damage;
        if (enemyHp <= 0)
        {
            //isEnemyDead = true;
            ChangeState(BattleState.End);
        }
        else
        {
            ChangeState(BattleState.MidDialogue);
        }
    }
}