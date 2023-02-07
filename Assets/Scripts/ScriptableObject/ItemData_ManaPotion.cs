using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mana Potion", menuName = "Scriptable Object/Item Data - Mana Potion", order = 3)]
public class ItemData_ManaPotion : ItemData, IUsable
{
    [Header("�������� ������")]
    public float totalRegenPoint = 30.0f;   // ��ü ȸ����
    public float duration = 3.0f;           // ��ü ȸ�� �ð�

    /// <summary>
    /// �������� ����ϸ� �Ͼ �� ó��
    /// </summary>
    /// <param name="target">�� �������� ���ȿ���� ���� ����� ���� ������Ʈ</param>
    public bool Use(GameObject target = null)
    {
        bool result = false;

        return result;
    }
}
