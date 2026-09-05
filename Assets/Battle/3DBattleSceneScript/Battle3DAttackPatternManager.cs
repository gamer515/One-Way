using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Battle3DAttackPatternManager : MonoBehaviour
{
    public Battle3DBattleStateMachine stateMachine;
    public Battle3DBattleSceneBattleBoxController boxController;
    public Battle3DPlayerController player;

    [Header("Enemy Reference")]
    public GameObject enemyObject;

    [Header("Bullet Prefabs")]
    public GameObject bulletA_Prefab;
    public GameObject bulletA_Straight_Prefab;
    public GameObject bulletB_Prefab;
    public GameObject bulletC_Prefab;
    public GameObject bulletC_Static_Prefab;
    public void StartEnemyTurnSequence()
    {
        StartCoroutine(RunThreePatternsSequence());
    }

    private IEnumerator RunThreePatternsSequence()
    {
        List<int> patternPool = new List<int>();
        int currentBattle = Battle3DBattleStateMachine.BattleIndex;

        if (currentBattle == 1) patternPool.AddRange(new int[] { 1, 2, 3 }); // Bullet A ����
        else if (currentBattle == 2) patternPool.AddRange(new int[] { 4, 5, 6 }); // Bullet C ����
        else patternPool.AddRange(new int[] { 7, 8, 9 }); // Bullet B ����

        for (int i = 0; i < 3; i++)
        {
            int randIndex = Random.Range(0, patternPool.Count);
            int selectedPattern = patternPool[randIndex];
            patternPool.RemoveAt(randIndex);

            // ����: ù ��° ������ �� ������ (0, -4) �߾����� �ű�� ����
            // ��, 4�� ����(����)ó�� �˾Ƽ� �÷��̾� ��ġ�� �ű�� ����� �ִ� ������ �����մϴ�.
            // (���� 5, 6���� �߾� �̵��� �ʿ� ���ٸ� selectedPattern != 4 && selectedPattern != 5 ... ������ �߰��ϸ� �˴ϴ�.)
            if (i == 0 && selectedPattern != 4)
            {
                player.transform.position = new Vector3(0f, -4f, 0f);
                Rigidbody pRb = player.GetComponent<Rigidbody>();
                if (pRb != null) pRb.linearVelocity = Vector3.zero;
            }

            yield return StartCoroutine(ExecutePattern(selectedPattern));

            // ���ϰ� ���� ������ ª�� �޽� �ð�
            yield return new WaitForSeconds(1f);
        }

        // 3���� ������ ������ �� ���� ������ ��ȯ
        stateMachine.ChangeState(Battle3DBattleStateMachine.BattleState.PlayerTurn);
    }

    private IEnumerator ExecutePattern(int patternID)
    {
        Debug.Log($"[Pattern System] ���� {patternID}�� �۵� ����!");

        switch (patternID)
        {
            case 1: // 4���� ���� �߻� (���ο� �߽��� ����)
                Vector3 boxCenter1 = new Vector3(0f, -4f, 0f); // ���� �ڽ��� �߽�
                boxController.ChangeBox(new Vector3(8f, 8f, 0f), boxCenter1, 0.5f);
                player.SetMovementMode(Battle3DPlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                float offset = 1.2f;

                // 1. ���ʿ��� ���� (Y��ǥ�� boxCenter ���� ����)
                for (int i = 0; i < 6; i++)
                {
                    Vector3 pos = new Vector3(boxController.leftWall.position.x - 1f, boxCenter1.y + 3f - (offset * i), 0f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 2. �Ʒ����� ���� (X��ǥ�� boxCenter ���� ����)
                for (int i = 0; i < 6; i++)
                {
                    Vector3 pos = new Vector3(boxCenter1.x - 3f + (offset * i), boxController.bottomWall.position.y - 1f, 0f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 3. �����ʿ��� ����
                for (int i = 0; i < 6; i++)
                {
                    Vector3 pos = new Vector3(boxController.rightWall.position.x + 1f, boxCenter1.y + 3f - (offset * i), 0f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);

                // 4. ������ ����
                for (int i = 0; i < 6; i++)
                {
                    Vector3 pos = new Vector3(boxCenter1.x - 3f + (offset * i), boxController.topWall.position.y + 1f, 0f);
                    Instantiate(bulletA_Prefab, pos, Quaternion.identity);
                }
                yield return new WaitForSeconds(2f);
                break;

            case 2: // ���ʿ��� ���������� ���� ������� ���̺� (�߰� �̻� ����)
                // 1. ���� ũ�� ���� (���� 8 ����)
                boxController.ChangeBox(new Vector3(8f, 8f, 0f), new Vector3(0f, -4f, 0f), 0.5f);
                yield return new WaitForSeconds(0.5f);

                int waveCount = 4; // �� 2�� �պ� (�� 6�� �ҿ�)
                float spawnInterval = 0.12f;
                float startX = -15f; // ���� ���� X ��ǥ

                // 2. 1��(Top)���� 6��(Bottom)������ Y��ǥ 6�� �̸� ���
                // ���� �߽��� (0, -4)�̰� ���̰� 8�̹Ƿ�, ���� Y��ǥ�� �뷫 -0.5 ~ -7.5
                float[] yPos = new float[6];
                float topY = -0.5f;
                float botY = -7.5f;
                for (int i = 0; i < 6; i++)
                {
                    // i�� 0�̸� 1��(Top), i�� 5�� 6��(Bottom)
                    yPos[i] = Mathf.Lerp(topY, botY, i / 5f);
                }

                // 3. ������� ���� ����
                for (int w = 0; w < waveCount; w++)
                {
                    // [1] 1��(Top) ���� ����
                    SpawnWaveBullet(startX, yPos[0]);
                    yield return new WaitForSeconds(spawnInterval);

                    // [2] 2~5�� �������鼭 ���� (�� �� 1���� �������� ���� ����)
                    int skipDown = Random.Range(1, 5); // 1, 2, 3, 4 �� �ϳ� ���� �̱�
                    for (int i = 1; i <= 4; i++)
                    {
                        if (i != skipDown) SpawnWaveBullet(startX, yPos[i]);
                        yield return new WaitForSeconds(spawnInterval); // ��� �� ��� 0.3�� ���� ����
                    }

                    // [3] 6��(Bottom) ���� ����
                    SpawnWaveBullet(startX, yPos[5]);
                    yield return new WaitForSeconds(spawnInterval);

                    // [4] 5~2�� �ö󰡸鼭 ���� (�� �� 1���� �������� ���� ����)
                    int skipUp = Random.Range(1, 5);
                    for (int i = 4; i >= 1; i--)
                    {
                        if (i != skipUp) SpawnWaveBullet(startX, yPos[i]);
                        yield return new WaitForSeconds(spawnInterval);
                    }
                }

                // ���� ���� �� �Ѿ��� ȭ�� ������ �� ���� ������ ���
                yield return new WaitForSeconds(2.5f);
                break;

            case 3: // ������ �Ʒ��� �������� �� (2ĭ �� ������ �¿�� �̵�)
                // ���� �� �� �����
                if (enemyObject != null) enemyObject.SetActive(false);

                boxController.ChangeBox(new Vector3(8f, 8f, 0f), new Vector3(0f, -4f, 0f), 0.5f);
                yield return new WaitForSeconds(0.5f);

                int rowCount = 12; // �� 12��(��) ���
                float spawnDelay = 0.2f; // 0.3�� ����
                float startY = 8f; // ���� ���� Y ��ǥ

                // 1. 1��(Left)���� 6��(Right)������ X��ǥ 6�� �̸� ���
                // ���� �߽��� 0�̰� �ʺ� 8�̹Ƿ�, ���� X��ǥ�� ���� �ְ� -3.5 ~ 3.5�� ����ϴ�.
                float[] xPos = new float[6];
                float leftX = -3.5f;
                float rightX = 3.5f;
                for (int i = 0; i < 6; i++)
                {
                    xPos[i] = Mathf.Lerp(leftX, rightX, i / 5f);
                }

                // 2. ������ ��ȣ(����)�� ���� �ε��� �迭 (0���� ����)
                // 1�� (2,3�� ����), 2�� (3,4�� ����), 3�� (4,5�� ����)�� �ǹ��մϴ�.
                int[] holeSequence = { 1, 2, 3, 2 };

                // 3. ���� ����
                for (int r = 0; r < rowCount; r++)
                {
                    // �̹� �ٿ��� ������ ���۵� ��ġ�� �迭���� ������� �����ɴϴ�.
                    int currentHoleStart = holeSequence[r % holeSequence.Length];

                    for (int i = 0; i < 6; i++)
                    {
                        // ���� �ڸ��� ���� �������̰ų� �� ���� ��(�� 2ĭ)�̸� �ǳʶݴϴ�!
                        if (i == currentHoleStart || i == currentHoleStart + 1)
                            continue;

                        // �� ������ �ƴϸ� �Ѿ� ����
                        SpawnVerticalBullet(xPos[i], startY);
                    }

                    // �� ���� �� ����� 0.3�� ���
                    yield return new WaitForSeconds(spawnDelay);
                }

                // ���� ���� �� �Ѿ��� ȭ�� ������ �� ���� ������ ���
                yield return new WaitForSeconds(2.5f);

                // ���� ���� �� �� �ٽ� ��Ÿ����
                if (enemyObject != null) enemyObject.SetActive(true);
              
                break;

            case 4: // ���η� �� ���� & �ε巯�� ������ �̵� & 8�� ���� ����
                // 1. ���� ũ�� ���� ����
                    boxController.ChangeBox(new Vector3(16f, 4f, 0f), new Vector3(0, -4f, 0f), 0.5f);
                    yield return new WaitForSeconds(0.5f); // ���ڰ� �� ���� ������ ���

                    // 2. �÷��̾� ���� �̵� ���� (���� ���)
                    player.isControlLocked = true; // ��Ʈ�� ����

                    Rigidbody pRb = player.GetComponent<Rigidbody>();
                    pRb.linearVelocity = Vector3.zero; // ���� �� ���ֱ�
                    pRb.useGravity = false; // 이동 중에는 중력을 끕니다.

                    Vector3 startPos = player.transform.position; // ���� ��ġ
                    Vector3 targetPos = new Vector3(boxController.leftWall.position.x + 1.5f, boxController.bottomWall.position.y + 0.8f, 0f); // ��ǥ ��ġ (���� �Ʒ�)

                    float moveDuration = 0.4f; // 0.4�� ���� ��! �ϰ� �̵�
                    float elapsed = 0f;

                    // ��ǥ ��ġ�� �ε巴�� ����
                    while (elapsed < moveDuration)
                    {
                        elapsed += Time.deltaTime;
                        // Vector3.Lerp�� �������� ������ �ð��� ���� �ε巴�� �̾��ݴϴ�.
                        player.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
                        yield return null; // ���� �����ӱ��� ���
                    }

                    player.transform.position = targetPos; // ���� ���� ���� ��ġ�� �� ����

                    // 3. �̵� �Ϸ�! �ٽ� �߷� ��� �Ѱ� ���� ��� ����
                    player.SetMovementMode(Battle3DPlayerController.MovementMode.Gravity);
                    player.isControlLocked = false;

                    yield return new WaitForSeconds(0.2f); // ���� ������ �� ����� ��ġ ���� �ð�

                    // 4. ���� ������ ���� (������Ʈ��)
                    int spikeCount = 8;

                    // ������: ������ �� ��ġ�� ����
                    float case4StartX = boxController.rightWall.position.x;
                    float case4StartY = boxController.bottomWall.position.y + 0.5f;

                    GameObject lastSpike = null;

                    for (int i = 0; i < spikeCount; i++)
                    {
                        // X��ǥ �̵� ��� ����: �׻� ���� �ڸ�(startX)���� �����˴ϴ�.
                        Vector3 spawnPos = new Vector3(case4StartX, case4StartY, 0f);

                        // ���� ���� �� ������ ���� ����
                        lastSpike = Instantiate(bulletC_Prefab, spawnPos, Quaternion.identity);

                        // 0.05�� �������� ������ ���� ����
                        yield return new WaitForSeconds(0.05f);
                    }

                    // 8���� ��� ������ ����, 
                    // ������ ����(lastSpike)�� ���� ���� ��� ����� ������ ���� ���
                    while (lastSpike != null)
                    {
                        yield return null;
                    }

                    break;

            case 5: // ������ �������� ���� (���� �� ���� ��� -> ���� ���� �ٴ� ���� ��ġ)
                {
                    //���� �� �� �����
                    if (enemyObject != null) enemyObject.SetActive(false);

                    // 1. [����] ���� ũ�� ���� (�ʺ� 8, ���� 10, �߽� 0, -2)
                    boxController.ChangeBox(new Vector3(9f, 12f, 0f), new Vector3(0f, -2f, 0f), 0.5f);
                    yield return new WaitForSeconds(0.5f);

                    // 2. �÷��̾ ���� ������� ���� �̵� (���� ���� �� ���߷�)
                    player.transform.position = new Vector3(0f, 1f, 0f);
                    player.isControlLocked = false;
                    player.SetMovementMode(Battle3DPlayerController.MovementMode.Free);

                    Rigidbody case5Rb = player.GetComponent<Rigidbody>();
                    if (case5Rb != null)
                    {
                        case5Rb.useGravity = false;
                        case5Rb.linearVelocity = Vector3.zero;
                    }

                    // 3. 3�� ���� ���� ������ ���ð� �����Ǿ� ���� �ö󰩴ϴ�.
                    float fallDuration = 3.0f;
                    float wallSpawnDelay = 0.1f;
                    float elapsedFall = 0f;
                    Vector3 upVelocity = new Vector3(0f, 8f, 0f);

                    while (elapsedFall < fallDuration)
                    {
                        // ���� �� ����
                        Vector3 case5LeftPos = new Vector3(boxController.leftWall.position.x + 0.5f, boxController.bottomWall.position.y - 0f, 0f);
                        SpawnMovingSpike(case5LeftPos, -90f, upVelocity);

                        // ������ �� ����
                        Vector3 case5RightPos = new Vector3(boxController.rightWall.position.x - 0.5f, boxController.bottomWall.position.y - 0f, 0f);
                        SpawnMovingSpike(case5RightPos, 90f, upVelocity);

                        elapsedFall += wallSpawnDelay;
                        yield return new WaitForSeconds(wallSpawnDelay);
                    }

                    // 4. 3�� ����! �� ���õ��� ���߰� 4�� �� �ı� ����
                    foreach (GameObject spike in activeWallSpikes)
                    {
                        if (spike != null)
                        {
                            Rigidbody spikeRb = spike.GetComponent<Rigidbody>();
                            if (spikeRb != null)
                            {
                                spikeRb.linearVelocity = Vector3.zero;
                            }
                            Destroy(spike, 2.0f);
                        }
                    }
                    activeWallSpikes.Clear();

                    // 5. �߷� ��带 �ٽ� �Ѽ� �ٴ����� �߶���ŵ�ϴ�.
                    if (case5Rb != null)
                    {
                        case5Rb.useGravity = true;
                    }
                    player.SetMovementMode(Battle3DPlayerController.MovementMode.Gravity);
                    player.isControlLocked = false;

                    // 6. [��û ���� �ݿ�] ���ѵ� 6f ���� ���� ���� 4���� �� ���� 1�� ��ġ
                    int totalSlots = 4; // ���� 4�� + �� ���� 1�� = �� 5ĭ
                    int safeHoleIndex = Random.Range(0, totalSlots); // 0~4 �� �������� �������� ����

                    float case5StartX = -3f; // �糡 1f�� ������ ���� 6f ������ ���� X��ǥ (-4f + 1f)
                    float spacing = 2f; // 6f ������ 5�� ĭ���� ������ ���� ���� (6f / 4)

                    for (int i = 0; i < totalSlots; i++)
                    {
                        // ���� ���õ� �ε����� �� �����̹Ƿ� ���ø� �������� �ʰ� �н�!
                        if (i == safeHoleIndex) continue;

                        // �ٴڿ� ������ ���� ����
                        Vector3 bottomPos = new Vector3(case5StartX + (i * spacing), boxController.bottomWall.position.y + 0.5f, 0f);
                        GameObject staticSpike = Instantiate(bulletC_Static_Prefab, bottomPos, Quaternion.identity);

                        // 4�� �� �ڵ� �ı�
                        Destroy(staticSpike, 2.0f);
                    }

                    // ���� ���� ��ȯ ��� �ð�
                    yield return new WaitForSeconds(2.2f);

                    //  ���� ���� �� �� �ٽ� ��Ÿ����
                    if (enemyObject != null) enemyObject.SetActive(true);

                    break;
                }

            case 6: // �߷� ���� (���� ���� ����)
                {
                    // 1. ���� ũ�� ���� (�ʺ� 10, ���� 8)
                    boxController.ChangeBox(new Vector3(10f, 8f, 0f), new Vector3(0f, -4f, 0f), 0.5f);
                    yield return new WaitForSeconds(0.5f);

                    //[��û �ݿ�] ���� ���� �� ���� ���� �� ���߷�(Free) ���� ��ȯ
                    player.isControlLocked = false;
                    player.SetMovementMode(Battle3DPlayerController.MovementMode.Free);

                    Rigidbody case6Rb = player.GetComponent<Rigidbody>();
                    if (case6Rb != null)
                    {
                        case6Rb.useGravity = false; // 중력 제거
                        case6Rb.linearVelocity = Vector3.zero; // ���� �ӵ� �ʱ�ȭ
                    }

                    int dashCount = 3;           // �� 3�� ������
                    float centerDashTime = 0.2f; // ù �߾� �̵� �ð�
                    float wallDashTime = 0.15f;  // ������ �������� �ð� (�ſ� ����)

                    for (int i = 0; i < dashCount; i++)
                    {
                        // [��û �ݿ�] ���� 'ù ��°(i == 0)' ������ ���� �÷��̾ �߾����� �̵���ŵ�ϴ�.
                        // �� ��°, �� ��° ���������� �� �ܰ踦 �ǳʶٰ� �ٷ� ���� ������ �̵��մϴ�.
                        if (i == 0)
                        {
                            player.isControlLocked = true; // �̵� �� ���� ���
                            yield return StartCoroutine(MovePlayerTo(new Vector3(0f, -4f, 0f), centerDashTime));
                            yield return new WaitForSeconds(0.1f); // ƨ��� �� ª�� ����
                        }

                        // 2. ���� �� ���� (0: Top, 1: Bottom, 2: Left, 3: Right)
                        int wallIndex = Random.Range(0, 4);
                        Vector3 case6TargetPos = Vector3.zero;
                        float case6Offset = 0.8f; // ���� ���� �ʱ� ���� ����

                        if (wallIndex == 0) case6TargetPos = new Vector3(player.transform.position.x, boxController.topWall.position.y - case6Offset, 0f);
                        else if (wallIndex == 1) case6TargetPos = new Vector3(player.transform.position.x, boxController.bottomWall.position.y + case6Offset, 0f);
                        else if (wallIndex == 2) case6TargetPos = new Vector3(boxController.leftWall.position.x + case6Offset, player.transform.position.y, 0f);
                        else if (wallIndex == 3) case6TargetPos = new Vector3(boxController.rightWall.position.x - case6Offset, player.transform.position.y, 0f);

                        // 3. ��! ���� ��ġ���� �ش� �� �������� ��ٷ� ��������
                        player.isControlLocked = true; // �з����� ���� ���� ���
                        yield return StartCoroutine(MovePlayerTo(case6TargetPos, wallDashTime));

                        // 4. ���� ���ڸ��� ��� ������ ������ (0.2�� ���� Ż���ؾ� ��!)
                        player.isControlLocked = false;

                        // 5. ��ӵ� 0.2���� ����(����) �ð�
                        yield return new WaitForSeconds(0.5f);

                        // 6. ������ �ִ� ����(Static)���� �ش� ������ ������ ����
                        List<GameObject> wallSpikes = new List<GameObject>();
                        int spikeAmount = 7; // �� �� �鿡 ������ ���� ����

                        if (wallIndex == 0) // ���� �� (�Ʒ��� ���� Ʀ)
                        {
                            float case6StartX = boxController.leftWall.position.x + 0.5f;
                            float spacing = (boxController.rightWall.position.x - boxController.leftWall.position.x - 1f) / (spikeAmount - 1);
                            for (int j = 0; j < spikeAmount; j++)
                            {
                                Vector3 pos = new Vector3(case6StartX + (j * spacing), boxController.topWall.position.y - 0.5f, 0f);
                                wallSpikes.Add(Instantiate(bulletC_Static_Prefab, pos, Quaternion.Euler(0, 0, 180f)));
                            }
                        }
                        else if (wallIndex == 1) // �Ʒ��� �� (���� ���� Ʀ)
                        {
                            float case6StartX = boxController.leftWall.position.x + 0.5f;
                            float spacing = (boxController.rightWall.position.x - boxController.leftWall.position.x - 1f) / (spikeAmount - 1);
                            for (int j = 0; j < spikeAmount; j++)
                            {
                                Vector3 pos = new Vector3(case6StartX + (j * spacing), boxController.bottomWall.position.y + 0.5f, 0f);
                                wallSpikes.Add(Instantiate(bulletC_Static_Prefab, pos, Quaternion.identity));
                            }
                        }
                        else if (wallIndex == 2) // ���� �� (�������� ���� Ʀ)
                        {
                            float case6StartY = boxController.bottomWall.position.y + 0.5f;
                            float spacing = (boxController.topWall.position.y - boxController.bottomWall.position.y - 1f) / (spikeAmount - 1);
                            for (int j = 0; j < spikeAmount; j++)
                            {
                                Vector3 pos = new Vector3(boxController.leftWall.position.x + 0.5f, case6StartY + (j * spacing), 0f);
                                wallSpikes.Add(Instantiate(bulletC_Static_Prefab, pos, Quaternion.Euler(0, 0, -90f)));
                            }
                        }
                        else if (wallIndex == 3) // ������ �� (������ ���� Ʀ)
                        {
                            float case6StartY = boxController.bottomWall.position.y + 0.5f;
                            float spacing = (boxController.topWall.position.y - boxController.bottomWall.position.y - 1f) / (spikeAmount - 1);
                            for (int j = 0; j < spikeAmount; j++)
                            {
                                Vector3 pos = new Vector3(boxController.rightWall.position.x - 0.5f, case6StartY + (j * spacing), 0f);
                                wallSpikes.Add(Instantiate(bulletC_Static_Prefab, pos, Quaternion.Euler(0, 0, 90f)));
                            }
                        }

                        // 7. ���ð� �����Ǵ� �ð� (�����ļ� ���߾� �ϴ� �ð�)
                        yield return new WaitForSeconds(0.6f);

                        // 8. ����ϰ� ���� ȸ��(����)
                        foreach (GameObject spike in wallSpikes)
                        {
                            if (spike != null) Destroy(spike);
                        }

                        // ���� ������ �ĳ��� ���� ���� ª�� �޽� ����
                        yield return new WaitForSeconds(0.2f);
                    }

                    // ��� ���� ������ ������ ���� �������� �Ѿ�� �� ���� ��� �ð�
                    yield return new WaitForSeconds(1.5f);
                    break;
                }

            case 7: // �ݽð� ������ (���ο� �߽��� ����)
                Vector3 boxCenter2 = new Vector3(0f, -4f, 0f);
                boxController.ChangeBox(new Vector3(8f, 8f, 0f), boxCenter2, 0.5f);
                player.SetMovementMode(Battle3DPlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                float angle = 60f;
                for (int i = 0; i < 24; i++)
                {
                    // 1. Vector3.zero ��� boxCenter2�� �������� ���� ��ǥ ���
                    Vector3 spawnPos = GetPosOnCircle(boxCenter2, 6f, angle);

                    // 2. Vector3.zero ��� boxCenter2�� �ٶ󺸵��� ���� ���� ���
                    Vector3 dirToCenter = boxCenter2 - spawnPos;

                    float rotZ = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;
                    Instantiate(bulletB_Prefab, spawnPos, Quaternion.Euler(0, 0, rotZ));

                    angle += 15f;
                    yield return new WaitForSeconds(0.1f);
                }
                yield return new WaitForSeconds(3f);
                break;
            case 8: // ����� ���� ������ (���ο� �߽��� ����)
                Vector3 boxCenter3 = new Vector3(0f, -4f, 0f);
                boxController.ChangeBox(new Vector3(8f, 8f, 0f), boxCenter3, 0.5f);
                player.SetMovementMode(Battle3DPlayerController.MovementMode.Free);
                yield return new WaitForSeconds(0.5f);

                // �߽������� ���� ���/���� ������� �������� ���� ��ġ�� ����ϴ�.
                Vector3 leftPos = boxCenter3 + new Vector3(-5f, 5f, 0f);
                Vector3 rightPos = boxCenter3 + new Vector3(5f, 5f, 0f);

                Instantiate(bulletB_Prefab, leftPos, Quaternion.Euler(0, 0, -45f));
                Instantiate(bulletB_Prefab, rightPos, Quaternion.Euler(0, 0, -135f));

                yield return new WaitForSeconds(2f);
                break;
            case 9: // ���� ������(BulletB) 4���� ���� (���� ���)
                {
                    // 1. ���� ũ�� ���� (�⺻ ũ��: �ʺ� 8, ���� 8)
                    boxController.ChangeBox(new Vector3(8f, 8f, 0f), new Vector3(0f, -4f, 0f), 0.5f);
                    yield return new WaitForSeconds(0.5f);

                    // 2. �÷��̾ ���� ����(Free) �� ���߷� ���·� ��ȯ
                    player.isControlLocked = false;
                    player.SetMovementMode(Battle3DPlayerController.MovementMode.Free);

                    Rigidbody case9Rb = player.GetComponent<Rigidbody>();
                    if (case9Rb != null)
                    {
                        case9Rb.useGravity = false; // 중력 제거
                        case9Rb.linearVelocity = Vector3.zero; // ������ �޴� �� �ʱ�ȭ
                    }

                    // 3. ���� ������
                    int repeatCount = 4; // �� 4�� ����
                    float case9SpawnDelay = 0.6f; // ���� ���� 0.6��
                    float spawnX = -17f; // X��ǥ�� ���� ������ ����

                    // 4���� ������ Y��ǥ �迭
                    float[] possibleYPos = { -2f, -3f, -4f, -5f, -6f, -7f };

                    // 4. ������ ���� ����
                    for (int i = 0; i < repeatCount; i++)
                    {
                        // ������ 4���� Y��ǥ �� �ϳ��� �������� �̽��ϴ�.
                        int randomIndex = Random.Range(0, possibleYPos.Length);
                        float spawnY = possibleYPos[randomIndex];

                        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

                        // BulletB ���� (�������� �ٶ󺸵��� ȸ���� 0���� Quaternion.identity ���)
                        // ���� �������� ���� ���� �ִٸ� Quaternion.Euler(0, 0, -90f) �� �������ּ���!
                        Instantiate(bulletB_Prefab, spawnPos, Quaternion.identity);

                        // ���� �������� 0.6�� ���
                        yield return new WaitForSeconds(case9SpawnDelay);
                    }

                    // ���� ���� �� ��� �������� ���� ������ �˳��� ���
                    yield return new WaitForSeconds(2.0f);
                    break;
                }
        }
    }

    // ���� ��ǥ ����� �Լ� ����
    private Vector3 GetPosOnCircle(Vector3 center, float radius, float angleInDegrees)
    {
        float ptX = center.x + radius * Mathf.Cos(angleInDegrees * Mathf.Deg2Rad);
        float ptY = center.y + radius * Mathf.Sin(angleInDegrees * Mathf.Deg2Rad);
        return new Vector3(ptX, ptY, 0f);
    }


    // 2�� ���� ���� �Ѿ� ���� �Լ�
    private void SpawnWaveBullet(float x, float y)
    {
        // ������ ��ġ�� �Ѿ� ����
        GameObject bullet = Instantiate(bulletA_Straight_Prefab, new Vector3(x, y, 0f), Quaternion.identity);

        // �߿�: ���� bulletA_Prefab ��ü�� �÷��̾ ���󰡴�(Homing) ��ũ��Ʈ�� �����ִٸ�, 
        // ���⼭ �� ��ũ��Ʈ�� ���ִ� �ڵ尡 �ʿ��� �� �ֽ��ϴ�. 
        // ��: bullet.GetComponent<HomingScript>().enabled = false;

        // ���ʿ��� ���������θ� �����ϵ��� �ӵ� ���� �ο�
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector3(8f, 0f, 0f); // 8�� �ӵ��� ������ �̵� (�׽�Ʈ �� �Ը��� �°� �����ϼ���)
        }
    }

    // 3�� ���� ����
    private void SpawnVerticalBullet(float x, float y)
    {
        GameObject bullet = Instantiate(bulletA_Straight_Prefab, new Vector3(x, y, 0f), Quaternion.identity);

        // ����->������ ���� ���������� Homing ��ũ��Ʈ�� �ִٸ� ��Ȱ��ȭ ���ּ���.
        // ��: bullet.GetComponent<HomingScript>().enabled = false;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // X�� �̵��� ����, Y������ -8f�� �ӵ��� �������� �մϴ�.
            rb.linearVelocity = new Vector3(0f, -8f, 0f);
        }
    }


    // 5�� ���� ����
    //  [����] ���� ȭ�鿡 �����Ǿ� ���ư��� �ִ� �� ���õ��� �����ϴ� ����Ʈ
    private List<GameObject> activeWallSpikes = new List<GameObject>();

    private void SpawnMovingSpike(Vector3 pos, float rotZ, Vector3 velocity)
    {
        GameObject spike = Instantiate(bulletC_Static_Prefab, pos, Quaternion.Euler(0, 0, rotZ));

        Rigidbody rb = spike.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        //  ������ ���ø� ���� ����Ʈ�� �߰��մϴ�.
        activeWallSpikes.Add(spike);

        StartCoroutine(DestroySpikeAtTopWall(spike));
    }
    //5�� ���� ����
    private IEnumerator DestroySpikeAtTopWall(GameObject spike)
    {
        while (spike != null)
        {
            if (spike.transform.position.y >= boxController.topWall.position.y)
            {
                //  �ı��Ǳ� ���� ����Ʈ���� �����ϰ� �����մϴ�.
                if (activeWallSpikes.Contains(spike))
                {
                    activeWallSpikes.Remove(spike);
                }

                Destroy(spike);
                yield break;
            }

            yield return null;
        }
    }


    // 6�� ���� ����: ������ ��ǥ ��ġ���� �ε巴�� ������� �Լ�
    private IEnumerator MovePlayerTo(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = player.transform.position;
        float elapsed = 0f;

        // �������� ���ȿ��� �߷��̳� �ٸ� ���� ���� �������� �ʵ��� �ӵ��� 0���� �����ϴ�.
        Rigidbody pRb = player.GetComponent<Rigidbody>();
        if (pRb != null)
        {
            pRb.linearVelocity = Vector3.zero;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Vector3.Lerp�� �̿��� �������� ���� ���̸� ���������� ����մϴ�.
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        // �������� ��ǥ ��ġ�� ���� ���� ��Ȯ�� ����ϴ�.
        player.transform.position = targetPosition;
    }
}
