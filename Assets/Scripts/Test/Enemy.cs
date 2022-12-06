using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHP = 1.0f; // 최대 HP

    // 아이템 드랍용 데이터
    [System.Serializable]
    public struct ItemDropInfo          // 드랍 아이템 정보
    {
        public ItemIDCode id;           // 아이템 종류
        [Range(0.0f,1.0f)]
        public float dropPercentage;    // 아이템 드랍확률
    }

    void MakeDropItem()
    {
        float percentage = UnityEngine.Random.Range(0.0f, 1.0f);                                // 드랍할 아이템을 결정하기 위한 랜덤 숫자 가져오기
        int index = 0;                                                                          // 드랍할 (내가 가지고 있는) 아이템의 인덱스
        float max = 0;                                                                          // 가장 드랍할 확률이 높은 아이템을 찾기 위한 임시값
        for(int i = 0; i < dropItems.Length; i++)
        {
            if(max < dropItems[i].dropPercentage)
            {
                max = dropItems[i].dropPercentage;                                              // 가장 드랍 확률이 높은 아이템 찾기
                index = i;                                                                      // index의 디폴트 값은 가장 드랍 확률이 높은 아이템
            }
        }

        float checkPercentage = 0.0f;                                                           // 아이템의 드랍 확률을 누적하는 임시 값
        for(int i = 0; i < dropItems.Length; i++)
        {
            checkPercentage += dropItems[i].dropPercentage;                                     // checkPercentage를 단계별로 계속 누적 시킴

            // checkPercentage와 percentage 비교 ( 랜덤 숫자가 누적된 확률보다 낮은지 확인, 낮으면 해당 아이템 생성)
            if(percentage <= checkPercentage)
            {
                index = i;                                                                      // 생성할 아이템 결정
                break;                                                                          // for문 종료
            }
        }

        GameObject obj = ItemFactory.MakeItem(dropItems[index].id, transform.position, true);   // 선택된 아이템 생성
    }

    /// <summary>
    /// 이 몬스터가 드랍할 아이템의 종류
    /// </summary>
    public ItemDropInfo[] dropItems;

#if UNITY_EDITOR
    /// <summary>
    /// 인스펙터 창에서 성공적으로 값이 변경되었을 때 실행
    /// </summary>
    private void OnValidate()
    {
        // 드랍 아이템의 드랍 확률의 합을 1로 만들기
        float total = 0.0f;
        foreach(var item in dropItems)
        {
            total += item.dropPercentage;   // 전체 합 구하기
        }    

        for(int i = 0; i < dropItems.Length; i++)
        {
            dropItems[i].dropPercentage /= total;   // 전체 합으로 나누어서 최종합을 1로 만들기
        }
    }
#endif

}
