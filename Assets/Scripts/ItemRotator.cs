using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ��ũ��Ʈ�� ���� ������Ʈ�� y���� �������� ��� ȸ��(�ð����)�ϸ鼭 ���Ʒ��� �ö󰬴� �������� �Ѵ�(�ﰢ�Լ� Ȱ��).
public class ItemRotator : MonoBehaviour
{
    public float rotateSpeed = 360.0f;  // ������Ʈ�� ȸ�� �ӵ�
    public float minHeight = 0.5f;      // ������Ʈ�� ���� ���� ����
    public float maxHeight = 1.5f;       // ������Ʈ�� ���� ���� ����

    float timeElapsed;                  // ��ü ���� �ð�(cos�� ����� �뵵)
    float halfDiff;                     // ��� ĳ�̿�
    Vector3 newPosition;                // �������� ���ο� ��ġ

    private void Start()
    {
        newPosition = transform.position;   // ���� ��ġ�� newPosition�� ����
        newPosition.y = minHeight;          // newPosition�� y ���� ���� ���� ���̰����� ����        
        transform.position = newPosition;   // ������Ʈ�� ��ġ�� newPosition���� ����

        timeElapsed = 0.0f;                 // �ð� ������ �ʱ�ȭ
        halfDiff = 0.5f * (maxHeight - minHeight);  // ĳ�̿� ��� ��� ����
    }

    private void Update()
    {
        //Mathf.Deg2Rad * 180.0f; // 1����
        //Mathf.Rad2Deg * pi; // 180��

        timeElapsed += Time.deltaTime * 2;  // �ð��� ��� ������Ŵ
        newPosition.x = transform.parent.position.x;    
        newPosition.z = transform.parent.position.z;
        newPosition.y = minHeight + (1 - Mathf.Cos(timeElapsed)) * halfDiff;    // ���̰��� cos �׷����� �̿��� ���
        transform.position = newPosition;                                       // ����� ���� newPosition���� ��ġ �ű��

        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);   // ���ڸ����� ���ۺ��� ������
    }
}
