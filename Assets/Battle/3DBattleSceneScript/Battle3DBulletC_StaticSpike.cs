using UnityEngine;

public class Battle3DBulletC_StaticSpike : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        // �ε��� ����� �÷��̾���
        if (collision.CompareTag("Player"))
        {
            Battle3DPlayerController player = collision.GetComponent<Battle3DPlayerController>();

            if (player != null)
            {
                // ������ 1�� �ش�
                player.TakeDamage(1);
            }

            // �÷��̾�� ������� ���� �ı�!
            //Destroy(gameObject);
        }
    }
}

