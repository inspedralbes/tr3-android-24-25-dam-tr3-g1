using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
public class ShopController : MonoBehaviour
{
    private string jsonResponse;
    private List<Character> characters = new List<Character>();
    private Button goBack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("estoy empezando");
        StartCoroutine(FetchCharactersNotBought());
        var uiDoc = GetComponent<UIDocument>().rootVisualElement;
        goBack = uiDoc.Q<Button>("BackButton");
        goBack.clicked += () => SceneManager.LoadScene("MenuScene");
    }

    private IEnumerator LoadImage(string url, Image image)
    {
        Debug.Log("loading image");
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


    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator FetchCharactersNotBought()
    {
        Debug.Log("Fetching characters");
        UnityWebRequest request = UnityWebRequest.Get("http://localhost:4000/getCharactersNotOwned/7");
        yield return request.SendWebRequest();
        Debug.Log("Fetched characters");
        Debug.Log(request.result);
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);
            var wrapper = JsonConvert.DeserializeObject<CharacterWrapper>("{\"characters\":" + jsonResponse + "}");
            characters = new List<Character>(wrapper.characters);
            var uiDoc = GetComponent<UIDocument>().rootVisualElement;

            var cardDisplay = uiDoc.Q<VisualElement>("CardDisplay");
            List<Character> charactersToRemove = new List<Character>();
            foreach (var character in characters)
            {
                var card = new VisualElement();
                card.AddToClassList("Card");

                var nameLabel = new Label(character.name);
                nameLabel.AddToClassList("UnitName");
                card.Add(nameLabel);

                var image = new Image();
                StartCoroutine(LoadImage("localhost:4000"+character.icon, image));
                card.Add(image);

                var buyButton = new Button() { text = character.price.ToString() };
                buyButton.AddToClassList("BuyButton");
                buyButton.clicked += () => StartCoroutine(BuyCharacter(character.id));
                card.Add(buyButton);

                cardDisplay.Add(card);

                // Add character to remove list after processing
                charactersToRemove.Add(character);
            }

            // Remove characters after iteration
            foreach (var character in charactersToRemove)
            {
                characters.Remove(character);
            }
        }
    }

    private IEnumerator BuyCharacter(int characterId)
    {
        UnityWebRequest request = new UnityWebRequest("http://localhost:4000/buyCharacter", "POST");
        request.SetRequestHeader("Content-Type", "application/json");

        var requestBody = new
        {
            id_user = 7,
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
            var uiDoc = GetComponent<UIDocument>().rootVisualElement;
            var cardDisplay = uiDoc.Q<VisualElement>("CardDisplay");
            var buttons = cardDisplay.Query<Button>().ToList();

            foreach (var button in buttons)
            {
                if (button.text == characterId.ToString())
                {
                    button.SetEnabled(false);
                    break;
                }
            }

        }
    }
}
