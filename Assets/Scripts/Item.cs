using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ 1���� ǥ���� Ŭ����
/// </summary>
public class Item : MonoBehaviour
{
    public ItemData data;   // �������� ����

    private void Start()
    {
        Instantiate(data.modelPrefab, transform.position,transform.rotation, transform);    // �������� ���� �߰�
    }
}
