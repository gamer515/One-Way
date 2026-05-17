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
    public void StartEnemyTurnSequence()
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

            // 핵심: 첫 번째 순서(i == 0)이고, 4번 패턴(가시)이 아닐 때만 중앙으로 강제 이동!
            if (i == 0 && selectedPattern != 4)
            {
                player.transform.position = new Vector2(0f, -4f); // 상자 정중앙
                Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
                if (pRb != null) pRb.linearVelocity = Vector2.zero;
            }

            // 패턴 실행
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
            case 1: // 4방향 동시 발사 (새로운 중심점 기준)
                Vector2 boxCenter1 = new Vector2(0f, -4f); // 현재 박스의 중심
                boxController.ChangeBox(new Vector2(8f, 8f), boxCenter1, 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                float offset = 1.2f;

                // 1. 왼쪽에서 생성 (Y좌표를 boxCenter 기준 보정)
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(boxController.leftWall.position.x - 1f, boxCenter1.y + 3f - (offset * i));
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 2. 아래에서 생성 (X좌표를 boxCenter 기준 보정)
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(boxCenter1.x - 3f + (offset * i), boxController.bottomWall.position.y - 1f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 3. 오른쪽에서 생성
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(boxController.rightWall.position.x + 1f, boxCenter1.y + 3f - (offset * i));
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 4. 위에서 생성
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(boxCenter1.x - 3f + (offset * i), boxController.topWall.position.y + 1f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);
                break;

            case 2: // 반시계 레이저 (새로운 중심점 조준)
                Vector2 boxCenter2 = new Vector2(0f, -4f);
                boxController.ChangeBox(new Vector2(8f, 8f), boxCenter2, 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                float angle = 60f;
                for (int i = 0; i < 24; i++)
                {
                    // 1. Vector2.zero 대신 boxCenter2를 기준으로 원형 좌표 계산
                    Vector2 spawnPos = GetPosOnCircle(boxCenter2, 6f, angle);

                    // 2. Vector2.zero 대신 boxCenter2를 바라보도록 방향 벡터 계산
                    Vector2 dirToCenter = boxCenter2 - spawnPos;

                    float rotZ = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;
                    Instantiate(bulletB_Prefab, spawnPos, Quaternion.Euler(0, 0, rotZ));

                    angle += 15f;
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(3f);
                break;

            case 3: // 양방향 동시 레이저 (새로운 중심점 기준)
                Vector2 boxCenter3 = new Vector2(0f, -4f);
                boxController.ChangeBox(new Vector2(8f, 8f), boxCenter3, 0.5f);
                player.SetMovementMode(PlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                // 중심점에서 좌측 상단/우측 상단으로 오프셋을 더해 위치를 잡습니다.
                Vector2 leftPos = boxCenter3 + new Vector2(-5f, 5f);
                Vector2 rightPos = boxCenter3 + new Vector2(5f, 5f);

                Instantiate(bulletB_Prefab, leftPos, Quaternion.Euler(0, 0, -45f));
                Instantiate(bulletB_Prefab, rightPos, Quaternion.Euler(0, 0, -135f));

                yield return new WaitForSeconds(2f);
                break;

            case 4: // 가로로 긴 상자 & 부드러운 일직선 이동 & 8개 묶음 가시
                // 1. 상자 크기 조절 시작
                boxController.ChangeBox(new Vector2(16f, 4f), new Vector2(0, -4f), 0.5f);
                yield return new WaitForSeconds(0.5f); // 상자가 다 변할 때까지 대기

                // 2. 플레이어 강제 이동 시작 (조작 잠금)
                player.isControlLocked = true; // 컨트롤 끄기

                Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
                pRb.linearVelocity = Vector2.zero; // 가던 힘 없애기
                pRb.gravityScale = 0f; // 이동 중에 바닥으로 떨어지지 않게 중력 잠깐 무시

                Vector2 startPos = player.transform.position; // 현재 위치
                Vector2 targetPos = new Vector2(boxController.leftWall.position.x + 1.5f, boxController.bottomWall.position.y + 0.8f); // 목표 위치 (왼쪽 아래)

                float moveDuration = 0.4f; // 0.4초 동안 슉! 하고 이동
                float elapsed = 0f;

                // 목표 위치로 부드럽게 당기기
                while (elapsed < moveDuration)
                {
                    elapsed += Time.deltaTime;
                    // Vector2.Lerp로 시작점과 끝점을 시간에 따라 부드럽게 이어줍니다.
                    player.transform.position = Vector2.Lerp(startPos, targetPos, elapsed / moveDuration);
                    yield return null; // 다음 프레임까지 대기
                }

                player.transform.position = targetPos; // 오차 없이 최종 위치에 딱 맞춤

                // 3. 이동 완료! 다시 중력 모드 켜고 조작 잠금 해제
                player.SetMovementMode(PlayerController.MovementMode.Gravity);
                player.isControlLocked = false;

                yield return new WaitForSeconds(0.2f); // 가시 나오기 전 잠깐의 눈치 게임 시간

                // 4. 가시 순차적 출현 (업데이트됨)
                int spikeCount = 8;

                // 시작점: 오른쪽 벽 위치로 고정
                float startX = boxController.rightWall.position.x;
                float startY = boxController.bottomWall.position.y + 0.5f;

                GameObject lastSpike = null;

                for (int i = 0; i < spikeCount; i++)
                {
                    // X좌표 이동 계산 삭제: 항상 같은 자리(startX)에서 생성됩니다.
                    Vector2 spawnPos = new Vector2(startX, startY);

                    // 가시 생성 및 마지막 가시 갱신
                    lastSpike = Instantiate(bulletC_Prefab, spawnPos, Quaternion.identity);

                    // 0.05초 간격으로 빠르게 연속 생성
                    yield return new WaitForSeconds(0.05f);
                }

                // 8개가 모두 생성된 이후, 
                // 마지막 가시(lastSpike)가 왼쪽 벽에 닿아 사라질 때까지 무한 대기
                while (lastSpike != null)
                {
                    yield return null;
                }

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