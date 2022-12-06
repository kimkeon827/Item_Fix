using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHP = 1.0f; // �ִ� HP

    // ������ ����� ������
    [System.Serializable]
    public struct ItemDropInfo          // ��� ������ ����
    {
        public ItemIDCode id;           // ������ ����
        [Range(0.0f,1.0f)]
        public float dropPercentage;    // ������ ���Ȯ��
    }

    void MakeDropItem()
    {
        float percentage = UnityEngine.Random.Range(0.0f, 1.0f);                                // ����� �������� �����ϱ� ���� ���� ���� ��������
        int index = 0;                                                                          // ����� (���� ������ �ִ�) �������� �ε���
        float max = 0;                                                                          // ���� ����� Ȯ���� ���� �������� ã�� ���� �ӽð�
        for(int i = 0; i < dropItems.Length; i++)
        {
            if(max < dropItems[i].dropPercentage)
            {
                max = dropItems[i].dropPercentage;                                              // ���� ��� Ȯ���� ���� ������ ã��
                index = i;                                                                      // index�� ����Ʈ ���� ���� ��� Ȯ���� ���� ������
            }
        }

        float checkPercentage = 0.0f;                                                           // �������� ��� Ȯ���� �����ϴ� �ӽ� ��
        for(int i = 0; i < dropItems.Length; i++)
        {
            checkPercentage += dropItems[i].dropPercentage;                                     // checkPercentage�� �ܰ躰�� ��� ���� ��Ŵ

            // checkPercentage�� percentage �� ( ���� ���ڰ� ������ Ȯ������ ������ Ȯ��, ������ �ش� ������ ����)
            if(percentage <= checkPercentage)
            {
                index = i;                                                                      // ������ ������ ����
                break;                                                                          // for�� ����
            }
        }

        GameObject obj = ItemFactory.MakeItem(dropItems[index].id, transform.position, true);   // ���õ� ������ ����
    }

    /// <summary>
    /// �� ���Ͱ� ����� �������� ����
    /// </summary>
    public ItemDropInfo[] dropItems;

#if UNITY_EDITOR
    /// <summary>
    /// �ν����� â���� ���������� ���� ����Ǿ��� �� ����
    /// </summary>
    private void OnValidate()
    {
        // ��� �������� ��� Ȯ���� ���� 1�� �����
        float total = 0.0f;
        foreach(var item in dropItems)
        {
            total += item.dropPercentage;   // ��ü �� ���ϱ�
        }    

        for(int i = 0; i < dropItems.Length; i++)
        {
            dropItems[i].dropPercentage /= total;   // ��ü ������ ����� �������� 1�� �����
        }
    }
#endif

}
