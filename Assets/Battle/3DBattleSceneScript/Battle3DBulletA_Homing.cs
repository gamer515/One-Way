using UnityEngine;
using System.Collections;

// ź�� A (�÷��̾� ���� ����ź)
public class Battle3DBulletA_Homing : MonoBehaviour
{
    public float speed = 8f;
    private Vector3 direction;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject player = GameObject.Find("Player"); // �÷��̾� �̸��� �°� ����
        if (player != null)
        {
            direction = (player.transform.position - transform.position).normalized;
        }

        if (rb != null)
            rb.linearVelocity = direction * speed;
    }

    void Update()
    {
        if (rb == null)
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}
