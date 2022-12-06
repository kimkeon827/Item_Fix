using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Scriptable Object/Item Data", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("������ �⺻ ������")]
    public uint id = 0;                 // ������ ID
    public string itemName = "������";   // �������� �̸�
    public GameObject modelPrefab;      // �������� ������ ǥ���� ������(����� ���� ��)
    public Sprite itemIcon;             // �������� �κ��丮���� ���� ��������Ʈ
    public uint value;                  // �������� ����
    public uint maxStackCount = 1;      // �κ��丮 ��ĭ�� �� �� �ִ� �ִ� ���� ����
    public string itemDescription;      // �������� �� ����
}
