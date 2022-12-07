using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBattle
{
    Transform transform { get; }

    float AttackPower { get; }

    void Attack(IBattle target);

}
