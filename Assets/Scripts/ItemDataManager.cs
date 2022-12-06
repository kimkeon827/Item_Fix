using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataManager : MonoBehaviour
{
    /// <summary>
    /// ��� ������ ������(������)
    /// </summary>
    public ItemData[] itemDatas;

    /// <summary>
    /// itemDatas Ȯ�ο� �ε���(Indexer). �迭ó�� ����� �� �ִ� ������Ƽ�� ����
    /// </summary>
    /// <param name="id">itemDatas�� �ε����� ����� ����</param>
    /// <returns>itemDatas�� id��° ������ ������</returns>
    public ItemData this[uint id] => itemDatas[id];

    /// <summary>
    /// itemDatas Ȯ�ο� �ε���
    /// </summary>
    /// <param name="code">Ȯ���� �������� Enum �ڵ�</param>
    /// <returns>code�� ����Ű�� ������</returns>
    public ItemData this[ItemIDCode code] => itemDatas[(int)code];


    /// <summary>
    /// ��ü ������ ������
    /// </summary>
    public int Length => itemDatas.Length;
}
