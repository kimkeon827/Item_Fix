using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    Player player;

    ItemDataManager itemData;

    public Player Player => player;

    public ItemDataManager ItemData => itemData;

    
}
