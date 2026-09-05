using UnityEngine;
using System.Collections;

public class Battle3DBulletB_Laser : MonoBehaviour
{
    public GameObject laserBeamVisual; // �ڽ����� �ִ� �Ͼ�� �簢�� ��������Ʈ ����

    IEnumerator Start()
    {
        laserBeamVisual.SetActive(false);
        // ���� �� 0.2�� ���
        yield return new WaitForSeconds(0.7f);

        // ������ �߻� (�Ͼ�� ĥ�ϱ� & �ݶ��̴� �ѱ�)
        laserBeamVisual.SetActive(true);

        // 0.5�� �� ������ �� ��ü �Ҹ�
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

