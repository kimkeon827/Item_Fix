using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton< GameManager >
{
    // 변수 ---------------------------------------------------------------------------------------

    /// <summary>
    /// 플레이어
    /// </summary>
    TestPlayer player;


    // 프로퍼티 ------------------------------------------------------------------------------------

    /// <summary>
    /// player 읽기 전용 프로퍼티.
    /// </summary>
    public TestPlayer Player => player;

    // 함수 ---------------------------------------------------------------------------------------

    /// <summary>
    /// 게임 메니저가 새로 만들어지거나 씬이 로드 되었을 때 실행될 초기화 함수
    /// </summary>
    protected override void Initialize()
    {
        

        player = FindObjectOfType<TestPlayer>();
        
    }
}


