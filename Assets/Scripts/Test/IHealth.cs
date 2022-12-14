using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    float HP { get; set; }  // HP�� Ȯ���ϰ� ������ �� �ִ�.
    float MaxHP { get; }    // �ִ�HP�� Ȯ���� �� �ִ�.

    /// <summary>
    /// HP�� ����� �� ����� ��������Ʈ�� ������Ƽ.
    /// �Ķ���ʹ� (����/�ִ�) ����.
    /// </summary>
    Action<float> onHealthChange { get; set; }

    void Die();             // �׾��� �� ����� �Լ�

    Action onDie { get; set; }  // �׾��� �� ����� ��������Ʈ�� ������Ƽ
}
