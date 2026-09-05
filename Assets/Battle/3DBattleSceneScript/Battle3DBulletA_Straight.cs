using UnityEngine;

public class Battle3DBulletA_Straight : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("�Ѿ��� �̵� �ӵ��Դϴ�.")]
    public float speed = 8f;

    [Tooltip("�Ѿ��� ���ư� �����Դϴ�. (�⺻��: �Ʒ���)")]
    public Vector3 direction = Vector3.down;

    [Header("Boundary Settings")]
    [Tooltip("�߽������κ��� �� �Ÿ� �̻� �־����� �ڵ����� �ı��˴ϴ�.")]
    public float destroyDistance = 20f;

    private Rigidbody rb;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        // ����: ���� ���� �ӵ��� ������ �ʰ�, �̹� ������(�Ŵ���)�� �ӵ��� ��ٸ� �װ� �״�� ���ϴ�!
        if (rb != null)
        {
            // ���� �Ŵ����� �ƹ� �ӵ��� �� ��ٸ�(0,0) �⺻ �������� ���ϴ�.
            if (rb.linearVelocity == Vector3.zero)
            {
                rb.linearVelocity = direction.normalized * speed;
            }
        }
    }

    void Update()
    {
        // ���� Rigidbody�� ���ų� Kinematic ��� ��� ���� �̵��� �ʿ��� ��츦 ���� ��� �ڵ�
        if (rb == null)
        {
            transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
        }

        // ���� ����ȭ�� ���� ���� �������� �ʹ� �־����� �ڵ����� �޸𸮿��� �����մϴ�.
        if (Vector3.Distance(startPosition, transform.position) > destroyDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        // �ε��� ����� Player �±׸� ������ �ִٸ�
        if (collision.CompareTag("Player"))
        {
            // �÷��̾��� ��ũ��Ʈ�� �����ɴϴ�. (�̸��� �ٸ��� �� �����ϼ���!)
            Battle3DPlayerController player = collision.GetComponent<Battle3DPlayerController>();

            if (player != null)
            {
                // �÷��̾�� ������ 1�� �����ϴ�. (�Լ� �̸��� �ٸ��� �����ϼ���!)
                player.TakeDamage(1);
            }

        }
    }
}


