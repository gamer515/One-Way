using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyPattern", menuName = "One-Way/Enemy Pattern")]
public class Battle3DEnemyPatternData : ScriptableObject
{
    [Header("Pattern Settings")]
    public string patternName;
    public float patternDuration = 5f;

    [Header("Box Configurations")]
    public Vector3 targetBoxSize = new Vector3(10f, 6f, 0f);
    public Vector3 targetBoxPos = new Vector3(0f, 0f, 0f);

    [Header("Player Constraints")]
    public Battle3DPlayerController.MovementMode playerMovementMode; // Free �Ǵ� Gravity

    [Header("Attack Logic")]
    public int patternRoutineID; // Battle3DAttackPatternManager���� ������ ���� ��ȣ (1~4)
}

