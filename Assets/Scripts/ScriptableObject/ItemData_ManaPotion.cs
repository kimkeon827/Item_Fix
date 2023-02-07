using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mana Potion", menuName = "Scriptable Object/Item Data - Mana Potion", order = 3)]
public class ItemData_ManaPotion : ItemData, IUsable
{
    [Header("마나포션 데이터")]
    public float totalRegenPoint = 30.0f;   // 전체 회복량
    public float duration = 3.0f;           // 전체 회복 시간

    /// <summary>
    /// 아이템을 사용하면 일어날 일 처리
    /// </summary>
    /// <param name="target">이 아이템의 사용효과를 받을 대상의 게임 오브젝트</param>
    public bool Use(GameObject target = null)
    {
        bool result = false;

        return result;
    }
}
