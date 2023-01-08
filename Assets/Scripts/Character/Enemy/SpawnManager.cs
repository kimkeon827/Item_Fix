using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnManager : MonoBehaviour
{
    public int maxCount;
    public int enemyCount;

    public float spawnTime = 3.0f; //몬스터 스폰
    public float curTime;  

    public Transform[] spawnPoints;
    public bool[] isSpawn;  
    public GameObject enemy;

    public Waypoints waypoints; //

    public static SpawnManager _instance;

    private void Start()
    {
        isSpawn = new bool[spawnPoints.Length]; //스폰 위치 안겹치기
        for( int i = 0; i < isSpawn.Length; i++)
        {
            isSpawn[i] = false;
        }
        _instance = this;

        
    }

    private void Update()
    {
        //몬스터 스폰 조건
        if (curTime >= spawnTime && enemyCount < maxCount)
        {
            int randomSpawn = Random.Range(0, spawnPoints.Length);  //스폰 포인트중 한곳에서 랜덤 생성
            if (!isSpawn[randomSpawn])
            {
                SpawnEnemy(randomSpawn);
            }
        }
        curTime += Time.deltaTime;  //이전 프레임 완료까지 시간 반환 -- 타이머
    }

    public void SpawnEnemy(int randomSpawn)
    {
        curTime = 0;    //스폰 하면 쿨 0
        enemyCount++;   //적 숫자 제한
        GameObject obj = Instantiate(enemy, spawnPoints[randomSpawn]); //몬스터 소환
        Enemy e= obj.GetComponent<Enemy>();
        e.waypoints = waypoints;

        isSpawn[randomSpawn] = true;

    }
}
