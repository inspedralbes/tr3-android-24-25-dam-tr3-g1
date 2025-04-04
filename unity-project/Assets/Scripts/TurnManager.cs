using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    public int currentPlayer = 1;
    public int turn = 1;

    [SerializeField]
    public User player1;
    public User player2;

    public bool IsArmyReady { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null  && Instance != this)
        {
            Debug.Log("AWAKE::TurnManager instance destroyed");
            Destroy(gameObject);
            
        }else
        {
            Debug.Log("AWAKE::TurnManager instance created");
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        
    }

    private void Start()
    {
        
    }
    public void ChangeTurn(){

        Debug.Log("SENDING CHANGE TURN");
        WebSocketManager.userId userId = new WebSocketManager.userId(){
            id = UserManager.Instance.CurrentUser.id
        };
        WebSocketManager.Instance.changeTurn(userId);
    }
    public void NextTurn(int NextTurn)
    {
        turn = NextTurn;
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

    public void SetArmyReady()
    {
        IsArmyReady = true;
    }
}

[System.Serializable]
public class UserListWrapper
{
    public List<User> users;
}
