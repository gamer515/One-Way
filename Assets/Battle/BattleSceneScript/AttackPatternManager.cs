using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackPatternManager : MonoBehaviour
{
    public BattleStateMachine stateMachine;
    public BattleSceneBattleBoxController boxController;
    public PlayerController player;

    [Header("Bullet Prefabs")]
    public GameObject bulletA_Prefab; // 유도 원형 탄알
    public GameObject bulletB_Prefab; // 레이저 포 (블래스터)
    public GameObject bulletC_Prefab; // 바닥 가시

    // 적의 턴이 시작되면 랜덤하게 3개의 패턴을 실행
    public void StartEnemyTurn()
    {
        StartCoroutine(RunThreePatternsSequence());
    }

    private IEnumerator RunThreePatternsSequence()
    {
        // 패턴 4개 중 서로 다른 3개를 뽑습니다.
        List<int> patternPool = new List<int> { 1, 2, 3, 4 };

        for (int i = 0; i < 3; i++)
        {
            int randIndex = Random.Range(0, patternPool.Count);
            int selectedPattern = patternPool[randIndex];
            patternPool.RemoveAt(randIndex);

            yield return StartCoroutine(ExecutePattern(selectedPattern));

            // 패턴과 패턴 사이 1초 대기
            yield return new WaitForSeconds(1f);
        }

        // 3번의 공격이 끝나면 내 공격 턴으로
        stateMachine.ChangeState(BattleStateMachine.BattleState.PlayerTurn);
    }

    private IEnumerator ExecutePattern(int patternID)
    {
        switch (patternID)
        {
            case 1: // 좌, 하, 우, 상 순서로 6발씩 동시 발사
                boxController.ChangeBox(new Vector2(8f, 8f), Vector2.zero, 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                float offset = 1.2f; // 탄알 사이의 간격

                // 1. 왼쪽에서 6개 동시 생성
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(boxController.leftWall.position.x - 1f, 3f - (offset * i));
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 2. 아래에서 6개 동시 생성
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(-3f + (offset * i), boxController.bottomWall.position.y - 1f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 3. 오른쪽에서 6개 동시 생성
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(boxController.rightWall.position.x + 1f, 3f - (offset * i));
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 4. 위에서 6개 동시 생성
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(-3f + (offset * i), boxController.topWall.position.y + 1f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);
                break;

            case 2: // 1시 방향부터 반시계 레이저 24개 (중심 조준)
                boxController.ChangeBox(new Vector2(8f, 8f), Vector2.zero, 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                float angle = 60f; // 1시 방향 시작
                for (int i = 0; i < 24; i++)
                {
                    // 1. 레이저가 생성될 위치 계산 (반시계로 이동)
                    Vector2 spawnPos = GetPosOnCircle(Vector2.zero, 6f, angle);

                    // 2. 중심(0,0)을 향하는 방향 벡터 계산
                    Vector2 dirToCenter = Vector2.zero - spawnPos;

                    // 3. 방향 벡터를 회전 각도(Z축)로 변환
                    float rotZ = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

                    // 4. 중심을 바라보는 각도로 레이저 생성
                    Instantiate(bulletB_Prefab, spawnPos, Quaternion.Euler(0, 0, rotZ));

                    // 위치는 15도씩 반시계 방향으로 다음 위치로 이동
                    angle += 15f;
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(3f);
                break;

            case 3: // 좌우 상단 레이저 동시 발사 (중심 조준)
                boxController.ChangeBox(new Vector2(8f, 8f), Vector2.zero, 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                // 왼쪽 상단(-5, 5)에서 중심(0,0)을 바라보는 각도는 -45도 입니다.
                Instantiate(bulletB_Prefab, new Vector2(-5f, 5f), Quaternion.Euler(0, 0, -45f));

                // 오른쪽 상단(5, 5)에서 중심(0,0)을 바라보는 각도는 -135도 입니다.
                // (왼쪽 레이저와 정확히 대칭으로 중심을 향해 발사됩니다)
                Instantiate(bulletB_Prefab, new Vector2(5f, 5f), Quaternion.Euler(0, 0, -135f));

                yield return new WaitForSeconds(2f);
                break;

            case 4: // 가로로 긴 상자 & 8개 묶음 가시 1회 발사
                boxController.ChangeBox(new Vector2(16f, 4f), new Vector2(0, -2f), 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Gravity);
                yield return new WaitForSeconds(0.5f); // 상자가 변할 때까지 대기

                int spikeCount = 8;        // 가시 8개
                float spikeSpacing = 0.8f; // 가시 사이 간격

                float startX = boxController.rightWall.position.x + 1f;
                float startY = boxController.bottomWall.position.y + 0.5f;

                // 무한루프(while) 제거! 딱 1번만 8개를 동시에 생성합니다.
                for (int i = 0; i < spikeCount; i++)
                {
                    Vector2 spawnPos = new Vector2(startX + (i * spikeSpacing), startY);
                    Instantiate(bulletC_Prefab, spawnPos, Quaternion.identity);
                }

                // 가시가 왼쪽으로 다 지나갈 시간(약 1.5초 ~ 2초)만 기다리고 패턴을 끝냅니다.
                yield return new WaitForSeconds(2f);
                break;
        }

        ClearBullets(); // 패턴 종료 후 남은 탄막 제거
    }

    private Vector2 GetPosOnCircle(Vector2 center, float radius, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(center.x + radius * Mathf.Cos(rad), center.y + radius * Mathf.Sin(rad));
    }

    private void ClearBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (var b in bullets) Destroy(b);
    }
}