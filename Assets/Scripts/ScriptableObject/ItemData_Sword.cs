using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sword Item Data", menuName = "Scriptable Object/Item Data - Sword", order = 3)]
public class ItemData_Sword : ItemData
{
    [Header("�� ������")]
    public float attackPower = 30;
    
}
