using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json; // Add this import for Newtonsoft.Json

public class LoginController : MonoBehaviour
{
    // Referències als elements UI Toolkit
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;

    private Button registerButton;

    // URL de l'API on s'envien les credencials
    private const string loginUrl = "http://lordgrids.dam.inspedralbes.cat:4000/login"; 

    private void OnEnable()
    {
        // Obté l'arrel del document UI Toolkit
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Busca els elements pel seu nom definit a l'UXML
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        loginButton = root.Q<Button>("login");
        registerButton = root.Q<Button>("Registrat");

        // Assigna l'event al botó
        loginButton.clicked += OnLoginButtonClicked;
        registerButton.clicked += OnRegisterButtonClicked;
    }

    private void OnDisable()
    {
        // Elimina l'event per evitar problemes de referències després de desactivar el script
        loginButton.clicked -= OnLoginButtonClicked;
    }

    private void OnLoginButtonClicked()
    {
        // Captura el text introduït per l'usuari
        string email = emailField.value;
        string password = passwordField.value;

        // Comença la petició HTTP asíncrona
        StartCoroutine(LoginRequest(email, password));
    }

    private void OnRegisterButtonClicked()
    {
        SceneManager.LoadScene("RegisterScene");
    }

    private IEnumerator LoginRequest(string email, string password)
    {
        // Crea un objecte JSON amb el username i password
        string jsonData = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData); // Converteix el JSON en bytes

        // Configura la petició HTTP amb el mètode POST
        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);  // Envia el cos de la petició
            request.downloadHandler = new DownloadHandlerBuffer(); // Gestiona la resposta
            request.SetRequestHeader("Content-Type", "application/json"); // Defineix el format JSON

            // Envia la petició i espera la resposta
            yield return request.SendWebRequest();

            // Comprova si la petició ha tingut èxit
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Resposta del servidor: " + responseText);

                // Deserialitza la resposta JSON a un objecte User
                User user = JsonUtility.FromJson<User>(responseText);

                // Comprova si la resposta conté l'usuari amb l'email introduït
                if (user != null && user.email == email)
                {
                    // Desa la informació de l'usuari
                    UserManager.Instance.SetUser(user);

                    // Fa una crida addicional per obtenir els personatges de l'usuari
                    string charactersUrl = $"http://lordgrids.dam.inspedralbes.cat:4000/armies/{UserManager.Instance.CurrentUser.id}/characters";
                    using (UnityWebRequest charactersRequest = UnityWebRequest.Get(charactersUrl))
                    {
                        yield return charactersRequest.SendWebRequest();

                        if (charactersRequest.result == UnityWebRequest.Result.Success)
                        {
                            string charactersResponse = charactersRequest.downloadHandler.text;
                            Debug.Log("Resposta dels personatges: " + charactersResponse);

                            // Deserialitza la resposta JSON a una llista de personatges
                            List<Character> characters = JsonConvert.DeserializeObject<List<Character>>(charactersResponse);
                            Debug.Log("Personatges: " + characters[0].name);

                            // Desa els personatges a l'exèrcit del jugador 1
                            TurnManager.Instance.player1.army = characters;
                        }
                        else
                        {
                            Debug.LogError("Error obtenint els personatges: " + charactersRequest.error);
                        }
                    }

                    SceneManager.LoadScene("MenuScene"); // Canvia a l'escena del menú
                }
                else
                {
                    Debug.LogError("Usuari no trobat.");
                }
            }
            else
            {
                Debug.LogError("Error a la connexió: " + request.error);
            }
        }
    }
}
