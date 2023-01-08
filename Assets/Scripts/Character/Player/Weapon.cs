using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    TestPlayer player;

    private void Start()
    {
        player = GameManager.Inst.Player;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("적을 공격했다.");

            IBattle target = other.GetComponent<IBattle>();
            if (target != null)
            {
                player.Attack(target);

                Vector3 impactPoint = transform.position + transform.up;    // 칼 위치 + 칼의 위쪽방향만큼 1만큼 이동
                Vector3 effectPoint = other.ClosestPoint(impactPoint);      // impactPoint에서 칼에 부딪친 컬라이더 위치에 가장 가까운 위치

                //Time.timeScale = 0.01f;
            }
        }
    }
}
