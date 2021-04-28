using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject go = new GameObject("Game Manager");
                go.AddComponent<GameManager>();
            }

            return _instance;
        }
    }
    public int _coins;
    public bool isDead;

    public void Awake()
    {
        _instance = this;
        _coins = 50;
    }

}
