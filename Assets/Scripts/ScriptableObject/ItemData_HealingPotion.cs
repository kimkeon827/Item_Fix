using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Potion", menuName = "Scriptable Object/Item Data - Healing Potion", order = 2)]
public class ItemData_HealingPotion : ItemData, IUsable
{
    [Header("�������� ������")]
    public float healPoint = 20.0f;

    /// <summary>
    /// �������� ����ϸ� �Ͼ �� ó��
    /// </summary>
    /// <param name="target">�� �������� ���ȿ���� ���� ����� ���� ������Ʈ</param>
    public bool Use(GameObject target = null)
    {
        bool result = false;
        IHealth health = target.GetComponent<IHealth>();    // ���������� HP�� ������ �ִ� target���Ը� ����ȴ�.
        if( health != null)
        {
            float oldHP = health.HP;
            health.HP += healPoint;     // ���� HP ����
            Debug.Log($"{itemName}�� ����߽��ϴ�. HP�� {healPoint}��ŭ �����մϴ�. HP : {health.HP} -> {health.HP += healPoint}");
            result = true;
        }

        return result;
    }
}
