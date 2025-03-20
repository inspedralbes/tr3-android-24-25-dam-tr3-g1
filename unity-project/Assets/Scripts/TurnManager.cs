using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public int currentPlayer = 1;
    public int turn = 1;

    public User player1 = new User(); 
    public User player2 = new User(); 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
    }

    public void NextTurn()
    {
        currentPlayer = currentPlayer == 1 ? 2 : 1;
        turn++;
    }

    public void Reset()
    {
        currentPlayer = 1;
        turn = 1;
    }

    public User GetCurrentPlayer()
    {
        return currentPlayer == 1 ? player1 : player2;
    }

    public string GetTurn()
    {
        return $"Turn {turn}";
    }
}

[System.Serializable]
public class UserListWrapper
{
    public List<User> users;
}
