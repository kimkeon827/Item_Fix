using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMana
{
    float MP { get; set; }  // MP�� Ȯ���ϰ� ������ �� �ִ�.
    float MaxMP { get; }    // �ִ�MP�� Ȯ���� �� �ִ�.

    /// <summary>
    /// MP�� ����� �� ����� ��������Ʈ�� ������Ƽ.
    /// �Ķ���ʹ� (����/�ִ�) ����.
    /// </summary>
    Action<float> onManaChange { get; set; }

    /// <summary>
    /// ������ ���������� ���������ִ� �Լ�. �ʴ� totalRegen/duration ��ŭ�� ȸ��
    /// </summary>
    /// <param name="totalRegen">��ü ȸ����</param>
    /// <param name="duration">���� �ð�</param>
    void ManaRegenerate(float totalRegen, float duration);
}
