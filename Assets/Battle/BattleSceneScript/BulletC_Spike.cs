using UnityEngine;
using System.Collections;

public class BulletC_Spike : MonoBehaviour
{
    // 기존 5f ~ 8f 였던 속도를 15f로 2배 이상 빠르게 올렸습니다.
    public float speed = 20f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);

        // 가시가 화면 왼쪽 밖(예: x좌표가 -12 보다 작아지면)으로 나가면 스스로 삭제합니다.
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}