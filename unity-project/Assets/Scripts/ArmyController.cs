using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ArmyController : MonoBehaviour
{
    private string jsonResponse;
    private List<Character> characters;
    private LI_Army userArmy;

    void Start()
    {
        StartCoroutine(FetchCharacters());
    }

    private IEnumerator FetchCharacters()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:4000/characters");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);
            CharacterList characterList = JsonUtility.FromJson<CharacterList>("{\"characters\":" + jsonResponse + "}");
            characters = characterList.characters;
            StartCoroutine(FetchUserArmy());
        }
    }

    private IEnumerator FetchUserArmy()
    {
        //int userId = UserManager.Instance.CurrentUser.id;
        int userId = 1;
        UnityWebRequest request = UnityWebRequest.Get($"http://localhost:4000/armies/{userId}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            jsonResponse = request.downloadHandler.text;
            Debug.Log("Army Response: " + jsonResponse);
            userArmy = JsonUtility.FromJson<LI_Army>(jsonResponse);
            PopulateFoldouts();
        }
    }

    private void PopulateFoldouts()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var dropdowns = root.Query<DropdownField>().ToList();

        for (int i = 0; i < dropdowns.Count; i++)
        {
            dropdowns[i].choices.Clear();

            foreach (var character in characters)
            {
                dropdowns[i].choices.Add(character.name);
            }

            if (dropdowns[i].choices.Count > 0)
            {
                dropdowns[i].value = GetCharacterNameById(GetUnitIdByIndex(i));
            }
        }
    }

    private int GetUnitIdByIndex(int index)
    {
        switch (index)
        {
            case 0: return userArmy.unit1;
            case 1: return userArmy.unit2;
            case 2: return userArmy.unit3;
            case 3: return userArmy.unit4;
            default: return -1;
        }
    }

    private string GetCharacterNameById(int id)
    {
        var character = characters.Find(c => c.id == id);
        return character != null ? character.name : string.Empty;
    }
}
