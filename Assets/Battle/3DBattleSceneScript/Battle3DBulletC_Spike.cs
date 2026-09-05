using UnityEngine;
using System.Collections;

public class Battle3DBulletC_Spike : MonoBehaviour
{
    // ���� 5f ~ 8f ���� �ӵ��� 15f�� 2�� �̻� ������ �÷Ƚ��ϴ�.
    public float speed = 20f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.left * speed;
    }

    void Update()
    {
        if (rb == null)
            transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

        // ���ð� ȭ�� ���� ��(��: x��ǥ�� -12 ���� �۾�����)���� ������ ������ �����մϴ�.
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
       
        // ����: ���̾��Ű�� �ִ� ���� ���� �̸��� ��Ȯ�� �Ȱ��� �����ּ���! (��: LeftWall)
        if (collision.gameObject.name == "LeftWall")
        {
            Destroy(gameObject);
        }
    }
}
