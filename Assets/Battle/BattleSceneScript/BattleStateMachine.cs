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

    [Header("Dialogues (직접 수정하세요)")]
    public string introDialogue = "* 새들은 지저귀고,\n  꽃들은 피어나고...";
    public string[] randomDialogues = {
        "* 넌 여기서 살아나가지 못할 거다.",
        "* 꽤나 잘 피하는군.",
        "* 뼈가 시리도록 아플 거다.",
        "* ...",
        "* 자비를 베풀 생각은 없나?",
        "* 내 턴이 영원히 안 끝나면 좋겠군.",
        "* (의지가 차오른다.)",
        "* 포기하는 게 어때?"
    };
    public string deathDialogue = "* ...그래.\n  내가 경고했잖아.";

    private bool isEnemyDead = false; // 적 체력이 0이 되었는지 체크하는 변수 (임시)

    void Start()
    {
        ChangeState(BattleState.Intro);
    }

    void Update()
    {
        // 대사가 끝난 후 Space나 Z를 누르면 다음 턴으로 넘어가는 로직
        if ((currentState == BattleState.Intro || currentState == BattleState.MidDialogue) && typewriter.IsTyping == false)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                if (isEnemyDead) ChangeState(BattleState.End);
                else ChangeState(BattleState.EnemyTurn);
            }
        }
    }

    public void ChangeState(BattleState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case BattleState.Intro:
                boxController.SetDialogueMode(0.5f, introDialogue);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                break;

            case BattleState.EnemyTurn:
                // 적 턴 시작 (서로 다른 패턴 3회 연속 실행)
                attackManager.StartEnemyTurn();
                break;

            case BattleState.PlayerTurn:
                Debug.Log("플레이어의 공격 턴 (1회)");
                // TODO: 공격 게이지 바 로직 실행
                // 게이지 바 공격이 끝나면 ChangeState(BattleState.MidDialogue) 호출 필요

                // 임시로 2초 뒤 대화로 넘어가게 처리
                Invoke("EndPlayerTurn", 2f);
                break;

            case BattleState.MidDialogue:
                string randomText = randomDialogues[Random.Range(0, randomDialogues.Length)];
                boxController.SetDialogueMode(0.3f, randomText);
                break;

            case BattleState.End:
                boxController.SetDialogueMode(0.5f, deathDialogue);
                break;
        }
    }

    private void EndPlayerTurn()
    {
        // 만약 내 공격으로 적 체력이 0이 되었다면 isEnemyDead = true; 처리
        ChangeState(BattleState.MidDialogue);
    }
}