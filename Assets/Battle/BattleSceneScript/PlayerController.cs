using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum MovementMode { Free, Gravity }

    [Header("Status")]
    public float maxHp = 20f;
    public float currentHp;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public MovementMode currentMode = MovementMode.Free;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHp = maxHp;
    }

    void FixedUpdate()
    {
        if (currentMode == MovementMode.Free) MoveFree();
        else MoveGravity();
    }

    void Update()
    {
        // 중력 모드일 때 지면에 닿아있으면 점프 (W 키 또는 위쪽 화살표)
        if (currentMode == MovementMode.Gravity && isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false;
            }
        }
    }

    private void MoveFree()
    {
        // WASD 또는 방향키 입력을 받아옵니다.
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(h, v);

        rb.gravityScale = 0;
        rb.linearVelocity = inputDir.normalized * moveSpeed;
    }

    private void MoveGravity()
    {
        // 좌우 이동 값만 받아옵니다.
        float h = Input.GetAxisRaw("Horizontal");

        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);
    }

    public void SetMovementMode(MovementMode mode)
    {
        currentMode = mode;
        if (mode == MovementMode.Free)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        Debug.Log($"피격! 남은 HP: {currentHp}");
        if (currentHp <= 0) Debug.Log("플레이어 사망"); // 추후 사망 처리
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BattleBox")) isGrounded = true;
    }
}