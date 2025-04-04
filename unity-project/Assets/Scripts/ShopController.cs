using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;

public class ShopController : MonoBehaviour
{
    private string jsonResponse;
    private List<Character> characters = new List<Character>();
    private Button goBack;
    private Label pointsLabel;
    private int playerPoints = 0;

    private int? _userId;
    public int userid
    {
        get
        {
            if (!_userId.HasValue)
            {
                _userId = UserManager.Instance?.CurrentUser.id ?? 0;
            }
            return _userId.Value;
        }
    }

    void Start()
    {
        Debug.Log("Starting ShopController");
        StartCoroutine(FetchPlayerPoints());
        StartCoroutine(FetchCharactersNotBought());
        
        var uiDoc = GetComponent<UIDocument>().rootVisualElement;
        pointsLabel = uiDoc.Q<Label>("PointsLabel");
        goBack = uiDoc.Q<Button>("BackButton");
        goBack.clicked += () => SceneManager.LoadScene("MenuScene");
    }

    private IEnumerator FetchPlayerPoints()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://lordgrids.dam.inspedralbes.cat:4000/users/" + userid + "/points");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error en la petició: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Resposta del servidor per als punts: " + jsonResponse);

            try
            {
                // Deserialitzar el JSON correctament
                var response = JsonConvert.DeserializeObject<PointsResponse>(jsonResponse);
                playerPoints = response.points;  // Assignem el valor correcte
                pointsLabel.text = "Punts: " + playerPoints;
            }
            catch (Exception e)
            {
                Debug.LogError("Error en convertir els punts: " + e.Message);
            }
        }
    }

    // Classe per deserialitzar la resposta JSON
    private class PointsResponse
    {
        public int points;
    }

    private void UpdatePointsUI()
    {
        if (pointsLabel != null)
        {
            pointsLabel.text = "Punts: " + playerPoints;
        }
    }

    private IEnumerator FetchCharactersNotBought()
    {
        UnityWebRequest request = UnityWebRequest.Get("http://lordgrids.dam.inspedralbes.cat:4000/getCharactersNotOwned/" + userid);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            jsonResponse = request.downloadHandler.text;
            var wrapper = JsonConvert.DeserializeObject<CharacterWrapper>("{\"characters\": " + jsonResponse + "}");
            characters = new List<Character>(wrapper.characters);
            
            var uiDoc = GetComponent<UIDocument>().rootVisualElement;
            var cardDisplay = uiDoc.Q<VisualElement>("CardDisplay");

            foreach (var character in characters)
            {
                var card = new VisualElement();
                card.AddToClassList("Card");

                var nameLabel = new Label(character.name);
                nameLabel.AddToClassList("UnitName");
                card.Add(nameLabel);

                var image = new Image();
                StartCoroutine(LoadImage("lordgrids.dam.inspedralbes.cat:4000" + character.icon, image));
                card.Add(image);

                var buyButton = new Button() { text = character.price.ToString() };
                buyButton.AddToClassList("BuyButton");
                buyButton.clicked += () => StartCoroutine(BuyCharacter(character.id, character.price));
                card.Add(buyButton);

                cardDisplay.Add(card);
            }
        }
    }

    private IEnumerator LoadImage(string url, Image image)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            image.image = texture;
        }
    }

    private IEnumerator BuyCharacter(int characterId, int price)
    {
        UnityWebRequest request = new UnityWebRequest("http://lordgrids.dam.inspedralbes.cat:4000/buyCharacter", "POST");
        request.SetRequestHeader("Content-Type", "application/json");

        var requestBody = new
        {
            id_user = userid,
            id_character = characterId
        };

        string json = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Character bought successfully");
            playerPoints -= price;
            UpdatePointsUI();
        }
    }
}
