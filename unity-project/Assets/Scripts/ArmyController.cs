using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ArmyController : MonoBehaviour
{
    private string jsonResponse;
    private List<Character> characters;
    private LI_Army userArmy;
    private List<DropdownField> dropdowns;

    void Start()
    {
        

        var root = GetComponent<UIDocument>().rootVisualElement;
        var updateButton = root.Q<Button>("update");
        var playButton = root.Q<Button>("play");
        dropdowns = root.Query<DropdownField>().ToList();

        foreach (var dropdown in dropdowns)
        {
            dropdown.RegisterValueChangedCallback(evt => OnDropdownValueChanged(dropdown));
        }

        updateButton.clicked += OnUpdateButtonClick;
        playButton.clicked += OnPlayButtonClick;

        StartCoroutine(FetchCharacters());
    }

    private void OnDropdownValueChanged(DropdownField dropdown)
    {
        int index = dropdowns.IndexOf(dropdown);
        int characterId = characters.Find(c => c.name == dropdown.value).id;

        switch (index)
        {
            case 0: userArmy.unit1 = characterId; break;
            case 1: userArmy.unit2 = characterId; break;
            case 2: userArmy.unit3 = characterId; break;
            case 3: userArmy.unit4 = characterId; break;
        }

        UpdateStatsLabels();
        Debug.Log($"Dropdown {index} changed to {dropdown.value} (ID: {characterId})");
    }

    private void UpdateStatsLabels()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Label>("unit1Stats").text = GetCharacterStats(userArmy.unit1);
        root.Q<Label>("unit2Stats").text = GetCharacterStats(userArmy.unit2);
        root.Q<Label>("unit3Stats").text = GetCharacterStats(userArmy.unit3);
        root.Q<Label>("unit4Stats").text = GetCharacterStats(userArmy.unit4);
    }

    private string GetCharacterStats(int characterId)
    {
        var character = characters.Find(c => c.id == characterId);
        return character != null ? $"Name: {character.name}, HP: {character.health}, Attack: {character.atk}" : "No character selected";
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
            
            // Wrap the JSON array in an object
            string wrappedJson = "{\"characters\":" + jsonResponse + "}";
            Debug.Log("Wrapped JSON: " + wrappedJson);
            CharacterList characterList = JsonUtility.FromJson<CharacterList>(wrappedJson);
            //Character[] characterArray = JsonUtility.FromJson<Character[]>(wrappedJson);
            Debug.Log("CharacterList: " + JsonUtility.ToJson(characterList));
            //Debug.Log("CharacterArray: " + JsonUtility.ToJson(characterArray));
            characters = characterList.characters;
            Debug.Log("CharacterList: " + characterList.characters);
            StartCoroutine(FetchUserArmy());
        }
    }
    [System.Serializable]
    public class CharacterList
    {
        public List<Character> characters;
    }

    private IEnumerator FetchUserArmy()
    {
        int userId = UserManager.Instance.CurrentUser.id;
        UnityWebRequest request = UnityWebRequest.Get($"http://localhost:4000/armies/{userId}");
        yield return request.SendWebRequest();

        Debug.Log(request.downloadHandler.text);

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
        if (characters == null || userArmy == null)
        {
            Debug.LogError("Characters or userArmy is null");
            return;
        }

        Debug.Log("Dropdowns: " + dropdowns);

        Debug.Log("Characters: " + characters);
        Debug.Log("User army: " + userArmy.unit1 + " " + userArmy.unit2 + " " + userArmy.unit3 + " " + userArmy.unit4);
        for (int i = 0; i < dropdowns.Count; i++)
        {
            dropdowns[i].choices.Clear();
            Debug.Log("Character in JSON format: " + JsonUtility.ToJson(characters));
            foreach (var character in characters)
            {
                Debug.Log("Adding character: " + character);
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

    private void OnUpdateButtonClick()
    {
        Debug.Log("Update button clicked");
        StartCoroutine(UpdateArmy());
    }

    private IEnumerator UpdateArmy()
    {
        int userId = UserManager.Instance.CurrentUser.id;
        string jsonData = JsonUtility.ToJson(userArmy);
        Debug.Log("Updating army: " + jsonData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest($"http://localhost:4000/armies/{userId}", "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Army updated successfully");
            }
            else
            {
                Debug.LogError("Error updating army: " + request.error);
            }
        }
    }

    private void OnPlayButtonClick()
    {
        List<Character> army = new List<Character>();
        foreach (var unitId in new int[] { userArmy.unit1, userArmy.unit2, userArmy.unit3, userArmy.unit4 })
        {
            var character = characters.Find(c => c.id == unitId);
            if (character != null)
            {
                army.Add(character);
            }
        }
        UserManager.Instance.CurrentUser.army = army;
        Debug.Log("Play button clicked");
        SceneManager.LoadScene("PlayScene");
    }
}

