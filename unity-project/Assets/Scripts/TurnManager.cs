using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public int currentPlayer = 1;
    public int turn = 1;

    public User player1 = new User(); 
    public User player2 = new User(); 

    private void Start()
    {
        StartCoroutine(FetchUsersFromServer());
    }

    private IEnumerator FetchUsersFromServer()
    {
        UnityWebRequest request = UnityWebRequest.Get($"http://localhost:4000/getPlayingUsers/{UserManager.Instance.CurrentUser.id}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            // Assuming the server returns a JSON array of two users
            UserListWrapper userListWrapper = JsonUtility.FromJson<UserListWrapper>(request.downloadHandler.text);
            if (userListWrapper.users.Count >= 2)
            {
                player1 = userListWrapper.users[0];
                player2 = userListWrapper.users[1];
            }
        }
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
