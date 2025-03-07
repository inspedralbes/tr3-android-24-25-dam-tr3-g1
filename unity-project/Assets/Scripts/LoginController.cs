using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
public class LoginController : MonoBehaviour
{
    // Referències als elements UI Toolkit
    private TextField emailField;
    private TextField passwordField;
    private Button loginButton;

    // URL de l'API on s'envien les credencials
    private const string loginUrl = "http://localhost:4000/login"; 

    private void OnEnable()
    {
        // Obté l'arrel del document UI Toolkit
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Busca els elements pel seu nom definit a l'UXML
        emailField = root.Q<TextField>("email");
        passwordField = root.Q<TextField>("password");
        loginButton = root.Q<Button>("login");

        // Assigna l'event al botó
        loginButton.clicked += OnLoginButtonClicked;
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

                // Comprova si la resposta conté l'usuari amb l'email introduït
                if (responseText.Contains("\"email\":\"" + email + "\""))
                {
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
