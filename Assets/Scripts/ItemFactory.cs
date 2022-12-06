using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������� ������ �ϴ� Ŭ����. ���丮 ������ ����
/// </summary>
public class ItemFactory
{
    static int itemCount = 0; // ������ ������ �� ����. ������ ���� ���̵��� ���ҵ� ��.

    /// <summary>
    /// ItemIDCode�� ������ ����
    /// </summary>
    /// <param name="code">������ ������ �ڵ�</param>
    /// <returns>���� ���</returns>
    public static GameObject MakeItem(ItemIDCode code)
    {
        GameObject obj = new GameObject();

        Item item = obj.AddComponent<Item>();           // Item ������Ʈ �߰��ϱ�
        item.data = GameManager.Inst.ItemData[code];

        string[] itemName = item.data.name.Split("_");
        obj.name = $"{itemName[1]}_{itemCount++}";      // ������Ʈ �̸� �����ϱ�
        obj.layer = LayerMask.NameToLayer("Item");      // ���̾� ����

        SphereCollider sc = obj.AddComponent<SphereCollider>(); // �ö��̴� �߰�
        sc.isTrigger = true;
        sc.radius = 0.5f;
        sc.center = Vector3.up;

        return obj;
    }

    /// <summary>
    /// ������ �ڵ带 �̿��� �������� Ư�� ��ġ�� �����ϴ� �Լ�
    /// </summary>
    /// <param name="code">������ ������ �ڵ�</param>
    /// <param name="position">������ ��ġ</param>
    /// <param name="randomNoise">��ġ�� �������� ������ ����. true�� �ణ�� �������� ���Ѵ�. �⺻���� false</param>
    /// <returns>������ ������</returns>
    public static GameObject MakeItem(ItemIDCode code, Vector3 position, bool randomNoise = false)
    {
        GameObject obj = MakeItem(code);    // �����
        if (randomNoise)                    // ��ġ�� �������� ���ϸ�
        {
            Vector2 noise = Random.insideUnitCircle * 0.5f; // ������ 0.5�� ���� ���ʿ��� ������ ��ġ ����
            position.x += noise.x;                          // ���� �������� �Ķ���ͷ� ���� ���� ��ġ�� �߰�
            position.z += noise.y;
        }
        obj.transform.position = position;  // ��ġ����
        return obj;
    }

    /// <summary>
    /// ������ �ڵ带 �̿��� �������� �ѹ��� ������ ����� �Լ�
    /// </summary>
    /// <param name="code">������ �������� ������ �ڵ�</param>
    /// <param name="count">������ ����</param>
    /// <returns>������ �����۵��� ��� �迭</returns>
    public static GameObject[] MakeItems(ItemIDCode code, int count)
    {
        GameObject[] objs = new GameObject[count];  // �迭 �����
        for(int i=0;i<count; i++)
        {
            objs[i] = MakeItem(code);               // count��ŭ �ݺ��ؼ� ������ ����
        }
        return objs;                                // ������ �� ����
    }

    /// <summary>
    /// ������ �ڵ带 �̿��� Ư�� ��ġ�� �������� �ѹ��� ������ �����ϴ� �Լ�
    /// </summary>
    /// <param name="code">������ �������� ������ �ڵ�</param>
    /// <param name="count">������ ����</param>
    /// <param name="position">������ ���� ��ġ</param>
    /// <param name="randomNoise">��ġ�� �������� ������ ����. true�� �ణ�� �������� ���Ѵ�. �⺻���� false</param>
    /// <returns></returns>
    public static GameObject[] MakeItems(ItemIDCode code, int count, Vector3 position, bool randomNoise = false)
    {
        GameObject[] objs = new GameObject[count];  // �迭 �����
        for (int i = 0; i < count; i++)
        {
            objs[i] = MakeItem(code, position, randomNoise);               // count��ŭ �ݺ��ؼ� ������ ����. ��ġ�� ����� ����
        }
        return objs;                                // ������ �� ����
    }

    /// <summary>
    /// ������ �ڵ带 �̿��� �������� �ѹ��� ������ ����� �Լ�
    /// </summary>
    /// <param name="id">������ �������� ������ ���̵�</param>
    /// <param name="count">������ ����</param>
    /// <returns>������ �����۵��� ��� �迭</returns>
    public static GameObject[] MakeItems(int id, int count)
    {
        GameObject[] objs = new GameObject[count];  // �迭 �����
        for (int i = 0; i < count; i++)
        {
            objs[i] = MakeItem(id);               // count��ŭ �ݺ��ؼ� ������ ����
        }
        return objs;                                // ������ �� ����
    }

    /// <summary>
    /// ������ id�� ������ ����
    /// </summary>
    /// <param name="id">������ ������ ID</param>
    /// <returns>������ ������</returns>
    public static GameObject MakeItem(int id)
    {
        if (id < 0)
            return null;

        return MakeItem((ItemIDCode)id);
    }

    /// <summary>
    /// ������ id�� �̿��� Ư�� ��ġ�� �������� �����ϴ� �Լ�
    /// </summary>
    /// <param name="id">������ ������ ���̵�</param>
    /// <param name="position">������ ��ġ</param>
    /// <returns>������ ������</returns>
    public static GameObject MakeItem(int id, Vector3 position, bool randomNoise = false)
    {
        GameObject obj = MakeItem(id);    // �����
        if (randomNoise)                    // ��ġ�� �������� ���ϸ�
        {
            Vector2 noise = Random.insideUnitCircle * 0.5f; // ������ 0.5�� ���� ���ʿ��� ������ ��ġ ����
            position.x += noise.x;                          // ���� �������� �Ķ���ͷ� ���� ���� ��ġ�� �߰�
            position.z += noise.y;
        }
        obj.transform.position = position;  // ��ġ����
        return obj;
    }

    /// <summary>
    /// ������ id�� �̿��� Ư�� ��ġ�� �������� �ѹ��� ������ �����ϴ� �Լ�
    /// </summary>
    /// <param name="id">������ �������� ������ ���̵�</param>
    /// <param name="count">������ ����</param>
    /// <param name="position">������ ���� ��ġ</param>
    /// <param name="randomNoise">��ġ�� �������� ������ ����. true�� �ణ�� �������� ���Ѵ�. �⺻���� false</param>
    /// <returns></returns>
    public static GameObject[] MakeItems(int id, int count, Vector3 position, bool randomNoise = false)
    {
        GameObject[] objs = new GameObject[count];  // �迭 �����
        for (int i = 0; i < count; i++)
        {
            objs[i] = MakeItem(id, position, randomNoise);               // count��ŭ �ݺ��ؼ� ������ ����. ��ġ�� ����� ����
        }
        return objs;                                // ������ �� ����
    }
}
